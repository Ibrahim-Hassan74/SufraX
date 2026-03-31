using AutoMapper;
using EStoreX.Core.Domain.IdentityEntities;
using EStoreX.Core.DTO.Account.Requests;
using EStoreX.Core.DTO.Account.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Account;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;

namespace EStoreX.Core.Services.Account
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<UserManagementService> _localizer;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<UserManagementService> localizer)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _localizer = localizer;
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> AddAdminAsync(CreateAdminDTO dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                DisplayName = dto.DisplayName,
                EmailConfirmed = true,
                PhoneNumber = dto.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return ApiResponseFactory.Failure(_localizer["FailedToCreateAdmin"].Value, 400, result.Errors.Select(x => x.Description).ToArray());

            if (!await _roleManager.RoleExistsAsync(UserTypeOptions.Admin.ToString()))
            {
                await _roleManager.CreateAsync(new ApplicationRole() { Name = UserTypeOptions.Admin.ToString() });
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            return ApiResponseFactory.Success(_localizer["AdminCreatedSuccessfully"].Value);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> AssignRoleToUserAsync(UpdateUserRoleDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["UserNotFound"].Value);

            if (!await _roleManager.RoleExistsAsync(dto.Role.ToString()))
                await _roleManager.CreateAsync(new ApplicationRole() { Name = dto.Role.ToString() });

            var result = await _userManager.AddToRoleAsync(user, dto.Role.ToString());

            return result.Succeeded
                ? ApiResponseFactory.Success(_localizer["RoleAssignedSuccessfully"].Value)
                : ApiResponseFactory.Failure(_localizer["FailedToAssignRole"].Value, 400, result.Errors.Select(error => error.Description).ToArray());
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> RemoveRoleFromUserAsync(UpdateUserRoleDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["UserNotFound"].Value);


            var result = await _userManager.RemoveFromRoleAsync(user, dto.Role.ToString());

            return result.Succeeded
                ? ApiResponseFactory.Success(_localizer["RoleRemovedSuccessfully"].Value)
                : ApiResponseFactory.Failure(_localizer["FailedToRemoveRole"].Value, 400, result.Errors.Select(error => error.Description).ToArray());
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["UserNotFound"].Value);

            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
            var currentRoles = await _userManager.GetRolesAsync(currentUser!);

            var targetRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains("Admin"))
            {
                if (targetRoles.Contains("Admin") || targetRoles.Contains("SuperAdmin"))
                    return ApiResponseFactory.Failure(_localizer["AdminsCannotActivateAdmin"].Value, 403);
            }

            if (currentRoles.Contains("SuperAdmin"))
            {
                if (targetRoles.Contains("SuperAdmin"))
                    return ApiResponseFactory.Failure(_localizer["SuperAdminsCannotActivateSuperAdmin"].Value, 403);
            }

            user.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded
                ? ApiResponseFactory.Success(_localizer["UserActivatedSuccessfully"].Value)
                : ApiResponseFactory.Failure(_localizer["FailedToActivateUser"].Value, 400, result.Errors.Select(error => error.Description).ToArray());
        }


        /// <inheritdoc/>
        public async Task<ApiResponse> DeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["UserNotFound"].Value);

            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
            var currentRoles = await _userManager.GetRolesAsync(currentUser!);

            // Get target user roles
            var targetRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains("Admin"))
            {
                if (targetRoles.Contains("Admin") || targetRoles.Contains("SuperAdmin"))
                    return ApiResponseFactory.Failure(_localizer["AdminsCannotDeactivateAdmin"].Value, 403);
            }

            if (currentRoles.Contains("SuperAdmin"))
            {
                if (targetRoles.Contains("SuperAdmin"))
                    return ApiResponseFactory.Failure(_localizer["SuperAdminsCannotDeactivateSuperAdmin"].Value, 403);
            }

            user.LockoutEnd = DateTimeOffset.MaxValue;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded
                ? ApiResponseFactory.Success(_localizer["UserDeactivatedSuccessfully"].Value)
                : ApiResponseFactory.Failure(_localizer["FailedToDeactivateUser"].Value, 400, result.Errors.Select(error => error.Description).ToArray());
        }


        /// <inheritdoc/>
        public async Task<ApiResponse> DeleteUserAsync(string targetUserId, string currentUserId)
        {
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["UserNotFound"].Value);

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return ApiResponseFactory.Failure(_localizer["Unauthorized"].Value, 404);

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            var currentRoles = await _userManager.GetRolesAsync(currentUser);

            bool isCurrentSuperAdmin = currentRoles.Contains("SuperAdmin");
            bool isCurrentAdmin = currentRoles.Contains("Admin");

            bool isTargetAdmin = targetRoles.Contains("Admin");
            bool isTargetSuperAdmin = targetRoles.Contains("SuperAdmin");

            if (isTargetSuperAdmin && !isCurrentSuperAdmin)
                return ApiResponseFactory.Failure(_localizer["OnlySuperAdminCanDeleteSuperAdmin"].Value, 400);

            if (isTargetAdmin && !isCurrentSuperAdmin)
                return ApiResponseFactory.Failure(_localizer["OnlySuperAdminCanDeleteAdmin"].Value, 400);

            if (!isTargetAdmin && !isTargetSuperAdmin && !(isCurrentAdmin || isCurrentSuperAdmin))
                return ApiResponseFactory.Failure(_localizer["OnlyAdminCanDeleteUser"].Value, 400);

            var result = await _userManager.DeleteAsync(targetUser);

            return result.Succeeded
                ? ApiResponseFactory.Success(_localizer["UserDeletedSuccessfully"].Value)
                : ApiResponseFactory.Failure(_localizer["FailedToDeleteUser"].Value, 400, result.Errors.Select(e => e.Description).ToArray());
        }



        /// <inheritdoc/>
        public async Task<ApiResponse> DeleteAdminAsync(string adminUserId)
        {
            var user = await _userManager.FindByIdAsync(adminUserId);
            if (user == null)
                return ApiResponseFactory.Failure(_localizer["UserNotFound"].Value, 404, _localizer["UserNotFound"].Value);

            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
            var currentRoles = await _userManager.GetRolesAsync(currentUser!);

            var targetRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains("Admin"))
            {
                return ApiResponseFactory.Failure(_localizer["AdminsCannotDeleteAdmin"].Value, 403);
            }

            if (currentRoles.Contains("SuperAdmin"))
            {
                if (targetRoles.Contains("SuperAdmin"))
                    return ApiResponseFactory.Failure(_localizer["SuperAdminsCannotDeleteSuperAdmin"].Value, 403);
            }

            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded
                ? ApiResponseFactory.Success(_localizer["AdminDeletedSuccessfully"].Value)
                : ApiResponseFactory.Failure(_localizer["FailedToDeleteAdmin"].Value, 400, result.Errors.Select(e => e.Description).ToArray());
        }


        /// <inheritdoc/>
        public async Task<List<ApplicationUserResponse>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .Include(u => u.Photo) 
                .ToListAsync();

            var filteredUsers = new List<ApplicationUser>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(nameof(UserTypeOptions.User)))
                {
                    filteredUsers.Add(user);
                }
            }

            var responses = _mapper.Map<List<ApplicationUserResponse>>(filteredUsers);

            foreach (var response in responses)
            {
                var user = filteredUsers.First(u => u.Id.ToString() == response.Id);
                response.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            }

            return responses;
        }



        /// <inheritdoc/>
        public async Task<ApplicationUserResponse?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.Users.Include(u => u.Photo).FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (user == null) return null;

            var response = _mapper.Map<ApplicationUserResponse>(user);
            response.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return response;
        }
        /// <inheritdoc/>
        public async Task<List<ApplicationUserResponse>> GetAdminsAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync(nameof(UserTypeOptions.Admin));
            var superAdmins = await _userManager.GetUsersInRoleAsync(nameof(UserTypeOptions.SuperAdmin));
            var res = admins.Concat(superAdmins).Distinct().ToList();
            return _mapper.Map<List<ApplicationUserResponse>>(res);
        }
    }
}