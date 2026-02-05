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
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

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
            IConfiguration configuration) : base(unitOfWork, mapper)
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
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> RegisterAsync(RegisterDTO registerDTO, string? clientKey)
        {
            if (registerDTO == null)
                return ApiResponseFactory.Failure("Invalid registration data.", 400, "Registration data cannot be null.");
            ValidationHelper.ModelValidation(registerDTO);

            if (await _userManager.FindByEmailAsync(registerDTO.Email) is not null)
                return ApiResponseFactory.Failure("Email is already registered.", 409, "This email is already in use.");



            ApplicationUser user = new ApplicationUser()
            {
                DisplayName = registerDTO.UserName,
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.Phone
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
                return ApiResponseFactory.Failure("Registration failed.", 400, result.Errors.Select(e => e.Description).ToArray());


            await EnsureRoleExistsAndAssignAsync(user, UserTypeOptions.User.ToString());

            await SendEmail(user, clientKey);

            return ApiResponseFactory.Success("Registration successful. Please check your email to confirm your account.");
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> LoginAsync(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                return ApiResponseFactory.Failure("Invalid login data.", 400, "Login data cannot be null.");

            ValidationHelper.ModelValidation(loginDTO);

            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
                return ApiResponseFactory.Failure("User not found.", 404, "No account found with this email.");


            if (!user.EmailConfirmed)
            {
                //await SendEmail(user);
                return ApiResponseFactory.Failure("Email not confirmed.", 403, "You must confirm your email before logging in.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.RememberMe, true);

            if (result.Succeeded)
            {
                return await CreateSuccessLoginResponseAsync(user, loginDTO.RememberMe);
            }
            else if (result.IsLockedOut)
            {
                string message = "Your account is temporarily locked due to multiple failed login attempts. Please try again later.";
                return ApiResponseFactory.Failure(message, 423, message);
            }
            else if (result.IsNotAllowed)
            {
                return ApiResponseFactory.Failure("User is not allowed to login.", 403, "User is not allowed to login.");
            }
            else
            {
                return ApiResponseFactory.Failure("Invalid login attempt.", 401, "Incorrect email or password.");
            }
        }


        /// <inheritdoc/>
        public async Task<ApiResponse> ConfirmEmailAsync(ConfirmEmailDTO dto)
        {
            ValidationHelper.ModelValidation(dto);
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponseFactory.Failure("User not found.", 404, "User with the provided ID does not exist.");

            if (await _userManager.IsEmailConfirmedAsync(user))
                return ApiResponseFactory.Failure("Email is already confirmed.", 200, "Email already confirmed.");

            if (user.LastEmailConfirmationToken != dto.Token)
                return ApiResponseFactory.Failure("Invalid or expired confirmation token.", 400, "Token mismatch or already used.");

            var result = await _userManager.ConfirmEmailAsync(user, dto.Token);

            if (result.Succeeded)
            {
                user.LastEmailConfirmationToken = null;
                await _userManager.RemoveAuthenticationTokenAsync(user, "EmailConfirmation", "Token");
                await _userManager.RemoveAuthenticationTokenAsync(user, "EmailConfirmation", "TokenTime");

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return ApiResponseFactory.Failure("Failed to update user after confirmation.", 500, updateResult.Errors.Select(e => e.Description).ToArray());

                return ApiResponseFactory.Success("Email confirmed successfully.");
            }
            return ApiResponseFactory.Failure("Failed to confirm email.", 400, result.Errors.Select(e => e.Description).ToArray());

        }
        /// <inheritdoc/>
        public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDTO dto, string? clientKey)
        {
            ValidationHelper.ModelValidation(dto);
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return ApiResponseFactory.Failure("Incorrect email.", 400, "Email not found.");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return ApiResponseFactory.Failure("Please confirm your email before resetting password.", 400, "Email is not confirmed.");

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
                        return ApiResponseFactory.Failure("A password reset email was already sent recently. Please wait before trying again.", 429, "Reset already requested.");
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

            string html = EmailTemplateService.GetPasswordResetEmailTemplate(resetLink);

            var emailDTO = new EmailDTO(user.Email, "Reset Your Password", html);

            await _emailSender.SendEmailAsync(emailDTO);

            return ApiResponseFactory.Success("A password reset link has been sent to your email.");
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> VerifyResetPasswordTokenAsync(VerifyResetPasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.Token))
                return ApiResponseFactory.Failure("Invalid verification request.", 400, "UserId and token are required.");

            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
                return ApiResponseFactory.Failure("User not found.", 404, "No account found for the provided user ID.");

            string decodedToken;

            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            }
            catch
            {
                return ApiResponseFactory.Failure("Invalid token format.", 400, "The token format is invalid or corrupted.");
            }

            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "ResetPassword", "Token");

            if (storedToken == null || storedToken != decodedToken)
                return ApiResponseFactory.Failure("Invalid or expired token.", 400, "The token is invalid or has already been used.");

            var tokenTimeStr = await _userManager.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime");

            if (string.IsNullOrEmpty(tokenTimeStr) || !DateTimeOffset.TryParse(tokenTimeStr, out var tokenTime))
                return ApiResponseFactory.Failure("Token validation failed.", 400, "The token timestamp is invalid.");

            if (DateTimeOffset.UtcNow > tokenTime.AddMinutes(5))
                return ApiResponseFactory.Failure("Reset password link has expired.", 400, "The reset password link has expired. Please request a new one.");

            return ApiResponseFactory.Success("The reset password token is valid.");
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            if (dto == null)
                return ApiResponseFactory.Failure("Invalid reset password data.", 400, "Request body cannot be null.");

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
                return ApiResponseFactory.Failure("Failed to reset password.", 400, resetResult.Errors.Select(e => e.Description).ToArray());

            await _userManager.RemoveAuthenticationTokenAsync(user, "ResetPassword", "Token");
            await _userManager.RemoveAuthenticationTokenAsync(user, "ResetPassword", "TokenTime");

            return ApiResponseFactory.Success("Password has been reset successfully.");
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> RefreshTokenAsync(TokenModel model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.RefreshToken))
                return ApiResponseFactory.Failure("Invalid token model.", 400, "Token and refresh token are required.");

            ClaimsPrincipal? principal;

            try
            {
                principal = _jwtService.GetPrincipalFromJwtToken(model.Token);
            }
            catch (SecurityTokenException ex)
            {
                return ApiResponseFactory.Failure("Invalid token.", 400, "Access token is invalid.");
            }

            if (principal is null)
                return ApiResponseFactory.Failure("Invalid token.", 400, "Access token is invalid.");

            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return ApiResponseFactory.Failure("Invalid token.", 400, "Email claim is missing in token.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return ApiResponseFactory.Failure("User not found.", 404, "User does not exist.");

            if (user.RefreshToken != model.RefreshToken ||
                user.RefreshTokenExpirationDateTime <= DateTimeOffset.UtcNow)
            {
                return ApiResponseFactory.Failure("Invalid refresh token.", 400, "Refresh token is invalid or expired.");
            }


            bool rememberMe = bool.TryParse(principal.FindFirst("remember_me")?.Value, out var rm) && rm;

            var authResponse = await _jwtService.CreateJwtToken(user, rememberMe) as ApiSuccessResponse;


            // Rotate refresh token
            user.RefreshToken = authResponse?.RefreshToken;
            user.RefreshTokenExpirationDateTime = authResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(user);

            authResponse.Success = true;
            authResponse.StatusCode = 200;
            authResponse.Message = "Token refreshed successfully.";

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

            return ApiResponseFactory.Success("Logged out successfully.");
        }
        /// <inheritdoc/>
        public async Task<ApplicationUserResponse?> GetUserByIdAsync(string userId)
            => await _userManagementService.GetUserByIdAsync(userId);
        /// <inheritdoc/>
        public async Task<ApiResponse> UpdateUserProfileAsync(UpdateUserDTO dto)
        {
            if (dto == null)
                return ApiResponseFactory.Failure("Invalid update data.", 400, "Update data cannot be null.");

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponseFactory.Failure("User not found.", 404, "No user found with the provided ID.");

            if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                user.DisplayName = dto.DisplayName;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ApiResponseFactory.Failure("Failed to update profile.", 400, updateResult.Errors.Select(e => e.Description).ToArray());

            if (!string.IsNullOrWhiteSpace(dto.CurrentPassword) && !string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                if (!passwordResult.Succeeded)
                    return ApiResponseFactory.Failure("Failed to change password.", 400, passwordResult.Errors.Select(e => e.Description).ToArray());
            }

            return ApiResponseFactory.Success("Profile updated successfully.");
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> ExternalLoginCallbackAsync(string remoteError = "")
        {
            if (!string.IsNullOrEmpty(remoteError))
                return ApiResponseFactory.Failure($"External provider error: {remoteError}", 400, remoteError);

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return ApiResponseFactory.Failure("Failed to load external login info.", 400, "external_login_info_missing");

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
                        "External provider did not supply email or username.",
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
                        "Please confirm your email before linking an external login.",
                        403,
                        "email_not_confirmed"
                    );
                }

                var linkResult = await _userManager.AddLoginAsync(user, info);
                if (!linkResult.Succeeded)
                {
                    return ApiResponseFactory.Failure(
                        "Failed to link external provider to existing account.",
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
                    "Failed to create account from external login.",
                    500,
                    createResult.Errors.Select(e => e.Description).ToArray()
                );
            }

            await EnsureRoleExistsAndAssignAsync(user, UserTypeOptions.User.ToString());

            var loginResult = await _userManager.AddLoginAsync(user, info);
            if (!loginResult.Succeeded)
            {
                return ApiResponseFactory.Failure(
                    "Failed to link external login.",
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

            string html = EmailTemplateService.GetConfirmationEmailTemplate(confirmationLink);

            var emailDTO = new EmailDTO(user.Email, "Confirm Your Email", html);

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
            tokenResponse.Message = "Login successful.";
            tokenResponse.StatusCode = 200;
            return tokenResponse;
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return ApiResponseFactory.NotFound("User not found");

            var deleted = await _userManager.DeleteAsync(user);

            if (!deleted.Succeeded)
                return ApiResponseFactory.InternalServerError("Failed to delete account", deleted.Errors.Select(e => e.Description).ToList());

            return ApiResponseFactory.Success("Account deleted successfully");
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> ResendConfirmationEmailAsync(string email, string? apiKey = null)
        {
            if (string.IsNullOrEmpty(email))
                return ApiResponseFactory.BadRequest("Email is required.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ApiResponseFactory.NotFound("User not found.");

            if (await _userManager.IsEmailConfirmedAsync(user))
                return ApiResponseFactory.Conflict("Account already confirmed.");

            var tokenTimeStr = await _userManager.GetAuthenticationTokenAsync(user, "EmailConfirmation", "TokenTime");
            if (!string.IsNullOrEmpty(tokenTimeStr) && DateTimeOffset.TryParse(tokenTimeStr, out var tokenTime))
            {
                if (DateTimeOffset.UtcNow < tokenTime.AddMinutes(5))
                {
                    return ApiResponseFactory.Failure(
                        "Confirmation email already sent recently. Please wait before requesting again.",
                        StatusCodes.Status429TooManyRequests,
                        "TOO_MANY_REQUESTS"
                    );
                }
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _userManager.SetAuthenticationTokenAsync(user, "EmailConfirmation", "Token", token);
            await _userManager.SetAuthenticationTokenAsync(user, "EmailConfirmation", "TokenTime", DateTimeOffset.UtcNow.ToString("o"));

            await SendEmail(user, apiKey);

            return ApiResponseFactory.Success("Confirmation email resent successfully.");
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
                return ApiResponseFactory.NotFound("User not found.");

            var file = dto.File;
            if (file == null || file.Length == 0)
                return ApiResponseFactory.BadRequest("No file provided.");

            if (!file.ContentType.StartsWith("image/"))
                return ApiResponseFactory.BadRequest("Invalid image type.");

            if (file.Length > 5 * 1024 * 1024)
                return ApiResponseFactory.BadRequest("Image size exceeds limit.");

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

            return ApiResponseFactory.Success("User photo uploaded successfully.");
        }


        public async Task<ApiResponse> DeleteUserPhotoAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Photo)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponseFactory.NotFound("User not found.");

            if (user.Photo == null)
                return ApiResponseFactory.BadRequest("User has no photo to delete.");

            _imageService.DeleteImageAsync(user.Photo.ImageName);
            await _unitOfWork.PhotoRepository.DeleteAsync(user.Photo.Id);
            user.Photo = null;

            await _unitOfWork.CompleteAsync();
            await _userManager.UpdateAsync(user);

            return ApiResponseFactory.Success("User photo deleted successfully.");
        }



    }
}
