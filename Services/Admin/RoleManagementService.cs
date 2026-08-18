using HotelManagement.Data;
using HotelManagement.Identity;
using HotelManagement.Models.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services.Admin
{
    public class RoleManagementService(
        HotelListringDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager) : IRoleManagementService
    {
        // System roles that cannot be deleted
        private static readonly string[] ProtectedRoles = ["Admin", "Reader"];

        public async Task<IList<RoleDto>> GetAllRolesAsync()
        {
            return await (
                from role in context.Roles
                join dept in context.Departments
                    on role.DepartmentId equals dept.Id into deptJoin
                from dept in deptJoin.DefaultIfEmpty()
                select new RoleDto
                {
                    Id             = role.Id,
                    Name           = role.Name!,
                    DepartmentId   = role.DepartmentId,
                    DepartmentName = dept != null ? dept.Name : null,
                    UserCount      = context.UserRoles.Count(ur => ur.RoleId == role.Id),
                    PermissionCount= context.RolePermissions.Count(rp => rp.RoleId == role.Id)
                }
            ).ToListAsync();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string roleId)
        {
            return await (
                from role in context.Roles
                where role.Id == roleId
                join dept in context.Departments
                    on role.DepartmentId equals dept.Id into deptJoin
                from dept in deptJoin.DefaultIfEmpty()
                select new RoleDto
                {
                    Id             = role.Id,
                    Name           = role.Name!,
                    DepartmentId   = role.DepartmentId,
                    DepartmentName = dept != null ? dept.Name : null,
                    UserCount      = context.UserRoles.Count(ur => ur.RoleId == role.Id),
                    PermissionCount= context.RolePermissions.Count(rp => rp.RoleId == role.Id)
                }
            ).FirstOrDefaultAsync();
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            var role = new ApplicationRole
            {
                Name         = dto.Name,
                DepartmentId = dto.DepartmentId
            };

            await roleManager.CreateAsync(role);

            return new RoleDto
            {
                Id             = role.Id,
                Name           = role.Name!,
                DepartmentId   = role.DepartmentId,
                UserCount      = 0,
                PermissionCount= 0
            };
        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role is null)
                return false;

            // Protect system roles from deletion
            if (ProtectedRoles.Contains(role.Name))
                return false;

            await roleManager.DeleteAsync(role);
            return true;
        }

        public async Task<IList<RolePermissionDto>> GetRolePermissionsAsync(string roleId)
        {
            return await (
                from rp in context.RolePermissions
                join p  in context.Permissions on rp.PermissionId equals p.Id
                where rp.RoleId == roleId
                orderby p.Resource, p.Action
                select new RolePermissionDto
                {
                    Id       = p.Id,
                    Resource = p.Resource,
                    Action   = p.Action,
                    FullName = p.Resource + "." + p.Action
                }
            ).ToListAsync();
        }

        public async Task AssignPermissionsAsync(string roleId, AssignPermissionsDto dto)
        {
            // Get already assigned IDs to skip duplicates
            var existing = await context.RolePermissions
                .Where(rp => rp.RoleId == roleId && dto.PermissionIds.Contains(rp.PermissionId))
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var toAdd = dto.PermissionIds
                .Except(existing)
                .Select(id => new RolePermission { RoleId = roleId, PermissionId = id });

            context.RolePermissions.AddRange(toAdd);
            await context.SaveChangesAsync();
        }

        public async Task<bool> RemovePermissionAsync(string roleId, int permissionId)
        {
            var entry = await context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (entry is null)
                return false;

            context.RolePermissions.Remove(entry);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<RoleUserDto>> GetUsersInRoleAsync(string roleId)
        {
            return await (
                from ur   in context.UserRoles
                join user in context.Users on ur.UserId equals user.Id
                where ur.RoleId == roleId
                select new RoleUserDto
                {
                    UserId    = user.Id,
                    Email     = user.Email!,
                    FirstName = user.FirstName,
                    LastName  = user.LastName
                }
            ).ToListAsync();
        }
    }
}
