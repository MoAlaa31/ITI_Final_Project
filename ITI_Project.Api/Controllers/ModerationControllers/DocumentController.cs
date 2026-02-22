using AutoMapper;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Persons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers.ModerationControllers
{
    public class DocumentController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;

        public DocumentController(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpGet("get-documents")]
        public async Task<ActionResult<ProviderDocumentDto>> GetAllProviderDocuments()
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var provider = await unitOfWork.Repository<Provider>().GetByIdAsync(providerId);
            if (provider is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var documents = await unitOfWork.Repository<ProviderDocument>()
                .GetManyByConditionAsync(spec => spec.ProviderId == providerId);
            if (documents is null || documents.Count == 0)
                return NotFound (new ApiResponse(StatusCodes.Status404NotFound, "No documents found for this provider"));

            var dto = mapper.Map<IReadOnlyList<ProviderDocumentDto>>(documents);

            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPost("upload-document")]
        public async Task<ActionResult<ProviderDocumentDto>> AddProviderDocument([FromForm] ProviderDocumentUploadDTO uploadDTO)
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var provider = await unitOfWork.Repository<Provider>().GetByIdAsync(providerId);
            if (provider is null) 
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var uploadResult = await fileStorageService.UploadFileAsync(
                uploadDTO.DocumentFile,
                "provider-documents",
                User,
                uploadDTO.FileName
            );

            if (!uploadResult.Success)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, uploadResult.Message));

            var document = new ProviderDocument
            {
                ProviderId = providerId,
                DocumentType = uploadDTO.DocumentType,
                DocumentUrl = uploadResult.FilePath!,
                IsApproved = false
            };

            try
            {
                await unitOfWork.Repository<ProviderDocument>().AddAsync(document);
                await unitOfWork.CompleteAsync();
            }
            catch(Exception ex) 
            {
                // If there's an error saving to the database, we should delete the uploaded file to avoid orphaned files
                fileStorageService.DeleteFile(uploadResult.FilePath!);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while saving the document. Please try again."));
            }

            var dto = mapper.Map<ProviderDocumentDto>(document);
            return Ok(dto);
        }
    }
}
