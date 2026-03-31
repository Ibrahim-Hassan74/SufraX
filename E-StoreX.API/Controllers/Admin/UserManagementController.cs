using Asp.Versioning;
using EStoreX.Core.DTO.Account.Requests;
using EStoreX.Core.DTO.Account.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Account;
using EStoreX.Core.ServiceContracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Admin
{
    /// <summary>
    /// user management controller for administrative operations
    /// </summary>
    [Route("api/v{version:apiVersion}/admin/user-management")]
    [ApiVersion(2.0)]
    public class UserManagementController : AdminControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IExportService _exportService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// User management controller constructor
        /// </summary>
        /// <param name="userManagementService">Service responsible for management</param>
        /// <param name="exportService">Service to manage files.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public UserManagementController(IUserManagementService userManagementService, IExportService exportService, IStringLocalizer<SharedResource> localizer)
        {
            _userManagementService = userManagementService;
            _exportService = exportService;
            _localizer = localizer;
        }
        /// <summary>
        /// Retrieves all users (Admin or SuperAdmin only).
        /// </summary>
        /// <returns>A list of all users.</returns>
        /// <response code="200">Returns the list of users.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ApplicationUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManagementService.GetAllUsersAsync();
            return Ok(users);
        }
        /// <summary>
        /// Retrieves a user by their ID (Admin or SuperAdmin only).
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>User details if found.</returns>
        /// <response code="200">Returns the user details.</response>
        /// <response code="404">If the user with the given ID was not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApplicationUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null) return NotFound(ApiResponseFactory.NotFound(_localizer["UserNotFound"].Value));
            return Ok(user);
        }
        /// <summary>
        /// Creates a new Admin (SuperAdmin only).
        /// </summary>
        /// <param name="dto">Admin creation request details.</param>
        /// <returns>Result of the creation operation.</returns>
        /// <response code="201">Admin created successfully.</response>
        /// <response code="400">If the request is invalid or creation failed.</response>
        [HttpPost("add-admin")]
        [Authorize(Roles = $"{nameof(UserTypeOptions.SuperAdmin)}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAdmin([FromBody] CreateAdminDTO dto)
        {
            var result = await _userManagementService.AddAdminAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Permanently deletes an admin (SuperAdmin only).
        /// </summary>
        /// <param name="id">The ID of the admin to delete.</param>
        /// <returns>Result of the deletion operation.</returns>
        /// <response code="200">Admin deleted successfully.</response>
        /// <response code="404">If the admin was not found.</response>
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = $"{nameof(UserTypeOptions.SuperAdmin)}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            var result = await _userManagementService.DeleteAdminAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Deletes a user (rules depend on roles: SuperAdmin/Admin).
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>Result of the deletion operation.</returns>
        /// <response code="200">User deleted successfully.</response>
        /// <response code="400">If deletion is not allowed or invalid.</response>
        /// <response code="401">If the current user is not authenticated.</response>
        /// <response code="404">If the user was not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(_localizer["UserNotLoggedIn"].Value);

            var result = await _userManagementService.DeleteUserAsync(id, currentUserId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Deactivates a user (Admin or SuperAdmin).
        /// </summary>
        /// <param name="id">The ID of the user to deactivate.</param>
        /// <returns>Result of the deactivation operation.</returns>
        /// <response code="200">User deactivated successfully.</response>
        /// <response code="404">If the user was not found.</response>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var result = await _userManagementService.DeactivateUserAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Activates a user (Admin or SuperAdmin).
        /// </summary>
        /// <param name="id">The ID of the user to activate.</param>
        /// <returns>Result of the activation operation.</returns>
        /// <response code="200">User activated successfully.</response>
        /// <response code="404">If the user was not found.</response>
        [HttpPatch("{id}/activate")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var result = await _userManagementService.ActivateUserAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Assigns a role to a user (SuperAdmin only).
        /// </summary>
        /// <param name="dto">Role assignment details.</param>
        /// <returns>Result of the assignment operation.</returns>
        /// <response code="200">Role assigned successfully.</response>
        /// <response code="400">If assignment failed.</response>
        /// <response code="404">If the user was not found.</response>
        [HttpPost("assign-role")]
        [Authorize(Roles = $"{nameof(UserTypeOptions.SuperAdmin)}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRole([FromBody] UpdateUserRoleDTO dto)
        {
            var result = await _userManagementService.AssignRoleToUserAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Removes a role from a user (SuperAdmin only).
        /// </summary>
        /// <param name="dto">Role removal details.</param>
        /// <returns>Result of the removal operation.</returns>
        /// <response code="200">Role removed successfully.</response>
        /// <response code="400">If removal failed.</response>
        /// <response code="404">If the user was not found.</response>
        [HttpPost("remove-role")]
        [Authorize(Roles = $"{nameof(UserTypeOptions.SuperAdmin)}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRole([FromBody] UpdateUserRoleDTO dto)
        {
            var result = await _userManagementService.RemoveRoleFromUserAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Exports all users into the specified file format.
        /// </summary>
        /// <param name="type">
        /// The type of export format.  
        /// Supported values are:  
        /// <list type="bullet">
        ///   <item><description><see cref="ExportType.Csv"/> → Comma Separated Values file (.csv)</description></item>
        ///   <item><description><see cref="ExportType.Excel"/> → Microsoft Excel file (.xlsx)</description></item>
        ///   <item><description><see cref="ExportType.Pdf"/> → Portable Document Format file (.pdf)</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// A downloadable file in the selected export format.  
        /// Returns <see cref="BadRequestResult"/> if the format is not supported.
        /// </returns>
        /// <response code="200">users exported successfully in the requested format.</response>
        /// <response code="400">Unsupported export type requested.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("export/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> Export(ExportType type)
        {
            var users = await _userManagementService.GetAllUsersAsync();

            return type switch
            {
                ExportType.Csv => File(_exportService.ExportToCsv(users), "text/csv", "users.csv"),
                ExportType.Excel => File(_exportService.ExportToExcel(users), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx"),
                ExportType.Pdf => File(_exportService.ExportToPdf(users), "application/pdf", "users.pdf"),
                _ => BadRequest(ApiResponseFactory.BadRequest(_localizer["UnsupportedExportType"].Value))
            };
        }

        /// <summary>
        /// Retrieves all users who are either Admins or SuperAdmins.
        /// </summary>
        /// <returns>A list of Admin and SuperAdmin users.</returns>
        /// <response code="200">Returns the list of users.</response>
        /// <response code="404">If no Admin or SuperAdmin users were found.</response>
        [HttpGet("admins")]
        [ProducesResponseType(typeof(List<ApplicationUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAdmins()
        {
            var admins = await _userManagementService.GetAdminsAsync();
            if (admins == null || !admins.Any())
                return NotFound(ApiResponseFactory.NotFound(_localizer["UserNotFound"].Value));
            return Ok(admins);
        }


    }
}
