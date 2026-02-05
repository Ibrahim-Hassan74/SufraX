using Domain.Entities.Common;
using EStoreX.Core.DTO.Account.Requests;
using EStoreX.Core.DTO.Account.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Orders.Requests;
using Microsoft.AspNetCore.Http;

namespace EStoreX.Core.ServiceContracts.Account
{
    /// <summary>
    /// Provides authentication-related operations such as user registration, login, 
    /// email confirmation, and password reset.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Registers a new user with the provided registration data.
        /// </summary>
        /// <param name="registerDTO">
        /// Data required for registering a new user.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse"/> indicating whether the 
        /// registration was successful (StatusCode 200) or failed with details 
        /// (e.g., StatusCode 400 or 409 if username/email is already in use).
        /// </returns>
        Task<ApiResponse> RegisterAsync(RegisterDTO registerDTO, string? clientKey);

        /// <summary>
        /// Authenticates a user with the provided login credentials.
        /// </summary>
        /// <param name="loginDTO">
        /// Login data including email and password.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse"/> indicating whether the 
        /// login was successful (StatusCode 200) or failed due to invalid credentials, 
        /// unconfirmed email, or locked account.
        /// </returns>
        Task<ApiResponse> LoginAsync(LoginDTO loginDTO);

        /// <summary>
        /// Confirms a user's email address using the confirmation token.
        /// </summary>
        /// <param name="dto">
        /// Contains the user ID and email confirmation token.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse"/> indicating whether the 
        /// email confirmation was successful (StatusCode 200) or failed due to an 
        /// invalid/expired token (StatusCode 400 or 404).
        /// </returns>
        Task<ApiResponse> ConfirmEmailAsync(ConfirmEmailDTO dto);

        /// <summary>
        /// Initiates the forgot password process by generating a reset token and 
        /// sending a reset link to the user's email.
        /// </summary>
        /// <param name="dto">
        /// Contains the email address of the user requesting password reset.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse"/> indicating whether the 
        /// reset email was sent successfully (StatusCode 200) or failed due to 
        /// invalid email or unconfirmed account.
        /// </returns>
        Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDTO dto, string? clientKey);

        /// <summary>
        /// Verifies the validity of a reset password token for a specific user.
        /// </summary>
        /// <param name="dto">
        /// A <see cref="VerifyResetPasswordDTO"/> containing the User ID and the 
        /// reset password token that needs to be verified.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse"/> indicating whether the 
        /// token is valid (StatusCode 200) or invalid/expired (StatusCode 400 or 404).
        /// </returns>
        Task<ApiResponse> VerifyResetPasswordTokenAsync(VerifyResetPasswordDTO dto);

        /// <summary>
        /// Resets the user's password using a valid reset token.
        /// </summary>
        /// <param name="dto">
        /// Contains user ID, reset token, new password, and confirmation password.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse"/> indicating whether the 
        /// password reset was successful (StatusCode 200) or failed due to token 
        /// issues or validation errors (StatusCode 400 or 404).
        /// </returns>
        Task<ApiResponse> ResetPasswordAsync(ResetPasswordDTO dto);
        /// <summary>
        /// Generates a new JWT (and refresh token) using a valid refresh token.
        /// </summary>
        /// <param name="model">The current (expired) access token and the refresh token.</param>
        /// <returns>
        /// Returns an <see cref="ApiResponse"/> with a new access token on success,
        /// or a failure response when the refresh token is invalid or expired.
        /// </returns>
        Task<ApiResponse> RefreshTokenAsync(TokenModel model);
        /// <summary>
        /// Updates the address information associated with a specific user.
        /// </summary>
        /// <param name="email">
        /// The email address of the user whose address will be updated.
        /// </param>
        /// <param name="address">
        /// The new <see cref="Address"/> information to associate with the user.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the address was updated successfully; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> UpdateAddress(string? email, Address? address);
        /// <summary>
        /// Retrieves the shipping address associated with a user by their email.
        /// </summary>
        /// <param name="email">
        /// The email address of the user. If null, the operation may return a default or empty result depending on implementation.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The task result contains a <see cref="ShippingAddressDTO"/> representing the user's shipping address.
        /// </returns>
        Task<ShippingAddressDTO?> GetAddress(string? email);

        /// <summary>
        /// Logs out the user by clearing their refresh token and signing them out of the application.
        /// </summary>
        /// <param name="email">The email of the user to log out. If null, only signs out without clearing refresh token.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating whether the logout operation was successful.
        /// </returns>
        Task<ApiResponse> LogoutAsync(string? email);
        /// <summary>
        /// Retrieves detailed information for a specific user by their unique ID.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier of the user to retrieve.
        /// </param>
        /// <returns>
        /// An <see cref="ApplicationUserResponse"/> containing user details if found,
        /// or <c>null</c> if the user does not exist.
        /// </returns>
        Task<ApplicationUserResponse?> GetUserByIdAsync(string userId);
        /// <summary>
        /// Updates the profile information of a specific user, including display name, phone number, 
        /// and optionally password (if current password is provided).
        /// </summary>
        /// <param name="dto">
        /// The <see cref="UpdateUserDTO"/> object containing the user's ID, updated profile details, 
        /// and password change information.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating whether the update was successful, 
        /// along with any relevant messages or errors.
        /// </returns>
        Task<ApiResponse> UpdateUserProfileAsync(UpdateUserDTO dto);

        /// <summary>
        /// Handles the callback from an external login provider (e.g., Google, GitHub).
        /// </summary>
        /// <param name="remoteError">
        /// Optional error message returned by the external provider during the authentication process.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating whether the login was successful or failed,
        /// along with any relevant error messages or authentication tokens.
        /// </returns>
        /// <remarks>
        /// This method:
        /// <list type="number">
        /// <item>Validates if an error was returned by the external provider.</item>
        /// <item>Retrieves external login info from the authentication manager.</item>
        /// <item>Checks if the external login is already linked to an existing user.</item>
        /// <item>Attempts to create a new user if none exists with the provided external login information.</item>
        /// <item>Returns HTTP 400 if the provider does not supply enough information to create a user (e.g., no email or name).</item>
        /// </list>
        /// </remarks>
        Task<ApiResponse> ExternalLoginCallbackAsync(string remoteError = "");
        /// <summary>
        /// Deletes the account of the given user.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>An ApiResponse indicating the result of the operation.</returns>
        Task<ApiResponse> DeleteAccountAsync(string userId);
        /// <summary>
        /// Resends the account confirmation email to the specified user.
        /// </summary>
        /// <param name="email">The email of the user who requested the confirmation email.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating the outcome of the operation:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>200 OK</c> - if the confirmation email was sent successfully.</description>
        ///   </item>
        ///   <item>
        ///     <description><c>404 Not Found</c> - if the user does not exist.</description>
        ///   </item>
        ///   <item>
        ///     <description><c>409 Conflict</c> - if the user account is already confirmed.</description>
        ///   </item>
        ///   <item>
        ///     <description><c>500 Internal Server Error</c> - if sending the email fails unexpectedly.</description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<ApiResponse> ResendConfirmationEmailAsync(string email, string? apiKey = null);
        /// <summary>
        /// Uploads or replaces the authenticated user's profile photo.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier of the user whose photo is being uploaded.
        /// </param>
        /// <param name="file">
        /// The image file to be uploaded as the user's profile photo.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse"/> indicating the result of the upload operation.
        /// </returns>
        /// <response code="200">
        /// Photo uploaded successfully. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – No file provided or invalid file format. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – User is not authenticated. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        Task<ApiResponse> UploadUserPhotoAsync(Guid userId, UploadUserPhotoDto dto);
        /// <summary>
        /// Deletes the authenticated user's profile photo.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier of the user whose photo is being deleted.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse"/> indicating the result of the delete operation.
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
        Task<ApiResponse> DeleteUserPhotoAsync(Guid userId);

    }
}
