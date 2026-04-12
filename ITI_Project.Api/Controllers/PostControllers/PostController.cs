using AutoMapper;
using ITI_Project.Api.DTO.Post;
using ITI_Project.Api.DTO.Posts;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Posts;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications.PostSpecs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.PostControllers
{
    public class PostController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;

        public PostController(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("get-recent-posts")]
        public async Task<ActionResult<IReadOnlyList<PostDTO>>> GetAllPosts([FromQuery] PostSpecParams specParams)
        {
            var posts = await unitOfWork.Repository<Post>()
                .GetAllWithSpecAsync(new PostsWithPaginationAndFiltersSpecification(specParams));

            var count = await unitOfWork.Repository<Post>()
                .GetCountAsync(new CountPostsWithFilterSpecification(specParams));

            var data = mapper.Map<List<PostDTO>>(posts);

            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (int.TryParse(clientIdClaim, out var clientId))
            {
                var postIds = data.Select(p => p.Id).ToList();
                var reactions = await unitOfWork.Repository<PostReaction>()
                    .GetManyByConditionAsync(r => r.ClientId == clientId && postIds.Contains(r.ServicePostId)) ?? new List<PostReaction>();

                var reactionByPostId = reactions.ToDictionary(r => r.ServicePostId, r => r.ReactionType);

                foreach (var post in data)
                    post.UserReaction = reactionByPostId.TryGetValue(post.Id, out var reaction) ? reaction : null;
            }

            return Ok(new Pagination<PostDTO>(specParams.PageIndex, specParams.PageSize, count, data));
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("add-post")]
        public async Task<ActionResult> AddPost([FromForm] PostFromUserDTO dto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var client = await unitOfWork.Repository<Client>().GetByIdAsync(clientId);
            if (client == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            if(client.GovernorateId == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Client must have a governorate to create a post"));

            var uploadedPaths = new List<string>();

            if (dto.Images is { Count: > 0 })
            {
                foreach (var image in dto.Images)
                {
                    var uploadResult = await fileStorageService.UploadFileAsync(image, "post-images", User);
                    if (!uploadResult.Success)
                    {
                        foreach (var path in uploadedPaths)
                            fileStorageService.DeleteFile(path);

                        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, uploadResult.Message));
                    }

                    uploadedPaths.Add(uploadResult.FilePath!);
                }
            }

            var post = mapper.Map<Post>(dto);
            post.ClientId = clientId;

            post.GovernorateId = client.GovernorateId.Value;
            if (client.RegionId.HasValue)
                post.RegionId = client.RegionId.Value;
            post.CreatedAt = DateHelper.GetNowInEgypt();

            try
            {
                await unitOfWork.Repository<Post>().AddAsync(post);
                await unitOfWork.CompleteAsync();

                if (uploadedPaths.Count > 0)
                {
                    var postImages = uploadedPaths.Select(path => new PostImage
                    {
                        ImageUrl = path,
                        PostId = post.Id,
                        Post = post
                    }).ToList();

                    await unitOfWork.Repository<PostImage>().AddRangeAsync(postImages);
                    await unitOfWork.CompleteAsync();
                }

                return Ok(new ApiResponse(StatusCodes.Status201Created, "Post created successfully"));
            }
            catch
            {
                foreach (var path in uploadedPaths)
                    fileStorageService.DeleteFile(path);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while creating the post"));
            }
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpDelete("delete-post/{postId:int}")]
        public async Task<ActionResult> DeletePost(int postId)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var post = await unitOfWork.Repository<Post>().GetByIdAsync(postId);
            if (post is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Post not found"));

            if (post.ClientId != clientId)
                return Forbid();

            try
            {
                var postImages = await unitOfWork.Repository<PostImage>()
                    .GetManyByConditionAsync(pi => pi.PostId == postId) ?? new List<PostImage>();

                foreach (var image in postImages)
                    fileStorageService.DeleteFile(image.ImageUrl);

                unitOfWork.Repository<Post>().Delete(post);
                await unitOfWork.CompleteAsync();

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Post deleted successfully"));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while deleting the post"));
            }
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("update-post/{postId:int}")]
        public async Task<ActionResult> UpdatePost(int postId, [FromBody] PostUpdateDTO dto)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var post = await unitOfWork.Repository<Post>().GetByIdAsync(postId);
            if (post is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Post not found"));

            if (post.ClientId != clientId)
                return Forbid();

            post.Title = dto.Title;
            post.Description = dto.Description;

            unitOfWork.Repository<Post>().Update(post);
            await unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Post updated successfully"));
        }
    }
}
