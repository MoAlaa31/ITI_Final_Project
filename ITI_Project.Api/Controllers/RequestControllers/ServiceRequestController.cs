using AutoMapper;
using ITI_Project.Api.DTO.Requests;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Constants;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Services;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Specifications.RequestSpecs;
using ITI_Project.Core.Specifications.ServiceRequestSpecs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ITI_Project.Core.IServices;

namespace ITI_Project.Api.Controllers.RequestControllers
{
    public class ServiceRequestController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;

        public ServiceRequestController(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPost("create-service-request")]
        public async Task<ActionResult<ServiceRequestDTO>> CreateServiceRequest([FromForm] ServiceRequestFromUserDTO serviceRequestDTO)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if(!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var clientExists = await unitOfWork.Repository<Client>().AnyAsync(u => u.Id == clientId);
            if (!clientExists)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            var serviceExists = await unitOfWork.Repository<Service>().AnyAsync(s => s.Id == serviceRequestDTO.ServiceId);
            if (!serviceExists)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid service"));

            var uploadedPaths = new List<string>();

            if (serviceRequestDTO.Images is { Count: > 0 })
            {
                foreach (var image in serviceRequestDTO.Images)
                {
                    var uploadResult = await fileStorageService.UploadFileAsync(image, "service-request-images", User);
                    if (!uploadResult.Success)
                    {
                        foreach (var path in uploadedPaths)
                            fileStorageService.DeleteFile(path);

                        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, uploadResult.Message));
                    }

                    uploadedPaths.Add(uploadResult.FilePath!);
                }
            }

            var serviceRequest = mapper.Map<ServiceRequest>(serviceRequestDTO);
            serviceRequest.ClientId = clientId;
            serviceRequest.RequestStatus = RequestStatus.Open;
            serviceRequest.CreatedAt = DateHelper.GetNowInEgypt();

            try
            {
                await unitOfWork.Repository<ServiceRequest>().AddAsync(serviceRequest);
                await unitOfWork.CompleteAsync();

                serviceRequest.ServiceRequestLocation = new ServiceRequestLocation
                {
                    Latitude = serviceRequestDTO.Latitude,
                    Longitude = serviceRequestDTO.Longitude,
                    ServiceRequestId = serviceRequest.Id
                };

                unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
                await unitOfWork.CompleteAsync();

                if (uploadedPaths.Count > 0)
                {
                    var images = uploadedPaths.Select(path => new ServiceRequestImage
                    {
                        ImageUrl = path,
                        ServiceRequestId = serviceRequest.Id,
                        ServiceRequest = serviceRequest
                    }).ToList();

                    await unitOfWork.Repository<ServiceRequestImage>().AddRangeAsync(images);
                    await unitOfWork.CompleteAsync();
                }
            }
            catch
            {
                foreach (var path in uploadedPaths)
                    fileStorageService.DeleteFile(path);

                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while creating the service request. Please try again."));
            }

            var dto = mapper.Map<ServiceRequestDTO>(serviceRequest);
            return CreatedAtAction(nameof(GetServiceRequestById), new { id = serviceRequest.Id }, dto);
        }

        [Authorize]
        [HttpGet("get-request-byid/{id:int}")]
        public async Task<ActionResult<ServiceRequestDTO>> GetServiceRequestById(int id)
        {
            var serviceRequest = await unitOfWork.Repository<ServiceRequest>()
                .GetByIdWithIncludesAsync(id, sr => sr.ServiceRequestLocation!, sr => sr.ServiceRequestImages!);
            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (User.IsInRole(nameof(UserRoleType.Client)))
            {
                var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
                if (int.TryParse(clientIdClaim, out var clientId) && serviceRequest.ClientId != clientId)
                    return Forbid();
            }
            else if (User.IsInRole(nameof(UserRoleType.Provider)))
            {
                var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
                if (int.TryParse(providerIdClaim, out var providerId) && serviceRequest.ProviderId != providerId)
                    return Forbid();
            }

            var dto = mapper.Map<ServiceRequestDTO>(serviceRequest);
            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpGet("my-requests")]
        public async Task<ActionResult<IReadOnlyList<ServiceRequestDTO>>> GetMyServiceRequests()
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var serviceRequests = await unitOfWork.Repository<ServiceRequest>()
                .GetManyByConditionAsync(sr => sr.ClientId == clientId, sr => sr.ServiceRequestImages!);

            if (serviceRequests is null || serviceRequests.Count == 0)
                return Ok(new List<ServiceRequestDTO>());

            var dto = mapper.Map<IReadOnlyList<ServiceRequestDTO>>(serviceRequests);
            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpGet("available-requests")]
        public async Task<ActionResult<IReadOnlyList<ServiceRequestDTO>>> GetAvailableServiceRequests([FromQuery] RequestSpecParams specParams)
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var provider = await unitOfWork.Repository<Provider>()
                .GetByIdWithIncludesAsync(providerId, p => p.BaseLocation!);

            if (provider?.BaseLocation is null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Provider base location is required"));

            var radiusKm = Math.Max(1, specParams.RadiusKm ?? 10);
            var lat = provider.BaseLocation.Latitude;
            var lng = provider.BaseLocation.Longitude;

            var deltaLat = radiusKm / 111.32;
            var deltaLng = radiusKm / (111.32 * Math.Cos(lat * Math.PI / 180));

            specParams.MinLatitude = lat - deltaLat;
            specParams.MaxLatitude = lat + deltaLat;
            specParams.MinLongitude = lng - deltaLng;
            specParams.MaxLongitude = lng + deltaLng;

            var providerServiceIds = await unitOfWork.Repository<ProviderService>()
                .GetManyByConditionAsync(ps => ps.ProviderId == providerId) ?? new List<ProviderService>();

            var serviceIds = providerServiceIds.Select(ps => ps.ServiceId).Distinct().ToList();
            if (serviceIds.Count == 0)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Provider has no services"));

            var specs = new ProviderServiceRequestWithPaginationSpecification(specParams, serviceIds);
            var serviceRequests = await unitOfWork.Repository<ServiceRequest>().GetAllWithSpecAsync(specs);

            if (serviceRequests is null || serviceRequests.Count == 0)
                return Ok(new List<ServiceRequestDTO>());

            // exact distance filter (Haversine)
            var filtered = serviceRequests
                .Where(sr => sr.ServiceRequestLocation != null &&
                    GetDistanceKm(lat, lng,
                        sr.ServiceRequestLocation.Latitude,
                        sr.ServiceRequestLocation.Longitude) <= radiusKm)
                .ToList();

            var dto = mapper.Map<IReadOnlyList<ServiceRequestDTO>>(filtered);
            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpGet("my-assigned-requests")]
        public async Task<ActionResult<IReadOnlyList<ServiceRequestDTO>>> GetMyAssignedRequests()
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var serviceRequests = await unitOfWork.Repository<ServiceRequest>()
                .GetManyByConditionAsync(sr => sr.ProviderId == providerId, sr => sr.ServiceRequestLocation!, sr => sr.ServiceRequestImages!);

            if (serviceRequests is null || serviceRequests.Count == 0)
                return Ok(new List<ServiceRequestDTO>());

            var dto = mapper.Map<IReadOnlyList<ServiceRequestDTO>>(serviceRequests);
            return Ok(dto);
        }

        //[Authorize(Roles = $"{nameof(UserRoleType.Provider)},{nameof(UserRoleType.Admin)}")]
        //[HttpPut("update-status/{id}")]
        //public async Task<ActionResult<ServiceRequestDTO>> UpdateServiceRequestStatus(int id, [FromQuery] RequestStatus status)
        //{
        //    var serviceRequest = await unitOfWork.Repository<ServiceRequest>().GetByIdAsync(id);
        //    if (serviceRequest is null)
        //        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

        //    if (User.IsInRole(nameof(UserRoleType.Provider)))
        //    {
        //        var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
        //        if (int.TryParse(providerIdClaim, out var providerId) && serviceRequest.ProviderId != providerId)
        //            return Forbid();
        //    }

        //    serviceRequest.RequestStatus = status;

        //    try
        //    {
        //        unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
        //        await unitOfWork.CompleteAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //            new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the service request status. Please try again."));
        //    }

        //    var dto = mapper.Map<ServiceRequestDTO>(serviceRequest);
        //    return CreatedAtAction(nameof(CreateServiceRequest), new { id = serviceRequest.Id }, dto);
        //}


        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("assign/{id:int}")]
        public async Task<ActionResult<ServiceRequestDTO>> AssignServiceRequest(int id, [FromQuery] int providerId)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>()
                .GetByIdWithIncludesAsync(id, sr => sr.RequestOffers!);

            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.ClientId != clientId)
                return Forbid();

            if (serviceRequest.RequestStatus != RequestStatus.Open || serviceRequest.ProviderId != null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Request is not available for assignment"));

            var hasOffer = serviceRequest.RequestOffers?.Any(o => o.ProviderId == providerId) == true;
            if (!hasOffer)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Provider did not submit an offer for this request"));

            serviceRequest.ProviderId = providerId;
            serviceRequest.RequestStatus = RequestStatus.Assigned;

            unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
            await unitOfWork.CompleteAsync();

            var dto = mapper.Map<ServiceRequestDTO>(serviceRequest);
            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Provider))]
        [HttpPut("start/{id:int}")]
        public async Task<ActionResult<ServiceRequestDTO>> StartServiceRequest(int id, [FromQuery] bool isAccepted)
        {
            var providerIdClaim = User.FindFirstValue(Identifiers.ProviderId);
            if (!int.TryParse(providerIdClaim, out var providerId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ProviderId claim is missing or invalid"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>().GetByIdAsync(id);
            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.ProviderId != providerId)
                return Forbid();

            if (serviceRequest.RequestStatus != RequestStatus.Assigned)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Request is not assigned to you"));

            if (!isAccepted)
            {
                serviceRequest.ProviderId = null;
                serviceRequest.RequestStatus = RequestStatus.Open;
                unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
                await unitOfWork.CompleteAsync();
                var dto = mapper.Map<ServiceRequestDTO>(serviceRequest);
                return Ok(dto);
            }
            else
            {
                serviceRequest.RequestStatus = RequestStatus.InProgress;
                unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
                await unitOfWork.CompleteAsync();
                var dto = mapper.Map<ServiceRequestDTO>(serviceRequest);
                return Ok(dto);
            }
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("complete/{id:int}")]
        public async Task<ActionResult<ServiceRequestDTO>> CompleteServiceRequest(int id)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>().GetByIdAsync(id);
            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.ClientId != clientId)
                return Forbid();

            if (serviceRequest.RequestStatus != RequestStatus.InProgress)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Request is not in progress"));

            serviceRequest.RequestStatus = RequestStatus.Completed;

            unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
            await unitOfWork.CompleteAsync();

            var dto = mapper.Map<ServiceRequestDTO>(serviceRequest);
            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpPut("cancel/{id}")]
        public async Task<ActionResult<ServiceRequestDTO>> CancelServiceRequest(int id)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>().GetByIdAsync(id);
            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.ClientId != clientId)
                return Forbid();

            if (serviceRequest.RequestStatus == RequestStatus.Completed)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Cannot cancel a completed request"));

            if (serviceRequest.RequestStatus == RequestStatus.Cancelled)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Request is already cancelled"));

            serviceRequest.RequestStatus = RequestStatus.Cancelled;

            try
            {
                unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
                await unitOfWork.CompleteAsync();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while cancelling the service request. Please try again."));
            }

            var dto = mapper.Map<ServiceRequestDTO>(serviceRequest);
            return Ok(dto);
        }

        [Authorize(Roles = nameof(UserRoleType.Client))]
        [HttpDelete("delete-service-request/{id:int}")]
        public async Task<ActionResult> DeleteServiceRequest(int id)
        {
            var clientIdClaim = User.FindFirstValue(Identifiers.ClientId);
            if (!int.TryParse(clientIdClaim, out var clientId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "ClientId claim is missing or invalid"));

            var serviceRequest = await unitOfWork.Repository<ServiceRequest>()
                .GetByIdWithIncludesAsync(id, sr => sr.ServiceRequestImages!);
            if (serviceRequest is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Service request not found"));

            if (serviceRequest.ClientId != clientId)
                return Forbid();

            try
            {
                if (serviceRequest.ServiceRequestImages != null)
                {
                    foreach (var image in serviceRequest.ServiceRequestImages)
                        fileStorageService.DeleteFile(image.ImageUrl);
                }

                unitOfWork.Repository<ServiceRequest>().Delete(serviceRequest);
                await unitOfWork.CompleteAsync();

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Service request deleted successfully"));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while deleting the service request"));
            }
        }





        private static double GetDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius km
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);
    }
}
