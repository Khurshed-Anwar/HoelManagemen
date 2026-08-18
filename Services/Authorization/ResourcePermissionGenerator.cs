using HotelManagement.Data;
using HotelManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services.Authorization
{
    public class ResourcePermissionGenerator(HotelListringDbContext context) : IResourcePermissionGenerator
    {
        public async Task<IList<string>> GenerateAsync(string resourceName)
        {
            var resource = resourceName.Trim();

            // Get actions that already exist for this resource in one query
            var existing = await context.Permissions
                .Where(p => p.Resource == resource)
                .Select(p => p.Action)
                .ToListAsync();

            // Only create the actions that are missing
            var missing = new[] 
            {
                PermissionHelper.Actions.Read,
                PermissionHelper.Actions.Create,
                PermissionHelper.Actions.Update,
                PermissionHelper.Actions.Delete
            }
            .Except(existing)
            .ToList();

            if (missing.Count == 0)
                return [];

            var newPermissions = missing.Select(action => new Permission
            {
                Resource    = resource,
                Action      = action,
                Description = $"Allows {action.ToLower()} operations on {resource}"
            }).ToList();

            context.Permissions.AddRange(newPermissions);
            await context.SaveChangesAsync();

            return newPermissions
                .Select(p => PermissionHelper.Format(p.Resource, p.Action))
                .ToList();
        }

        public async Task<IList<string>> GetAllResourcesAsync()
        {
            return await context.Permissions
                .Select(p => p.Resource)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        public async Task EnsureExistsAsync(string resourceName)
        {
            // Silent version of GenerateAsync — used during DB seeding
            await GenerateAsync(resourceName);
        }
    }
}
