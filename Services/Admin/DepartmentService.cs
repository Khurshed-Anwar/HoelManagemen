using HotelManagement.Data;
using HotelManagement.Identity;
using HotelManagement.Models.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services.Admin
{
    public class DepartmentService(
        HotelListringDbContext context,
        RoleManager<ApplicationRole> roleManager) : IDepartmentService
    {
        public async Task<IList<DepartmentDto>> GetAllAsync()
        {
            return await (
                from d in context.Departments
                select new DepartmentDto
                {
                    Id          = d.Id,
                    Name        = d.Name,
                    Description = d.Description,
                    IsActive    = d.IsActive,
                    CreatedDate = d.CreatedDate,
                    RoleCount   = context.Roles.Count(r => r.DepartmentId == d.Id)
                }
            ).ToListAsync();
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            return await (
                from d in context.Departments
                where d.Id == id
                select new DepartmentDto
                {
                    Id          = d.Id,
                    Name        = d.Name,
                    Description = d.Description,
                    IsActive    = d.IsActive,
                    CreatedDate = d.CreatedDate,
                    RoleCount   = context.Roles.Count(r => r.DepartmentId == d.Id)
                }
            ).FirstOrDefaultAsync();
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
        {
            var department = new Department
            {
                Name        = dto.Name,
                Description = dto.Description,
                IsActive    = true,
                CreatedDate = DateTime.UtcNow
            };

            context.Departments.Add(department);
            await context.SaveChangesAsync();

            // Auto-create Admin and Reader roles for this department
            await CreateDepartmentRolesAsync(department);

            return new DepartmentDto
            {
                Id          = department.Id,
                Name        = department.Name,
                Description = department.Description,
                IsActive    = department.IsActive,
                CreatedDate = department.CreatedDate,
                RoleCount   = 2
            };
        }

        public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto)
        {
            var department = await context.Departments.FindAsync(id);
            if (department is null)
                return null;

            department.Name        = dto.Name;
            department.Description = dto.Description;
            department.IsActive    = dto.IsActive;

            await context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id          = department.Id,
                Name        = department.Name,
                Description = department.Description,
                IsActive    = department.IsActive,
                CreatedDate = department.CreatedDate,
                RoleCount   = await context.Roles.CountAsync(r => r.DepartmentId == id)
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await context.Departments.FindAsync(id);
            if (department is null)
                return false;

            // Soft delete
            department.IsDeleted  = true;
            department.DeletedAt  = DateTime.UtcNow;
            department.IsActive   = false;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<DepartmentRoleDto>> GetRolesAsync(int departmentId)
        {
            return await (
                from role in context.Roles
                where role.DepartmentId == departmentId
                select new DepartmentRoleDto
                {
                    RoleId          = role.Id,
                    RoleName        = role.Name!,
                    RoleType        = role.Name!.EndsWith("Admin") ? "Admin" : "Reader",
                    UserCount       = context.UserRoles.Count(ur => ur.RoleId == role.Id),
                    PermissionCount = context.RolePermissions.Count(rp => rp.RoleId == role.Id)
                }
            ).ToListAsync();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        // Creates {DeptName}Admin and {DeptName}Reader roles and links them to the department
        private async Task CreateDepartmentRolesAsync(Department department)
        {
            var adminRoleName  = $"{department.Name}Admin";
            var readerRoleName = $"{department.Name}Reader";

            foreach (var roleName in new[] { adminRoleName, readerRoleName })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole
                    {
                        Name         = roleName,
                        DepartmentId = department.Id
                    };
                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}
