using HotelManagement.Data;
using HotelManagement.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services.Authorization
{
    public class PermissionService(
        HotelListringDbContext context,
        UserManager<ApplicationUser> userManager) : IPermissionService
    {
        // ── User Permission Queries ───────────────────────────────────────────────

        public async Task<IList<string>> GetUserPermissionsAsync(string userId)
        {
            // Single query — joins UserRoles → RolePermissions → Permissions
            // then UNION with direct UserPermissions → Permissions
            var rolePermissions =
                from userRole   in context.UserRoles
                join rp         in context.RolePermissions on userRole.RoleId equals rp.RoleId
                join permission in context.Permissions     on rp.PermissionId  equals permission.Id
                where userRole.UserId == userId
                select permission.Resource + "." + permission.Action;

            var directPermissions =
                from up         in context.UserPermissions
                join permission in context.Permissions on up.PermissionId equals permission.Id
                where up.UserId == userId
                select permission.Resource + "." + permission.Action;

            return await rolePermissions
                .Union(directPermissions)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> UserHasPermissionAsync(string userId, string permission)
        {
            var parts = permission.Split('.', 2);
            if (parts.Length != 2)
                return false;

            var resource = parts[0];
            var action   = parts[1];

            // Single query — checks both role and direct permissions in one round trip
            var hasRolePermission =
                from userRole   in context.UserRoles
                join rp         in context.RolePermissions on userRole.RoleId equals rp.RoleId
                join p          in context.Permissions     on rp.PermissionId  equals p.Id
                where userRole.UserId == userId
                   && p.Resource      == resource
                   && p.Action        == action
                select 1;

            var hasDirectPermission =
                from up in context.UserPermissions
                join p  in context.Permissions on up.PermissionId equals p.Id
                where up.UserId   == userId
                   && p.Resource  == resource
                   && p.Action    == action
                select 1;

            return await hasRolePermission.Union(hasDirectPermission).AnyAsync();
        }

        // ── Role Permission Management ────────────────────────────────────────────

        public async Task<IList<string>> GetRolePermissionsAsync(string roleId)
        {
            return await (
                from rp in context.RolePermissions
                join p  in context.Permissions on rp.PermissionId equals p.Id
                where rp.RoleId == roleId
                orderby p.Resource, p.Action
                select p.Resource + "." + p.Action
            ).ToListAsync();
        }

        public async Task AssignPermissionsToRoleAsync(string roleId, IEnumerable<int> permissionIds)
        {
            var ids = permissionIds.ToList();

            // Get already assigned IDs in one query
            var existing = await context.RolePermissions
                .Where(rp => rp.RoleId == roleId && ids.Contains(rp.PermissionId))
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var toAdd = ids
                .Except(existing)
                .Select(id => new RolePermission { RoleId = roleId, PermissionId = id });

            context.RolePermissions.AddRange(toAdd);
            await context.SaveChangesAsync();
        }

        public async Task RemovePermissionFromRoleAsync(string roleId, int permissionId)
        {
            var entry = await context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (entry is not null)
            {
                context.RolePermissions.Remove(entry);
                await context.SaveChangesAsync();
            }
        }

        // ── Direct User Permission Management ────────────────────────────────────

        public async Task AssignPermissionToUserAsync(string userId, int permissionId)
        {
            var exists = await context.UserPermissions
                .AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (!exists)
            {
                context.UserPermissions.Add(new UserPermission
                {
                    UserId       = userId,
                    PermissionId = permissionId
                });
                await context.SaveChangesAsync();
            }
        }

        public async Task RemovePermissionFromUserAsync(string userId, int permissionId)
        {
            var entry = await context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (entry is not null)
            {
                context.UserPermissions.Remove(entry);
                await context.SaveChangesAsync();
            }
        }

        // ── Permission Records ────────────────────────────────────────────────────

        public async Task<int> GetOrCreatePermissionAsync(string resource, string action)
        {
            var permission = await context.Permissions
                .FirstOrDefaultAsync(p => p.Resource == resource && p.Action == action);

            if (permission is not null)
                return permission.Id;

            var newPermission = new Permission
            {
                Resource    = resource,
                Action      = action,
                Description = $"Allows {action.ToLower()} operations on {resource}"
            };

            context.Permissions.Add(newPermission);
            await context.SaveChangesAsync();

            return newPermission.Id;
        }

        public async Task<IList<string>> GetAllPermissionsAsync()
        {
            return await context.Permissions
                .OrderBy(p => p.Resource)
                .ThenBy(p => p.Action)
                .Select(p => p.Resource + "." + p.Action)
                .ToListAsync();
        }

        public async Task<IList<string>> GetDirectUserPermissionsAsync(string userId)
        {
            return await (
                from up in context.UserPermissions
                join p  in context.Permissions on up.PermissionId equals p.Id
                where up.UserId == userId
                orderby p.Resource, p.Action
                select p.Resource + "." + p.Action
            ).ToListAsync();
        }
    }
}
