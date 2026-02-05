using Asp.Versioning;
using AutoMapper;
using Domain.Entities.Common;
using EStoreX.Core.Domain.IdentityEntities;
using EStoreX.Core.DTO.Account.Requests;
using EStoreX.Core.DTO.Account.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IAuthService = EStoreX.Core.ServiceContracts.Account.IAuthenticationService;

namespace E_StoreX.API.Controllers.Public
{
    /// <summary>
    /// Controller responsible for handling user authentication-related actions
    /// such as registration, login, email confirmation, and password reset for the E-StoreX API.
    /// </summary>
    [ApiVersion(1.0)]
    public class AccountController : CustomControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IApiClientService _clientService;
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authService">
        /// The authentication service that handles user registration, login, 
        /// email confirmation, and password reset logic.
        /// </param>
        /// <param name="mapper">
        /// mapper instance for mapping between DTOs and domain entities.
        /// </param>
        /// <param name="signInManager">
        /// sign-in manager for handling user sign-in operations,
        /// </param>
        /// <param name="clientService">
        /// Service to validate client id and platform.
        /// </param>
        public AccountController(IAuthService authService, IMapper mapper, SignInManager<ApplicationUser> signInManager, IApiClientService clientService)
        {
            _authService = authService;
            _mapper = mapper;
            _signInManager = signInManager;
            _clientService = clientService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="registerDTO">
        /// An object containing user registration details like username, email, and password.
        /// </param>
        /// <returns>
        /// Returns a response depending on the outcome of the registration process.
        /// </returns>
        /// <response code="200">Registration succeeded. The user account has been created successfully.</response>
        /// <response code="400">
        /// Registration failed due to invalid input.
        /// For example:
        /// - Missing required fields (username, email, or password)
        /// - Password does not meet security requirements
        /// - Email format is invalid
        /// </response>
        /// <response code="409">
        /// Registration failed because the email or username is already in use.
        /// The system does not allow duplicate accounts with the same email.
        /// </response>
        [HttpPost("register")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostRegister([FromBody] RegisterDTO registerDTO)
        {
            var clientKey = HttpContext.Request.Headers["X-API-KEY"].FirstOrDefault();
            var response = await _authService.RegisterAsync(registerDTO, clientKey);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Authenticates an existing user and generates a JWT token.
        /// </summary>
        /// <param name="loginDTO">
        /// The user's login credentials (email and password).
        /// </param>
        /// <returns>
        /// Returns a response based on the login outcome.
        /// </returns>
        /// <response code="200">
        /// Login succeeded. Returns <see cref="ApiSuccessResponse"/> with a JWT token and refresh token.
        /// </response>
        /// <response code="400">
        /// Bad Request – Input invalid. Examples include:
        /// - loginDTO is null
        /// - Email or password missing
        /// - Invalid input format
        /// Returns <see cref="ApiErrorResponse"/> with details.
        /// </response>
        /// <response code="401">
        /// Unauthorized – Invalid credentials. 
        /// The email/password combination does not match any user account.
        /// Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="403">
        /// Forbidden – User's email is not confirmed yet. 
        /// The system requires email verification before login.
        /// Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User does not exist with the provided email.
        /// Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="423">
        /// Locked – Account temporarily locked due to multiple failed login attempts.
        /// Returns <see cref="ApiErrorResponse"/> explaining the lockout period.
        /// </response>
        [HttpPost("login")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        public async Task<IActionResult> PostLogin([FromBody] LoginDTO loginDTO)
        {
            var response = await _authService.LoginAsync(loginDTO);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Confirms a user's email using the provided user ID and token.
        /// </summary>
        /// <param name="dto">
        /// Contains the user ID, email confirmation token, and an optional redirect URL.
        /// </param>
        /// <returns>
        /// Returns a response based on the confirmation outcome.
        /// </returns>
        /// <response code="200">
        /// Email confirmed successfully. Returns <see cref="ApiResponse"/> with success message.
        /// </response>
        /// <response code="400">
        /// Bad Request – Invalid confirmation data. Examples include:
        /// - dto is null
        /// - UserId or Token is missing
        /// - Token format invalid
        /// Returns <see cref="ApiErrorResponse"/> with details.
        /// </response>
        /// <response code="404">
        /// Not Found – User with the provided ID does not exist.
        /// Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpGet("confirm-email")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.UserId) || string.IsNullOrEmpty(dto.Token))
            {
                return BadRequest(ApiResponseFactory.BadRequest("Invalid confirmation data."));// BadRequest();
            }
            var response = await _authService.ConfirmEmailAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Sends a password reset link to the user's email address.
        /// </summary>
        /// <param name="dto">
        /// Contains the email address of the user who requested a password reset.
        /// </param>
        /// <returns>
        /// Returns a response based on the outcome of the request.
        /// </returns>
        /// <response code="200">
        /// Password reset link sent successfully. Returns <see cref="ApiResponse"/> with success message.
        /// </response>
        /// <response code="400">
        /// Bad Request – Invalid input. Examples include:
        /// - dto is null
        /// - Email is missing or malformed
        /// Returns <see cref="ApiErrorResponse"/> with details.
        /// </response>
        /// <response code="429">
        /// Too Many Requests – User has requested password reset too frequently.
        /// Returns <see cref="ApiErrorResponse"/> indicating rate limiting.
        /// </response>
        [HttpPost("forgot-password")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            var clientKey = HttpContext.Request.Headers["X-API-KEY"].FirstOrDefault();
            var response = await _authService.ForgotPasswordAsync(dto, clientKey);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Verifies the validity of a reset password token for a given user.
        /// </summary>
        /// <param name="dto">
        /// Contains the User ID and reset token to validate.
        /// </param>
        /// <returns>
        /// Returns a response indicating whether the token is valid or not.
        /// </returns>
        /// <response code="200">
        /// Token is valid. Returns <see cref="ApiResponse"/> confirming token validity.
        /// </response>
        /// <response code="400">
        /// Bad Request – Invalid input. Examples include:
        /// - dto is null
        /// - UserId or Token is missing
        /// Returns <see cref="ApiErrorResponse"/> with error details.
        /// </response>
        /// <response code="404">
        /// Not Found – Token is invalid, expired, or user does not exist.
        /// Returns <see cref="ApiErrorResponse"/> indicating the problem.
        /// </response>
        [HttpGet("reset-password/verify")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyResetPassword([FromQuery] VerifyResetPasswordDTO dto)
        {
            var response = await _authService.VerifyResetPasswordTokenAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Resets the user's password using a valid token.
        /// </summary>
        /// <param name="dto">
        /// Contains user ID, reset token, and new password details.
        /// </param>
        /// <returns>
        /// Returns a response indicating whether the password was successfully reset.
        /// </returns>
        /// <response code="200">
        /// Password reset succeeded. Returns <see cref="ApiResponse"/> confirming the update.
        /// </response>
        /// <response code="400">
        /// Bad Request – Input is invalid or password rules are not met. Examples include:
        /// - dto is null
        /// - UserId, Token, or NewPassword missing
        /// - Password does not meet complexity requirements
        /// Returns <see cref="ApiErrorResponse"/> with detailed error messages.
        /// </response>
        /// <response code="404">
        /// Not Found – User not found or token invalid/expired. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpPost("reset-password")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Generates a new access token (and refresh token) using a valid refresh token.
        /// </summary>
        /// <param name="model">
        /// Contains the expired access token and its corresponding refresh token.
        /// </param>
        /// <returns>
        /// Returns a response indicating whether the token refresh succeeded.
        /// </returns>
        /// <response code="200">
        /// Token refreshed successfully. Returns <see cref="ApiSuccessResponse"/> containing the new JWT and refresh token.
        /// </response>
        /// <response code="400">
        /// Bad Request – Input is invalid, missing, or the refresh token is expired/does not match the user. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User associated with the token does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpPost("generate-new-jwt-token")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel model)
        {
            var response = await _authService.RefreshTokenAsync(model);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Updates the authenticated user's shipping address.
        /// </summary>
        /// <param name="addressDTO">
        /// The <see cref="ShippingAddressDTO"/> containing the new address information.
        /// </param>
        /// <returns>
        /// Returns a response indicating whether the address update was successful.
        /// </returns>
        /// <response code="200">
        /// Address updated successfully. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – Input is null, invalid, or address update failed. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated. Returns <see cref="ApiResponse"/>.
        /// </response>
        [Authorize]
        [HttpPatch("update-address")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAddress([FromBody] ShippingAddressDTO addressDTO)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var address = _mapper.Map<Address>(addressDTO);
            bool ok = await _authService.UpdateAddress(email, address);
            if (ok)
                return Ok(ApiResponseFactory.Success("Address updated successfully."));
            return BadRequest(ApiResponseFactory.BadRequest("Failed to update address."));
        }

        /// <summary>
        /// Retrieves the shipping address of the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// Requires the user to be authenticated. The user's email is extracted from the JWT claims
        /// and used to fetch the associated shipping address from the authentication service.
        /// </remarks>
        /// <returns>
        /// Returns the shipping address if found, or an error response if retrieval fails.
        /// </returns>
        /// <response code="200">
        /// Shipping address retrieved successfully. Returns <see cref="ShippingAddressDTO"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – User email is missing in claims or address could not be retrieved. Returns <see cref="ApiErrorResponse"/> or <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – User is not authenticated. Returns <see cref="ApiResponse"/>.
        /// </response>
        [HttpGet("get-address")]
        [Authorize]
        [ProducesResponseType(typeof(ShippingAddressDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAddress()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var shippingAddress = await _authService.GetAddress(email);
            if (shippingAddress == null)
                return BadRequest(ApiResponseFactory.BadRequest("Failed to get address."));
            return Ok(shippingAddress);
        }
        /// <summary>
        /// Logs out the currently authenticated user by clearing their refresh token and signing them out.
        /// </summary>
        /// <remarks>
        /// Requires the user to be authenticated. The user's email is extracted from the JWT claims
        /// to identify the account to log out. Refresh tokens are cleared and the user is signed out.
        /// </remarks>
        /// <returns>
        /// Returns a success message if logout succeeds, or an unauthorized response if the user is not authenticated.
        /// </returns>
        /// <response code="200">
        /// Logout successful. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated. Returns <see cref="ApiResponse"/>.
        /// </response>
        [HttpGet("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostLogout()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var response = await _authService.LogoutAsync(email);
            return StatusCode(response.StatusCode, response);
        }
        /// <summary>
        /// Retrieves information about the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// Requires the user to be authenticated. The endpoint reads the user ID from the JWT claims
        /// and fetches the corresponding user details from the authentication service.
        /// </remarks>
        /// <returns>
        /// Returns detailed information about the authenticated user.
        /// </returns>
        /// <response code="200">
        /// User found and returned successfully. Returns <see cref="ApplicationUserResponse"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – No user ID found in claims. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApplicationUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponseFactory.BadRequest("User ID not found in claims."));

            var userResponse = await _authService.GetUserByIdAsync(userId);
            if (userResponse == null)
                return NotFound(ApiResponseFactory.NotFound("User not found."));

            return Ok(userResponse);
        }
        /// <summary>
        /// Updates the authenticated user's profile information, including display name, 
        /// phone number, and optionally their password.
        /// </summary>
        /// <param name="dto">
        /// An <see cref="UpdateUserDTO"/> object containing the new profile data, 
        /// including current password and new password if the password is being changed.
        /// </param>
        /// <returns>
        /// Returns detailed result of the update operation.
        /// </returns>
        /// <response code="200">
        /// Profile updated successfully. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – Invalid update data, e.g., null DTO or invalid fields. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – User is not authenticated. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="403">
        /// Forbidden – User ID in token does not match the DTO's user ID. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpPatch("update-profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserDTO dto)
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != dto.UserId)
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden());

            var result = await _authService.UpdateUserProfileAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Initiates the external login process by redirecting the user to the chosen authentication provider (e.g., Google, GitHub).
        /// </summary>
        /// <param name="provider">
        /// The external authentication provider to use (e.g., "Google", "GitHub").
        /// </param>
        /// <param name="clientId">
        /// Unique identifier for the calling client.
        /// </param>
        /// <returns>
        /// Returns a challenge result that redirects the user to the external provider's login page.
        /// </returns>
        /// <remarks>
        /// Configures the external authentication properties, sets the redirect URL 
        /// to handle the provider's callback, and prompts the user to select an account.
        /// </remarks>
        /// <response code="302">
        /// Redirect/Challenge to external provider's login page.
        /// </response>
        /// <response code="400">
        /// Bad Request – The 'provider' query parameter is missing or invalid. Returns <see cref="ApiResponse"/>.
        /// </response>
        [HttpGet("external-login")]
        [ProducesResponseType(StatusCodes.Status302Found)] // Redirect/Challenge
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public IActionResult ExternalLogin([FromQuery] ExternalLoginProvider provider, [FromQuery] Guid clientId)
        {
            var providerName = provider.ToString();
            if (string.IsNullOrEmpty(providerName))
                return BadRequest(ApiResponseFactory.BadRequest("Provider is required."));
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), nameof(AccountController)) ?? "api/v1/Account/external-login-callback";

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(providerName, redirectUrl);

            properties.Items["prompt"] = "select_account";
            properties.Items["clientId"] = clientId.ToString();

            return Challenge(properties, providerName);
        }

        /// <summary>
        /// Handles the callback from the external login provider after the authentication attempt.
        /// </summary>
        /// <param name="remoteError">
        /// Optional error message returned by the external provider during the authentication process.
        /// </param>
        /// <returns>
        /// Returns the authentication response including status code and relevant data or error messages.
        /// </returns>
        /// <remarks>
        /// Processes the external login result by calling <see cref="_authService.ExternalLoginCallbackAsync"/>.
        /// </remarks>
        /// <response code="200">
        /// Login successful. Returns <see cref="ApiSuccessResponse"/> with user info and JWT tokens.
        /// </response>
        /// <response code="400">
        /// Bad Request – Remote error returned by the external provider, or missing required external login info.
        /// </response>
        /// <response code="401">
        /// Unauthorized – User is not authenticated or external login could not be linked.
        /// </response>
        /// <response code="409">
        /// Conflict – The email from the external provider is already registered with an existing account.
        /// </response>
        /// <response code="500">
        /// Internal Server Error – Failed to create user account or link external login due to server error.
        /// </response>

        [HttpGet("external-login-callback")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExternalLoginCallback(string remoteError = "")
        {
            var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            var clientId = authResult.Properties?.Items["clientId"] ?? "";

            if (!Guid.TryParse(clientId, out var id))
                return BadRequest(ApiResponseFactory.BadRequest());

            var response = await _authService.ExternalLoginCallbackAsync(remoteError);

            if (response.Success)
            {
                var client = await _clientService.GetClientAsync(id);
                var res = response as ApiSuccessResponse;

                var queryParams = ApiResponseFactory.BuildAuthQueryParams(res);

                var redirectUrl = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
                    client.OAuthCallbackUrl,
                    queryParams
                );

                return Redirect(redirectUrl);
            }


            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Permanently deletes the currently authenticated user's account.
        /// </summary>
        /// <remarks>
        /// This action requires authentication. Once deleted, the account cannot be restored.
        /// </remarks>
        /// <response code="200">Account deleted successfully.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">An error occurred while deleting the account.</response>
        [HttpDelete("delete")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseFactory.Unauthorized("Invalid user."));
            var result = await _authService.DeleteAccountAsync(userId);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Resends the account confirmation email to the specified user.
        /// </summary>
        /// <remarks>
        /// Use this endpoint if you haven't received or have lost the original confirmation email.  
        /// Provide your registered email address, and the system will send a new confirmation email 
        /// to that address if the account exists and is not already confirmed.
        /// </remarks>
        /// <param name="request">The request containing the email address of the user.</param>
        /// <returns>
        /// <c>200 OK</c> if the confirmation email was resent successfully;  
        /// <c>400 Bad Request</c> if the email is already confirmed or the input is invalid;  
        /// <c>404 Not Found</c> if no account exists with the provided email;  
        /// <c>500 Internal Server Error</c> for unexpected failures during the email sending process.
        /// </returns>
        /// <response code="200">Confirmation email resent successfully.</response>
        /// <response code="400">The email is already confirmed or invalid request.</response>
        /// <response code="404">No user found with the provided email.</response>
        /// <response code="500">An unexpected error occurred while sending the confirmation email.</response>
        [HttpPost("resend-confirmation-email")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(ApiResponseFactory.BadRequest("Email is required"));

            var clientKey = HttpContext.Request.Headers["X-API-KEY"].FirstOrDefault();
            var response = await _authService.ResendConfirmationEmailAsync(request.Email, clientKey);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Uploads (or replaces) the authenticated user's profile photo.
        /// </summary>
        /// <param name="dto">
        /// The image file to upload. Only one file is allowed.
        /// </param>
        /// <returns>
        /// Returns detailed result of the upload operation.
        /// </returns>
        /// <response code="200">
        /// Photo uploaded successfully. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – No file provided. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – User is not authenticated. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpPatch("upload-photo")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadUserPhoto([FromForm] UploadUserPhotoDto dto)
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdFromToken))
                return StatusCode(StatusCodes.Status401Unauthorized, ApiResponseFactory.Unauthorized());

            var result = await _authService.UploadUserPhotoAsync(Guid.Parse(userIdFromToken), dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Deletes the authenticated user's profile photo.
        /// </summary>
        /// <returns>
        /// Returns detailed result of the delete operation.
        /// </returns>
        /// <response code="200">
        /// Photo deleted successfully. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – User has no photo to delete. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – User is not authenticated. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpDelete("delete-photo")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteUserPhoto()
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdFromToken))
                return StatusCode(StatusCodes.Status401Unauthorized, ApiResponseFactory.Unauthorized());

            var result = await _authService.DeleteUserPhotoAsync(Guid.Parse(userIdFromToken));
            return StatusCode(result.StatusCode, result);
        }

    }
}
