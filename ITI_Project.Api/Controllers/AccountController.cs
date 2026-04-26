using ITI_Project.Api.DTO;
using ITI_Project.Api.DTO.Account;
using ITI_Project.Api.ErrorHandling;
using ITI_Project.Api.Helpers;
using ITI_Project.Core;
using ITI_Project.Core.Enums;
using ITI_Project.Core.IServices;
using ITI_Project.Core.Models.Identity;
using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Users;
using ITI_Project.Services.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITI_Project.Api.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IAuthService authService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<AccountController> logger;
        private readonly IConfiguration configuration;
        private readonly IFileStorageService fileStorageService;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IAuthService authService,
            IUnitOfWork unitOfWork,
            ILogger<AccountController> logger,
            IConfiguration configuration,
            IFileStorageService fileStorageService
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.authService = authService;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.configuration = configuration;
            this.fileStorageService = fileStorageService;
        }

        #region Register
        [HttpPost("register")] // POST: api/Account/register
        public async Task<ActionResult> Register(RegisterDTO model)
        {
            if (await userManager.FindByEmailAsync(model.Email) is not null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "This Email is Already Exist."));

            var user = new AppUser
            {
                FullName = $"{model.FirstName} {model.LastName}".Trim(),
                Email = model.Email,
                UserName = model.Email,
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var isDuplicate = result.Errors.Any(e => e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail");
                var errors = isDuplicate
                    ? new[] { "Email already exists." }
                    : result.Errors.Select(e => e.Description).ToArray();

                return BadRequest(new ApiValidationErrorResponse(StatusCodes.Status400BadRequest, "Failed to create user.")
                {
                    Errors = errors
                });
            }

            // Fetch registered user
            var registeredUser = await userManager.FindByEmailAsync(model.Email);
            if (registeredUser == null)
                return BadRequest(new ApiResponse(400, "User registration failed."));

            var roleResult = await userManager.AddToRoleAsync(registeredUser, nameof(UserRoleType.Client));
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(registeredUser); // rollback user creation
                return BadRequest(new ApiValidationErrorResponse(StatusCodes.Status400BadRequest, "Failed to assign Client role.")
                {
                    Errors = roleResult.Errors.Select(e => e.Description).ToArray()
                });
            }

            // OTP Configuration
            //var OTP = await GenerateAndSaveOtp(registeredUser, OtpType.EmailVerification);

            //var email = new Email()
            //{
            //    Subject = "Your OTP Code for Email Verification",
            //    Recipients = model.Email,
            //    Body = EmailTemplateService.GetOtpEmailBody(user.Email, OTP)
            //};
            //var reuslt = await _emailService.SendEmailAsync(email);
            //if (!reuslt)
            //    return StatusCode(500, new ApiResponse(500, "Failed to send new OTP code"));

            var newClient = new Client
            {
                AppUserId = registeredUser.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateHelper.GetTodayInEgypt()
            };
            try
            {
                await unitOfWork.Repository<Client>().AddAsync(newClient);
                await unitOfWork.CompleteAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database update error while creating account for client: {Email}", model.Email);
                await userManager.DeleteAsync(registeredUser); // Rollback user creation

                return BadRequest(new ApiResponse(500, "An unexpected error occurred."));
            }

            logger.LogInformation("User registered successfully: {Email}", model.Email);

            // after creating Client (newClient)
            if (model.IsProvider)
            {
                var provider = new Provider
                {
                    ClientId = newClient.Id,
                    StartedAt = DateHelper.GetTodayInEgypt(),
                    VerificationStatus = VerificationStatus.Pending,
                    Isverified = false,
                    Rating = null,
                    RatingSum = 0,
                    ReviewsCount = 0,
                    JobsCount = 0,
                };

                try
                {
                    await unitOfWork.Repository<Provider>().AddAsync(provider);
                    await unitOfWork.CompleteAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while creating provider for client: {Email}", model.Email);
                    await userManager.DeleteAsync(registeredUser); // Rollback user creation
                    unitOfWork.Repository<Client>().Delete(newClient); // Rollback client creation
                    return BadRequest(new ApiResponse(500, "An unexpected error occurred while registering as a provider."));
                }
                logger.LogInformation("Provider registered successfully: {Email}", model.Email);
            }
            return Ok($"{(model.IsProvider ? "Provider" : "Client")} registered successfully");
        }

        #endregion


        #region Login
        [HttpPost("login")] // POST: api/Account/login
        public async Task<ActionResult<ClientDto>> Login(LoginDTO model)
        {
            var appUser = await userManager.FindByEmailAsync(model.Email);

            if (appUser == null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "Invalid Email"));

            var result = await signInManager.CheckPasswordSignInAsync(appUser, model.Password, true);

            if (!result.Succeeded)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "Invalid Password"));

            var roles = await userManager.GetRolesAsync(appUser);

            var client = await unitOfWork.Repository<Client>().GetByAppUserIdAsync(appUser.Id);
            if (client == null)
                return StatusCode(StatusCodes.Status500InternalServerError,(new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while registering")));

            var provider = await unitOfWork.Repository<Provider>().GetByConditionAsync(p => p.ClientId == client.Id);

            var isClientProfileComplete =
                !string.IsNullOrWhiteSpace(client.FirstName) &&
                !string.IsNullOrWhiteSpace(client.LastName) &&
                client.GovernorateId.HasValue &&
                client.RegionId.HasValue;

            var status = provider != null
                ? provider.VerificationStatus switch
                {
                    VerificationStatus.Pending => ProfileStatus.Pending,
                    VerificationStatus.UnderReview => ProfileStatus.UnderReview,
                    VerificationStatus.Approved => ProfileStatus.Approved,
                    VerificationStatus.Rejected => ProfileStatus.Rejected,
                    VerificationStatus.Suspended => ProfileStatus.Suspended,
                    _ => ProfileStatus.Pending
                }
                : (isClientProfileComplete ? ProfileStatus.Completed : ProfileStatus.Pending);
            // Generate Access Token
            var accessToken = await authService.CreateTokenAsync(appUser, userManager);

            // Generate or Retrieve Active Refresh Token
            var refreshToken = appUser.RefreshTokens?.FirstOrDefault(rt => rt.IsActive);
            if (refreshToken == null)
            {
                refreshToken = TokenHelper.GenerateRefreshToken();
                appUser.RefreshTokens?.Add(refreshToken);
                await userManager.UpdateAsync(appUser);
            }

            SetRefreshTokenInCookie(refreshToken.Token, refreshToken.ExpiresOn);

            return Ok(
                new ClientDto()
                {
                    FullName = appUser.FullName,
                    Email = appUser.Email!,
                    AccessToken = accessToken,
                    Role = roles,
                    IsProvider = provider != null,
                    Status = status,
                    PictureUrl = client.PictureUrl,
                    AccessTokenExpiration = DateTime.UtcNow.AddMinutes(double.Parse(configuration["JWT:AccessTokenExpirationInMinutes"]!)),
                    IsAuthenticated = true
                }
            );
        }

        #endregion


        #region Logout
        [HttpPost("logout")] // POST: api/Account/logout
        public async Task<ActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "No refresh token found" });

            var result = await authService.RevokeTokenAsync(refreshToken);
            if (!result)
                return BadRequest(new { message = "Failed to revoke token" });

            Response.Cookies.Delete("refreshToken");
            await signInManager.SignOutAsync();

            return Ok(new { message = "Logged out successfully" });
        }

        #endregion


        #region Delete Account
        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "Invalid user"));

            var appUser = await userManager.FindByEmailAsync(email);
            if (appUser == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "User not found"));

            var client = await unitOfWork.Repository<Client>().GetByAppUserIdAsync(appUser.Id);
            if (client == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Client not found"));

            var provider = await unitOfWork.Repository<Provider>()
                .GetByConditionAsync(p => p.ClientId == client.Id);

            var roles = await userManager.GetRolesAsync(appUser);

            if (roles.Contains(nameof(UserRoleType.Admin)) && client.Id == 1)
            {
                return BadRequest(new ApiResponse(
                    StatusCodes.Status400BadRequest,
                    "The main admin account cannot be deleted"
                ));
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                // Cancel client requests
                var clientRequests = await unitOfWork.Repository<ServiceRequest>()
                    .GetManyByConditionAsync(sr =>
                        sr.ClientId == client.Id &&
                        (sr.RequestStatus == RequestStatus.Open ||
                        sr.RequestStatus == RequestStatus.Assigned ||
                        sr.RequestStatus == RequestStatus.InProgress));

                foreach (var request in clientRequests ?? Enumerable.Empty<ServiceRequest>())
                {
                    request.RequestStatus = RequestStatus.Cancelled;
                }

                // delete reports
                var reports = await unitOfWork.Repository<Report>()
                    .GetManyByConditionAsync(r =>
                      r.ReporterId == client.Id ||
                      r.TargetUserId == client.Id) ?? new List<Report>();

                var hasActiveReports = reports.Any(r => r.TargetUserId == client.Id && r.Status == ReportStatus.UnderReview);

                if (hasActiveReports)
                {
                    return BadRequest(new ApiResponse(
                        StatusCodes.Status400BadRequest,
                        "You cannot delete your account while you have active reports"
                    ));
                }

                foreach (var report in reports)
                {
                    if (report.ReporterId == client.Id)
                    {
                        unitOfWork.Repository<Report>().Delete(report);
                    }
                }

                // Cancel provider requests (if provider exists)
                if (provider != null)
                {
                    var providerRequests = await unitOfWork.Repository<ServiceRequest>()
                        .GetManyByConditionAsync(sr =>
                            sr.ProviderId == provider.Id &&
                            (sr.RequestStatus == RequestStatus.Assigned ||
                             sr.RequestStatus == RequestStatus.InProgress));

                    foreach (var request in providerRequests ?? Enumerable.Empty<ServiceRequest>())
                    {
                        request.RequestStatus = RequestStatus.Cancelled;
                        request.ProviderId = null;
                    }

                    var offers = await unitOfWork.Repository<RequestOffer>()
                        .GetManyByConditionAsync(o => o.ProviderId == provider.Id) ?? new List<RequestOffer>();

                    foreach (var offer in offers)
                    {
                        unitOfWork.Repository<RequestOffer>().Delete(offer);
                    }
                    // delete documents
                    var documents = await unitOfWork.Repository<ProviderDocument>()
                        .GetManyByConditionAsync(d => d.ProviderId == provider.Id) ?? new List<ProviderDocument>();

                    foreach (var doc in documents)
                    {
                        if (!string.IsNullOrWhiteSpace(doc.DocumentUrl))
                            fileStorageService.DeleteFile(doc.DocumentUrl);

                        unitOfWork.Repository<ProviderDocument>().Delete(doc);
                    }
                    // delete provider
                    unitOfWork.Repository<Provider>().Delete(provider);
                }

                // 2. Delete client
                unitOfWork.Repository<Client>().Delete(client);


                // 3. Delete Identity user

                await unitOfWork.CompleteAsync();

                var result = await userManager.DeleteAsync(appUser);
                if (!result.Succeeded)
                {
                    await unitOfWork.RollbackAsync();
                    return StatusCode(500, new ApiResponse(500, "Failed to delete user account"));
                }

                await unitOfWork.CommitAsync();

                // 4. Revoke tokens
                var refreshToken = Request.Cookies["refreshToken"];
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await authService.RevokeTokenAsync(refreshToken);
                    Response.Cookies.Delete("refreshToken");
                }

                // 5. Sign out
                await signInManager.SignOutAsync();

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Account deleted successfully"));
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();

                logger.LogError(ex, "Error while deleting account for user {Email}", email);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(500, "An error occurred while deleting the account"));
            }
        } 

        #endregion


        #region Refresh token
        [HttpPost("refresh-token")] // POST: api/Account/refreshToken
        public async Task<ActionResult> RefreshTokenAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Refresh token is missing"));

            var result = await authService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, result.Message));

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }

        #endregion


        #region Revoke Token
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] string? Token)
        {
            var token = Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Token is required"));

            var result = await authService.RevokeTokenAsync(token);

            if (!result)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Token is invalid"));

            await signInManager.SignOutAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Token revoked"));
        }

        #endregion


        #region Change Password
        [Authorize]
        //[EnableRateLimiting("PasswordLimiter")]
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await userManager.FindByEmailAsync(email!);
            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User is unauthorized"));


            var isPasswordValid = await signInManager.CheckPasswordSignInAsync(user, request.OldPassword, true);
            if (!isPasswordValid.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid Password"));

            var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Failed to change password"));

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Password changed successfully"));
        }

        #endregion


        #region Account Status
        [Authorize]
        [HttpGet("account-status")]
        public async Task<ActionResult<AccountStatusDto>> GetAccountStatus()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var appUser = await userManager.FindByEmailAsync(email!);
            if (appUser == null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User is unauthorized"));

            var roles = await userManager.GetRolesAsync(appUser);

            var client = await unitOfWork.Repository<Client>().GetByAppUserIdAsync(appUser.Id);
            if (client == null)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError, "Client not found"));

            var provider = await unitOfWork.Repository<Provider>().GetByConditionAsync(p => p.ClientId == client.Id);

            var isClientProfileComplete =
                !string.IsNullOrWhiteSpace(client.FirstName) &&
                !string.IsNullOrWhiteSpace(client.LastName) &&
                client.GovernorateId.HasValue &&
                client.RegionId.HasValue;

            var status = provider != null
                ? provider.VerificationStatus switch
                {
                    VerificationStatus.Pending => ProfileStatus.Pending,
                    VerificationStatus.UnderReview => ProfileStatus.UnderReview,
                    VerificationStatus.Approved => ProfileStatus.Approved,
                    VerificationStatus.Rejected => ProfileStatus.Rejected,
                    VerificationStatus.Suspended => ProfileStatus.Suspended,
                    _ => ProfileStatus.Pending
                }
                : (isClientProfileComplete ? ProfileStatus.Completed : ProfileStatus.Pending);

            return Ok(new AccountStatusDto
            {
                Role = roles,
                IsProvider = provider != null,
                Status = status
            });
        }

        #endregion


        #region Private Methods
        /******************************** Private Method ********************************/
        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookiOption = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookiOption);
        }

        //private async Task<string> GenerateAndSaveOtp(AppUser user, OtpType OtpType)
        //{
        //    var OTP = new Random().Next(100000, 999999).ToString();
        //    var userOTP = new UserOtpVerifications()
        //    {
        //        OtpCode = OTP,
        //        OtpType = OtpType,
        //        ExpiresOn = DateTime.Now.AddMinutes(5),
        //        IsVerified = false,
        //        ApplicationUserId = user.Id
        //    };
        //    await _unitOfWork.Repository<UserOtpVerifications>().AddWithSaveAsync(userOTP);

        //    return OTP;
        //}

        #endregion
    }
}
