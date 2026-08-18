using HotelManagement.Data;
using HotelManagement.Helpers;
using HotelManagement.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            HotelListringDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            // Ensure DB is up to date
            await context.Database.MigrateAsync();

            await SeedDepartmentsAsync(context);
            await SeedRolesAsync(context, roleManager);
            await SeedPermissionsAsync(context);
            await SeedRolePermissionsAsync(context, roleManager);
            await SeedAdminUserAsync(userManager);
        }

        // ── Departments ───────────────────────────────────────────────────────────

        private static async Task SeedDepartmentsAsync(HotelListringDbContext context)
        {
            string[] names = ["Finance", "IT", "Sales"];

            foreach (var name in names)
            {
                if (!await context.Departments.AnyAsync(d => d.Name == name))
                {
                    context.Departments.Add(new Department
                    {
                        Name        = name,
                        Description = $"{name} Department",
                        IsActive    = true,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        // ── Roles ─────────────────────────────────────────────────────────────────

        private static async Task SeedRolesAsync(
            HotelListringDbContext context,
            RoleManager<ApplicationRole> roleManager)
        {
            // System roles — no department
            string[] systemRoles = ["Admin", "Reader"];

            foreach (var name in systemRoles)
            {
                if (!await roleManager.RoleExistsAsync(name))
                    await roleManager.CreateAsync(new ApplicationRole { Name = name });
            }

            // Department-scoped roles — Admin + Reader per department
            string[] deptNames = ["Finance", "IT", "Sales"];

            foreach (var dept in deptNames)
            {
                var department = await context.Departments
                    .FirstOrDefaultAsync(d => d.Name == dept);

                if (department is null)
                    continue;

                foreach (var suffix in new[] { "Admin", "Reader" })
                {
                    var roleName = $"{dept}{suffix}";
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new ApplicationRole
                        {
                            Name         = roleName,
                            DepartmentId = department.Id
                        });
                    }
                }
            }
        }

        // ── Permissions ───────────────────────────────────────────────────────────

        private static async Task SeedPermissionsAsync(HotelListringDbContext context)
        {
            string[] resources = ["Countries", "Hotels"];

            foreach (var resource in resources)
            {
                foreach (var action in new[]
                {
                    PermissionHelper.Actions.Read,
                    PermissionHelper.Actions.Create,
                    PermissionHelper.Actions.Update,
                    PermissionHelper.Actions.Delete
                })
                {
                    if (!await context.Permissions.AnyAsync(
                            p => p.Resource == resource && p.Action == action))
                    {
                        context.Permissions.Add(new Permission
                        {
                            Resource    = resource,
                            Action      = action,
                            Description = $"Allows {action.ToLower()} operations on {resource}"
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        // ── Role-Permission Mappings ───────────────────────────────────────────────

        private static async Task SeedRolePermissionsAsync(
            HotelListringDbContext context,
            RoleManager<ApplicationRole> roleManager)
        {
            // Load all permissions into a lookup dict: "Countries.Read" → id
            var permLookup = await context.Permissions
                .ToDictionaryAsync(p => p.Resource + "." + p.Action, p => p.Id);

            // Define which permissions each role gets
            // Key: role name, Value: list of permission full names
            var matrix = new Dictionary<string, IEnumerable<string>>
            {
                // Admin — all permissions
                ["Admin"] = [
                    "Countries.Read", "Countries.Create", "Countries.Update", "Countries.Delete",
                    "Hotels.Read",    "Hotels.Create",    "Hotels.Update",    "Hotels.Delete"
                ],

                // Reader — read-only on everything
                ["Reader"] = [
                    "Countries.Read",
                    "Hotels.Read"
                ],

                // Sales department
                ["SalesAdmin"] = [
                    "Countries.Read", "Countries.Create", "Countries.Update",
                    "Hotels.Read",    "Hotels.Create",    "Hotels.Update",    "Hotels.Delete"
                ],
                ["SalesReader"] = [
                    "Countries.Read",
                    "Hotels.Read"
                ],

                // IT department
                ["ITAdmin"] = [
                    "Countries.Read", "Countries.Create", "Countries.Update",
                    "Hotels.Read",    "Hotels.Create",    "Hotels.Update"
                ],
                ["ITReader"] = [
                    "Countries.Read",
                    "Hotels.Read"
                ],

                // Finance department
                ["FinanceAdmin"] = [
                    "Countries.Read", "Countries.Update", "Countries.Delete",
                    "Hotels.Read",    "Hotels.Create"
                ],
                ["FinanceReader"] = [
                    "Countries.Read",
                    "Hotels.Read"
                ]
            };

            foreach (var (roleName, permissions) in matrix)
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role is null)
                    continue;

                foreach (var perm in permissions)
                {
                    if (!permLookup.TryGetValue(perm, out var permId))
                        continue;

                    var alreadyAssigned = await context.RolePermissions
                        .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permId);

                    if (!alreadyAssigned)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId       = role.Id,
                            PermissionId = permId
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        // ── Admin User ────────────────────────────────────────────────────────────

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail    = "admin@hotel.com";
            const string adminPassword = "Abcd1234!";

            var existing = await userManager.FindByEmailAsync(adminEmail);
            if (existing is not null)
                return;

            var admin = new ApplicationUser
            {
                UserName    = adminEmail,
                Email       = adminEmail,
                FirstName   = "System",
                LastName    = "Admin",
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
