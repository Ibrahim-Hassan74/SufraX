using AutoMapper;
using Domain.Entities.Common;
using Domain.Entities.Product;
using EStoreX.Core.Domain.IdentityEntities;
using EStoreX.Core.DTO.Account.Requests;
using EStoreX.Core.DTO.Account.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.ServiceContracts.Account;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Res = EStoreX.Core.Resources.Services.Common.EmailTemplateService;
using System.Globalization;

namespace EStoreX.Core.Services.Account
{
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSenderService _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly IUserManagementService _userManagementService;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IImageService _imageService;
        private readonly IEntityImageManager<ApplicationUser> _imageManager;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer<AuthenticationService> _localizer;

        public AuthenticationService(UserManager<ApplicationUser> userManager,
            IEmailSenderService emailSender,
            SignInManager<ApplicationUser> signInManager,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserManagementService userManagementService,
            RoleManager<ApplicationRole> roleManager,
            IEntityImageManager<ApplicationUser> imageManager,
            IImageService imageService,
            IConfiguration configuration,
            IStringLocalizer<AuthenticationService> localizer) : base(unitOfWork, mapper)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _userManagementService = userManagementService;
            _roleManager = roleManager;
            _imageManager = imageManager;
            _imageService = imageService;
            _configuration = configuration;
            _localizer = localizer;
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> RegisterAsync(RegisterDTO registerDTO, string? clientKey)
        {
            if (registerDTO == null)
                return ApiResponseFactory.Failure(_localizer["InvalidRegistrationData"].Value, 400, _localizer["RegistrationDataRequired"].Value);
            ValidationHelper.ModelValidation(registerDTO);

            if (await _userManager.FindByEmailAsync(registerDTO.Email) is not null)
                return ApiResponseFactory.Failure(_localizer["EmailAlreadyRegistered"].Value, 409, _localizer["EmailAlreadyInUse"].Value);



            ApplicationUser user = new ApplicationUser()
            {
                DisplayName = registerDTO.UserName,
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.Phone
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
                return ApiResponseFactory.Failure(_localizer["RegistrationFailed"].Value, 400, result.Errors.Select(e => e.Description).ToArray());


            await EnsureRoleExistsAndAssignAsync(user, UserTypeOptions.User.ToString());

            await SendEmail(user, clientKey);

            return ApiResponseFactory.Success(_localizer["RegistrationSuccessfulConfirmEmail"].Value);
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> LoginAsync(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                return ApiResponseFactory.Failure(_localizer["InvalidLoginData"].Value, 400, _localizer["LoginDataRequired"].Value);

            ValidationHelper.ModelValidation(loginDTO);

            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["EmailNotFound"].Value);


            if (!user.EmailConfirmed)
            {
                //await SendEmail(user);
                return ApiResponseFactory.Failure(_localizer["EmailNotConfirmed"].Value, 403, _localizer["ConfirmEmailBeforeLogin"].Value);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.RememberMe, true);

            if (result.Succeeded)
            {
                return await CreateSuccessLoginResponseAsync(user, loginDTO.RememberMe);
            }
            else if (result.IsLockedOut)
            {
                string message = _localizer["AccountLockedOut"].Value;
                return ApiResponseFactory.Failure(message, 423, message);
            }
            else if (result.IsNotAllowed)
            {
                return ApiResponseFactory.Failure(_localizer["LoginNotAllowed"].Value, 403, _localizer["LoginNotAllowed"].Value);
            }
            else
            {
                return ApiResponseFactory.Failure(_localizer["InvalidLoginAttempt"].Value, 401, _localizer["IncorrectEmailOrPassword"].Value);
            }
        }


        /// <inheritdoc/>
        public async Task<ApiResponse> ConfirmEmailAsync(ConfirmEmailDTO dto)
        {
            ValidationHelper.ModelValidation(dto);
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["UserIdNotFound"].Value);

            if (await _userManager.IsEmailConfirmedAsync(user))
                return ApiResponseFactory.Failure(_localizer["EmailAlreadyConfirmed"].Value, 200, _localizer["EmailAlreadyConfirmed"].Value);

            if (user.LastEmailConfirmationToken != dto.Token)
                return ApiResponseFactory.Failure(_localizer["InvalidConfirmationToken"].Value, 400, _localizer["TokenMismatch"].Value);

            var result = await _userManager.ConfirmEmailAsync(user, dto.Token);

            if (result.Succeeded)
            {
                user.LastEmailConfirmationToken = null;
                await _userManager.RemoveAuthenticationTokenAsync(user, "EmailConfirmation", "Token");
                await _userManager.RemoveAuthenticationTokenAsync(user, "EmailConfirmation", "TokenTime");

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return ApiResponseFactory.Failure(_localizer["FailedToUpdateUserAfterConfirmation"].Value, 500, updateResult.Errors.Select(e => e.Description).ToArray());

                return ApiResponseFactory.Success(_localizer["EmailConfirmedSuccessfully"].Value);
            }
            return ApiResponseFactory.Failure(_localizer["FailedToConfirmEmail"].Value, 400, result.Errors.Select(e => e.Description).ToArray());

        }
        /// <inheritdoc/>
        public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDTO dto, string? clientKey)
        {
            ValidationHelper.ModelValidation(dto);
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return ApiResponseFactory.Failure(_localizer["IncorrectEmail"].Value, 400, _localizer["EmailNotFound"].Value);

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return ApiResponseFactory.Failure(_localizer["ConfirmEmailBeforeReset"].Value, 400, _localizer["EmailNotConfirmed"].Value);

            //var logins = await _userManager.GetLoginsAsync(user);
            //if (logins.Any())
            //    return ApiResponseFactory.Failure("You registered using an external provider (Google/GitHub). Use it to log in.", 400, "External login detected.");

            #region Timer
            var existingToken = await _userManager.GetAuthenticationTokenAsync(user, "ResetPassword", "Token");

            if (!string.IsNullOrEmpty(existingToken))
            {
                var tokenTimeStr = await _userManager.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime");
                if (!string.IsNullOrEmpty(tokenTimeStr) && DateTimeOffset.TryParse(tokenTimeStr, out var tokenTime))
                {
                    if (DateTimeOffset.UtcNow < tokenTime.AddMinutes(5))
                        return ApiResponseFactory.Failure(_localizer["PasswordResetSentRecently"].Value, 429, _localizer["ResetAlreadyRequested"].Value);
                }
            }
            #endregion

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _userManager.SetAuthenticationTokenAsync(user, "ResetPassword", "Token", token);
            await _userManager.SetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime", DateTimeOffset.UtcNow.ToString());

            //var request = _httpContextAccessor.HttpContext?.Request;

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var client = await _unitOfWork.ApiClientRepository.GetByApiKeyAsync(clientKey!);
            var callback = client?.PasswordResetCallbackUrl ?? "";

            var frontendResetUrl = _configuration["FrontURLs:FrontendResetUrl"];

            var resetLink =
                $"{frontendResetUrl}" +
                $"?userId={Uri.EscapeDataString(user.Id.ToString())}" +
                $"&token={Uri.EscapeDataString(encodedToken)}" +
                $"&callback={Uri.EscapeDataString(callback)}";

            string html = EmailTemplateService.GetPasswordResetEmailTemplate(resetLink, CultureInfo.CurrentUICulture.Name);
            var emailDTO = new EmailDTO(user.Email, Res.PasswordReset_Subject, html);
            await _emailSender.SendEmailAsync(emailDTO);

            return ApiResponseFactory.Success(_localizer["PasswordResetLinkSent"].Value);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> VerifyResetPasswordTokenAsync(VerifyResetPasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.Token))
                return ApiResponseFactory.Failure(_localizer["InvalidVerificationRequest"].Value, 400, _localizer["UserIdAndTokenRequired"].Value);

            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["NoAccountForUserId"].Value);

            string decodedToken;

            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            }
            catch
            {
                return ApiResponseFactory.Failure(_localizer["InvalidTokenFormat"].Value, 400, _localizer["TokenFormatInvalid"].Value);
            }

            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "ResetPassword", "Token");

            if (storedToken == null || storedToken != decodedToken)
                return ApiResponseFactory.Failure(_localizer["InvalidOrExpiredToken"].Value, 400, _localizer["TokenInvalidOrUsed"].Value);

            var tokenTimeStr = await _userManager.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime");

            if (string.IsNullOrEmpty(tokenTimeStr) || !DateTimeOffset.TryParse(tokenTimeStr, out var tokenTime))
                return ApiResponseFactory.Failure(_localizer["TokenValidationFailed"].Value, 400, _localizer["TokenTimestampInvalid"].Value);

            if (DateTimeOffset.UtcNow > tokenTime.AddMinutes(5))
                return ApiResponseFactory.Failure(_localizer["ResetPasswordLinkExpired"].Value, 400, _localizer["ResetPasswordLinkExpired"].Value);

            return ApiResponseFactory.Success(_localizer["ResetPasswordTokenValid"].Value);
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            if (dto == null)
                return ApiResponseFactory.Failure(_localizer["InvalidResetPasswordData"].Value, 400, _localizer["RequestBodyRequired"].Value);

            var verifyResponse = await VerifyResetPasswordTokenAsync(
                new VerifyResetPasswordDTO
                {
                    UserId = dto.UserId!,
                    Token = dto.Token!
                }
            );

            if (!verifyResponse.Success)
                return verifyResponse;

            var user = await _userManager.FindByIdAsync(dto.UserId!);
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            var resetResult = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);


            if (!resetResult.Succeeded)
                return ApiResponseFactory.Failure(_localizer["FailedToResetPassword"].Value, 400, resetResult.Errors.Select(e => e.Description).ToArray());

            await _userManager.RemoveAuthenticationTokenAsync(user, "ResetPassword", "Token");
            await _userManager.RemoveAuthenticationTokenAsync(user, "ResetPassword", "TokenTime");

            return ApiResponseFactory.Success(_localizer["PasswordResetSuccessful"].Value);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> RefreshTokenAsync(TokenModel model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.RefreshToken))
                return ApiResponseFactory.Failure(_localizer["InvalidTokenModel"].Value, 400, _localizer["TokenAndRefreshRequired"].Value);

            ClaimsPrincipal? principal;

            try
            {
                principal = _jwtService.GetPrincipalFromJwtToken(model.Token);
            }
            catch (SecurityTokenException ex)
            {
                return ApiResponseFactory.Failure(_localizer["InvalidToken"].Value, 400, _localizer["AccessTokenInvalid"].Value);
            }

            if (principal is null)
                return ApiResponseFactory.Failure(_localizer["InvalidToken"].Value, 400, _localizer["AccessTokenInvalid"].Value);

            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return ApiResponseFactory.Failure(_localizer["InvalidToken"].Value, 400, _localizer["EmailClaimMissing"].Value);

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["EmailNotFound"].Value);

            if (user.RefreshToken != model.RefreshToken ||
                user.RefreshTokenExpirationDateTime <= DateTimeOffset.UtcNow)
            {
                return ApiResponseFactory.Failure(_localizer["InvalidRefreshToken"].Value, 400, _localizer["RefreshTokenInvalidOrExpired"].Value);
            }


            bool rememberMe = bool.TryParse(principal.FindFirst("remember_me")?.Value, out var rm) && rm;

            var authResponse = await _jwtService.CreateJwtToken(user, rememberMe) as ApiSuccessResponse;


            // Rotate refresh token
            user.RefreshToken = authResponse?.RefreshToken;
            user.RefreshTokenExpirationDateTime = authResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(user);

            authResponse.Success = true;
            authResponse.StatusCode = 200;
            authResponse.Message = _localizer["TokenRefreshedSuccessfully"].Value;

            return authResponse;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAddress(string? email, Address? address)
        {
            if (email is null || address is null)
                return false;

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return false;

            return await _unitOfWork.AuthenticationRepository.UpdateAddress(user.Id, address);
        }
        /// <inheritdoc/>
        public async Task<ShippingAddressDTO?> GetAddress(string? email)
        {
            if (string.IsNullOrEmpty(email))
                return null;
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return null;
            var address = await _unitOfWork.AuthenticationRepository.GetAddress(user.Id);
            if (address is null) return null;
            return _mapper.Map<ShippingAddressDTO>(address);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> LogoutAsync(string? email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpirationDateTime = DateTimeOffset.MinValue;
                    await _userManager.UpdateAsync(user);
                }
            }

            await _signInManager.SignOutAsync();

            return ApiResponseFactory.Success(_localizer["LoggedOutSuccessfully"].Value);
        }
        /// <inheritdoc/>
        public async Task<ApplicationUserResponse?> GetUserByIdAsync(string userId)
            => await _userManagementService.GetUserByIdAsync(userId);
        /// <inheritdoc/>
        public async Task<ApiResponse> UpdateUserProfileAsync(UpdateUserDTO dto)
        {
            if (dto == null)
                return ApiResponseFactory.Failure(_localizer["InvalidUpdateData"].Value, 400, _localizer["UpdateDataRequired"].Value);

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["NoAccountForUserId"].Value);

            if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                user.DisplayName = dto.DisplayName;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ApiResponseFactory.Failure(_localizer["FailedToUpdateProfile"].Value, 400, updateResult.Errors.Select(e => e.Description).ToArray());

            if (!string.IsNullOrWhiteSpace(dto.CurrentPassword) && !string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                if (!passwordResult.Succeeded)
                    return ApiResponseFactory.Failure(_localizer["FailedToChangePassword"].Value, 400, passwordResult.Errors.Select(e => e.Description).ToArray());
            }

            return ApiResponseFactory.Success(_localizer["ProfileUpdatedSuccessfully"].Value);
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> ExternalLoginCallbackAsync(string remoteError = "")
        {
            if (!string.IsNullOrEmpty(remoteError))
                return ApiResponseFactory.Failure(_localizer["ExternalProviderError", remoteError].Value, 400, remoteError);

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return ApiResponseFactory.Failure(_localizer["FailedToLoadExternalLoginInfo"].Value, 400, "external_login_info_missing");

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                return await CreateSuccessLoginResponseAsync(user, false);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                var fallback =
                    info.Principal.FindFirstValue(ClaimTypes.Name)
                    ?? info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(fallback))
                {
                    return ApiResponseFactory.Failure(
                        _localizer["ExternalProviderMissingIdentityData"].Value,
                        400,
                        "missing_identity_data"
                    );
                }

                email = fallback;
            }

            user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                if (!user.EmailConfirmed)
                {
                    return ApiResponseFactory.Failure(
                        _localizer["ConfirmEmailBeforeLinking"].Value,
                        403,
                        "email_not_confirmed"
                    );
                }

                var linkResult = await _userManager.AddLoginAsync(user, info);
                if (!linkResult.Succeeded)
                {
                    return ApiResponseFactory.Failure(
                        _localizer["FailedToLinkExternalProvider"].Value,
                        500,
                        linkResult.Errors.Select(e => e.Description).ToArray()
                    );
                }

                return await CreateSuccessLoginResponseAsync(user, false);
            }

            var profileImageUrl =
                info.Principal.FindFirstValue("picture")   
                ?? info.Principal.FindFirstValue("avatar_url")
                ?? info.Principal.FindFirstValue("urn:google:picture");

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = info.Principal.FindFirstValue(ClaimTypes.Name),
                EmailConfirmed = true
            };

            var randomPassword = PasswordGenerator.Generate(32);

            var createResult = await _userManager.CreateAsync(user, randomPassword);
            if (!createResult.Succeeded)
            {
                return ApiResponseFactory.Failure(
                    _localizer["FailedToCreateAccountFromExternal"].Value,
                    500,
                    createResult.Errors.Select(e => e.Description).ToArray()
                );
            }

            await EnsureRoleExistsAndAssignAsync(user, UserTypeOptions.User.ToString());

            var loginResult = await _userManager.AddLoginAsync(user, info);
            if (!loginResult.Succeeded)
            {
                return ApiResponseFactory.Failure(
                    _localizer["FailedToLinkExternalLogin"].Value,
                    500,
                    loginResult.Errors.Select(e => e.Description).ToArray()
                );
            }

            if (!string.IsNullOrEmpty(profileImageUrl))
            {
                try
                {
                    await _imageService.ImportExternalAvatarAsync(user, profileImageUrl);
                    await _userManager.UpdateAsync(user);
                }
                catch
                {
                }
            }

            return await CreateSuccessLoginResponseAsync(user, false);
        }


        private bool IsMobileDevice(HttpRequest? request)
        {
            if (request is null) return false;

            var userAgent = request.Headers["User-Agent"].ToString().ToLower();
            var src = request.Headers["X-Source"].ToString().ToLower();

            return userAgent.Contains("android") ||
                userAgent.Contains("iphone") ||
                userAgent.Contains("ipad") ||
                userAgent.Contains("mobile") ||
                userAgent.Contains("opera mini") ||
                userAgent.Contains("flutter") ||
                src.Contains("flutter");
        }

        //private async Task SendEmail(ApplicationUser user)
        //{
        //    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        //    user.LastEmailConfirmationToken = token;

        //    await _userManager.UpdateAsync(user);

        //    var request = _httpContextAccessor.HttpContext?.Request;
        //    var scheme = request?.Scheme ?? "https";
        //    var host = request?.Host.Value ?? "localhost:5000";

        //    string redirectUrl = $"{scheme}://{host}/email-confirmed";

        //    string confirmationLink = $"{scheme}://{host}/api/v2/frontend/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}&redirectTo={Uri.EscapeDataString(redirectUrl)}";

        //    string html = EmailTemplateService.GetConfirmationEmailTemplate(confirmationLink);

        //    var emailDTO = new EmailDTO(user.Email, "Confirm Your Email", html);
        //    await _emailSender.SendEmailAsync(emailDTO);
        //}

        private async Task SendEmail(ApplicationUser user, string? apiKey)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            user.LastEmailConfirmationToken = token;
            await _userManager.UpdateAsync(user);

            var frontendBaseUrl = _configuration["FrontURLs:BaseUrl"];

            var redirectUrl = "";

            if (!string.IsNullOrEmpty(apiKey))
            {
                var client = await _unitOfWork.ApiClientRepository.GetByApiKeyAsync(apiKey);
                redirectUrl = client?.AccountActivationCallbackUrl ?? "";
            }

            var confirmationLink =
                $"{frontendBaseUrl}/auth/confirm-email" +
                $"?userId={user.Id}" +
                $"&token={Uri.EscapeDataString(token)}" +
                $"&redirectTo={Uri.EscapeDataString(redirectUrl)}";

            string html = EmailTemplateService.GetConfirmationEmailTemplate(confirmationLink, CultureInfo.CurrentUICulture.Name);
            var emailDTO = new EmailDTO(user.Email, Res.Confirmation_Subject, html);
            await _emailSender.SendEmailAsync(emailDTO);
        }

        private async Task EnsureRoleExistsAndAssignAsync(ApplicationUser user, string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });

            await _userManager.AddToRoleAsync(user, roleName);
        }
        private async Task<ApiSuccessResponse> CreateSuccessLoginResponseAsync(ApplicationUser user, bool rememberMe)
        {
            var tokenResponse = await _jwtService.CreateJwtToken(user, rememberMe) as ApiSuccessResponse;
            user.RefreshToken = tokenResponse?.RefreshToken;
            user.RefreshTokenExpirationDateTime = tokenResponse.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);
            tokenResponse.Success = true;
            tokenResponse.Message = _localizer["LoginSuccessful"].Value;
            tokenResponse.StatusCode = 200;
            return tokenResponse;
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return ApiResponseFactory.NotFound(_localizer["UserNotFound"].Value);

            var deleted = await _userManager.DeleteAsync(user);

            if (!deleted.Succeeded)
                return ApiResponseFactory.InternalServerError(_localizer["FailedToDeleteAccount"].Value, deleted.Errors.Select(e => e.Description).ToList());

            return ApiResponseFactory.Success(_localizer["AccountDeletedSuccessfully"].Value);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> ResendConfirmationEmailAsync(string email, string? apiKey = null)
        {
            if (string.IsNullOrEmpty(email))
                return ApiResponseFactory.BadRequest(_localizer["EmailIsRequired"].Value);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ApiResponseFactory.NotFound(_localizer["UserNotFound"].Value);

            if (await _userManager.IsEmailConfirmedAsync(user))
                return ApiResponseFactory.Conflict(_localizer["AccountAlreadyConfirmed"].Value);

            var tokenTimeStr = await _userManager.GetAuthenticationTokenAsync(user, "EmailConfirmation", "TokenTime");
            if (!string.IsNullOrEmpty(tokenTimeStr) && DateTimeOffset.TryParse(tokenTimeStr, out var tokenTime))
            {
                if (DateTimeOffset.UtcNow < tokenTime.AddMinutes(5))
                {
                    return ApiResponseFactory.Failure(
                        _localizer["ConfirmationEmailSentRecently"].Value,
                        StatusCodes.Status429TooManyRequests,
                        "TOO_MANY_REQUESTS"
                    );
                }
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _userManager.SetAuthenticationTokenAsync(user, "EmailConfirmation", "Token", token);
            await _userManager.SetAuthenticationTokenAsync(user, "EmailConfirmation", "TokenTime", DateTimeOffset.UtcNow.ToString("o"));

            await SendEmail(user, apiKey);

            return ApiResponseFactory.Success(_localizer["ConfirmationEmailResentSuccessfully"].Value);
        }

        //public async Task<ApiResponse> UploadUserPhotoAsync(Guid userId, IFormFile file)
        //{
        //    var user = await _userManager.Users
        //        .Include(u => u.Photo)
        //        .FirstOrDefaultAsync(u => u.Id == userId);

        //    if (user == null)
        //        return ApiResponseFactory.NotFound("User not found.");

        //    if (file == null)
        //        return ApiResponseFactory.BadRequest("No file provided.");

        //    if (user.Photo != null)
        //    {
        //        _imageService.DeleteImageAsync(user.Photo.ImageName);
        //        await _unitOfWork.PhotoRepository.DeleteAsync(user.Photo.Id);
        //        user.Photo = null;
        //    }

        //    var folderName = user.UserName.Replace(" ", "").ToLowerInvariant();

        //    var formFileCollection = new FormFileCollection { file };
        //    var imagePaths = await _imageService.AddImageAsync(formFileCollection, $"Users/{folderName}");

        //    user.Photo = new Photo
        //    {
        //        ImageName = imagePaths.First(),
        //        UserId = userId
        //    };

        //    await _unitOfWork.CompleteAsync();
        //    await _userManager.UpdateAsync(user);

        //    return ApiResponseFactory.Success("User photo uploaded successfully.");
        //}
        public async Task<ApiResponse> UploadUserPhotoAsync(Guid userId, UploadUserPhotoDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.Photo)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponseFactory.NotFound(_localizer["UserNotFound"].Value);

            var file = dto.File;
            if (file == null || file.Length == 0)
                return ApiResponseFactory.BadRequest(_localizer["NoFileProvided"].Value);

            if (!file.ContentType.StartsWith("image/"))
                return ApiResponseFactory.BadRequest(_localizer["InvalidImageType"].Value);

            if (file.Length > 5 * 1024 * 1024)
                return ApiResponseFactory.BadRequest(_localizer["ImageSizeExceedsLimit"].Value);

            if (user.Photo != null)
            {
                _imageService.DeleteImageAsync(user.Photo.ImageName);
                await _unitOfWork.PhotoRepository.DeleteAsync(user.Photo.Id);
                user.Photo = null;
            }

            var folderName = user.UserName.Replace(" ", "").ToLowerInvariant();

            var imagePath = await _imageService.SaveUserAvatarAsync(
                file,
                $"Users/{folderName}",
                dto.Crop
            );

            user.Photo = new Photo
            {
                ImageName = imagePath,
                UserId = userId
            };

            await _unitOfWork.CompleteAsync();
            await _userManager.UpdateAsync(user);

            return ApiResponseFactory.Success(_localizer["UserPhotoUploadedSuccessfully"].Value);
        }


        public async Task<ApiResponse> DeleteUserPhotoAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Photo)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponseFactory.NotFound(_localizer["UserNotFound"].Value);

            if (user.Photo == null)
                return ApiResponseFactory.BadRequest(_localizer["UserHasNoPhotoToDelete"].Value);

            _imageService.DeleteImageAsync(user.Photo.ImageName);
            await _unitOfWork.PhotoRepository.DeleteAsync(user.Photo.Id);
            user.Photo = null;

            await _unitOfWork.CompleteAsync();
            await _userManager.UpdateAsync(user);

            return ApiResponseFactory.Success(_localizer["UserPhotoDeletedSuccessfully"].Value);
        }



    }
}
