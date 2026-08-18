using HotelManagement.Identity;
using HotelManagement.Models.Admin;
using HotelManagement.Services.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services.Admin
{
    public class UserManagementService(
        UserManager<ApplicationUser> userManager,
        IPermissionService permissionService) : IUserManagementService
    {
        public async Task<IList<UserDto>> GetAllUsersAsync()
        {
            var users = await userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                result.Add(MapToDto(user, [.. roles]));
            }

            return result;
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return null;

            var roles = await userManager.GetRolesAsync(user);
            var allPermissions = await permissionService.GetUserPermissionsAsync(userId);
            var directPermissions = await permissionService.GetDirectUserPermissionsAsync(userId);

            return new UserDetailDto
            {
                Id              = user.Id,
                Email           = user.Email,
                FirstName       = user.FirstName,
                LastName        = user.LastName,
                CreatedDate     = user.CreatedDate,
                LastLoginDate   = user.LastLoginDate,
                IsLockedOut     = await userManager.IsLockedOutAsync(user),
                Roles           = [.. roles],
                Permissions     = allPermissions,
                DirectPermissions = directPermissions
            };
        }

        public async Task<UserDto?> UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return null;

            if (dto.FirstName is not null)
                user.FirstName = dto.FirstName;

            if (dto.LastName is not null)
                user.LastName = dto.LastName;

            if (dto.Email is not null && dto.Email != user.Email)
            {
                user.Email    = dto.Email;
                user.UserName = dto.Email;
            }

            await userManager.UpdateAsync(user);

            var roles = await userManager.GetRolesAsync(user);
            return MapToDto(user, [.. roles]);
        }

        public async Task AssignRolesAsync(string userId, AssignRolesDto dto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return;

            var currentRoles = await userManager.GetRolesAsync(user);

            var toRemove = currentRoles.Except(dto.RoleNames).ToList();
            var toAdd    = dto.RoleNames.Except(currentRoles).ToList();

            if (toRemove.Count > 0)
                await userManager.RemoveFromRolesAsync(user, toRemove);

            if (toAdd.Count > 0)
                await userManager.AddToRolesAsync(user, toAdd);
        }

        public async Task<bool> LockUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return false;

            var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return result.Succeeded;
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return false;

            var result = await userManager.SetLockoutEndDateAsync(user, null);
            return result.Succeeded;
        }

        public async Task<bool> ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return false;

            var token  = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return false;

            var result = await userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        private static UserDto MapToDto(ApplicationUser user, IList<string> roles) =>
            new()
            {
                Id            = user.Id,
                Email         = user.Email,
                FirstName     = user.FirstName,
                LastName      = user.LastName,
                CreatedDate   = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                Roles         = roles
            };
    }
}
