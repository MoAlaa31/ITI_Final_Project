using ITI_Project.Api.ErrorHandling;
using ITI_Project.Core;
using ITI_Project.Core.Enums;
using ITI_Project.Core.Models.Users;
using ITI_Project.Core.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITI_Project.Core.Specifications;

namespace ITI_Project.Api.Controllers.UserControllers
{
    public class AdminController: BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [Authorize(Roles = nameof(UserRoleType.Admin))]
        [HttpGet("get-admin-dashboard")]
        public async Task<ActionResult<object>> GetDashboardStats([FromQuery] DashboardPeriod period = DashboardPeriod.Week)
        {
            // Clients who are not providers
            var clientCount = await unitOfWork.Repository<Client>()
                .GetCountAsync(new BaseSpecifications<Client>(c => c.Provider == null));

            // Providers
            var providerCount = await unitOfWork.Repository<Provider>()
                .GetCountAsync(new BaseSpecifications<Provider>());

            // Requests with status Open
            var openRequests = await unitOfWork.Repository<ServiceRequest>()
                .GetCountAsync(new BaseSpecifications<ServiceRequest>(r => r.RequestStatus == RequestStatus.Open));

            // Requests with status InProgress
            var inProgressRequests = await unitOfWork.Repository<ServiceRequest>()
                .GetCountAsync(new BaseSpecifications<ServiceRequest>(r => r.RequestStatus == RequestStatus.InProgress));

            DateTime fromDate = DateTime.UtcNow.Date;
            int days = period switch
            {
                DashboardPeriod.Week => 7,
                DashboardPeriod.Month => 30,
                DashboardPeriod.Year => 365,
                _ => 7
            };
            fromDate = fromDate.AddDays(-days + 1);

            var requests = await unitOfWork.Repository<ServiceRequest>()
                .GetManyByConditionAsync(r => r.CreatedAt >= fromDate);

            var perDay = requests
                .GroupBy(r => r.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList();

            return Ok(new
            {
                ClientsNotProviders = clientCount,
                Providers = providerCount,
                OpenRequests = openRequests,
                InProgressRequests = inProgressRequests,
                RequestsPerDay = perDay
            });
        }
    }
}