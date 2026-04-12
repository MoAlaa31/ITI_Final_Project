using AutoMapper;
using ITI_Project.Api.Attributes;
using ITI_Project.Api.DTO.Moderation;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Users;
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

        [Authorize(Roles = $"{nameof(UserRoleType.Admin)},{nameof(UserRoleType.Provider)}")]
        [HttpGet("get-documents")]
        public async Task<ActionResult<ProviderDocumentDto>> GetAllProviderDocuments([FromQuery] int? providerId = null)
        {
            int actualProviderId;

            if (!User.IsInRole(nameof(UserRoleType.Admin)))
            {
                var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
                if (!int.TryParse(providerIdClaim, out actualProviderId))
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));
            }
            else
            {
                if (!providerId.HasValue)
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "ProviderId is required for admin users"));
                actualProviderId = providerId.Value;
            }

            var provider = await unitOfWork.Repository<Provider>().GetByIdAsync(actualProviderId);
            if (provider is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var documents = await unitOfWork.Repository<ProviderDocument>()
                .GetManyByConditionAsync(spec => spec.ProviderId == actualProviderId);
            if (documents is null || documents.Count == 0)
                return NotFound (new ApiResponse(StatusCodes.Status404NotFound, "No documents found for this provider"));

            var dto = mapper.Map<IReadOnlyList<ProviderDocumentDto>>(documents);

            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("upload-document")]
        public async Task<ActionResult<ProviderDocumentDto>> AddProviderDocument([FromForm] ProviderDocumentUploadDTO uploadDTO)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var provider = await unitOfWork.Repository<Provider>().GetByConditionAsync(p => p.ClientId == clientId);
            if (provider is null) 
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var existingDocuments = await unitOfWork.Repository<ProviderDocument>()
                .GetManyByConditionAsync(d => d.ProviderId == provider.Id) ?? new List<ProviderDocument>();

            if (existingDocuments.Count >= 3)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Providers can only upload 3 documents."));

            var existingDocument = existingDocuments
                .FirstOrDefault(d => d.DocumentType == uploadDTO.DocumentType);
            if (existingDocument != null)
                return Conflict(new ApiResponse(StatusCodes.Status409Conflict, "A document of this type already exists for this provider."));

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
                ProviderId = provider.Id,
                DocumentType = uploadDTO.DocumentType,
                DocumentUrl = uploadResult.FilePath!,
                IsApproved = null
            };

            try
            {
                await unitOfWork.Repository<ProviderDocument>().AddAsync(document);

                var distinctDocumentTypes = existingDocuments
                    .Select(d => d.DocumentType)
                    .Append(document.DocumentType)
                    .Distinct()
                    .Count();

                if (distinctDocumentTypes == 3)
                {
                    provider.VerificationStatus = VerificationStatus.UnderReview;
                    unitOfWork.Repository<Provider>().Update(provider);
                }

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

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpPut("validate-document/{documentId:int}")]
        public async Task<ActionResult<ProviderDocumentDto>> ValidateProviderDocument(int documentId, [FromQuery] bool isValid)
        {
            var document = await unitOfWork.Repository<ProviderDocument>().GetByIdAsync(documentId);
            if (document is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Document not found"));

            document.IsApproved = isValid;

            try
            {
                if (!isValid)
                {
                    var provider = await unitOfWork.Repository<Provider>().GetByIdAsync(document.ProviderId);
                    if (provider is null)
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

                    provider.VerificationStatus = VerificationStatus.Pending;
                    unitOfWork.Repository<Provider>().Update(provider);
                }

                unitOfWork.Repository<ProviderDocument>().Update(document);
                await unitOfWork.CompleteAsync();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while validating the document. Please try again."));
            }

            var dto = mapper.Map<ProviderDocumentDto>(document);
            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("update-document/{documentId:int}")]
        public async Task<ActionResult<ProviderDocumentDto>> UpdateProviderDocument(int documentId, [FromForm] ProviderDocumentUpdateDTO updateDTO)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var provider = await unitOfWork.Repository<Provider>().GetByConditionAsync(p => p.ClientId == clientId);
            if (provider is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Provider not found"));

            var document = await unitOfWork.Repository<ProviderDocument>().GetByIdAsync(documentId);
            if (document is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Document not found"));

            if (document.ProviderId != provider.Id)
                return Forbid();

            var uploadResult = await fileStorageService.UploadFileAsync(
                updateDTO.DocumentFile,
                "provider-documents",
                User,
                updateDTO.FileName
            );

            if (!uploadResult.Success)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, uploadResult.Message));

            if (!string.IsNullOrWhiteSpace(document.DocumentUrl))
                fileStorageService.DeleteFile(document.DocumentUrl);

            document.DocumentUrl = uploadResult.FilePath!;
            document.IsApproved = null;

            try
            {
                unitOfWork.Repository<ProviderDocument>().Update(document);
                await unitOfWork.CompleteAsync();
            }
            catch
            {
                fileStorageService.DeleteFile(uploadResult.FilePath!);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the document. Please try again."));
            }

            var dto = mapper.Map<ProviderDocumentDto>(document);
            return Ok(dto);
        }
    }
}
