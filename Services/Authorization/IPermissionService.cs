namespace HotelManagement.Services.Authorization
{
    public interface IPermissionService
    {
        // ── User Permission Queries ───────────────────────────────────────────────

        // Returns all permission names for a user (role permissions + direct permissions)
        // e.g. ["Hotels.Create", "Hotels.Update", "Countries.Read"]
        Task<IList<string>> GetUserPermissionsAsync(string userId);

        // Checks if a user has a specific permission (from any source)
        Task<bool> UserHasPermissionAsync(string userId, string permission);

        // ── Role Permission Management ────────────────────────────────────────────

        // Returns all permission names assigned to a role
        Task<IList<string>> GetRolePermissionsAsync(string roleId);

        // Assigns a list of permissions to a role (bulk assign)
        Task AssignPermissionsToRoleAsync(string roleId, IEnumerable<int> permissionIds);

        // Removes a single permission from a role
        Task RemovePermissionFromRoleAsync(string roleId, int permissionId);

        // ── Direct User Permission Management ────────────────────────────────────

        // Assigns a permission directly to a user (overrides role permissions)
        Task AssignPermissionToUserAsync(string userId, int permissionId);

        // Removes a direct permission from a user
        Task RemovePermissionFromUserAsync(string userId, int permissionId);

        // ── Permission Records ────────────────────────────────────────────────────

        // Finds an existing permission or creates it if it doesn't exist
        Task<int> GetOrCreatePermissionAsync(string resource, string action);

        // Returns all permissions grouped by resource
        Task<IList<string>> GetAllPermissionsAsync();

        // Returns only the permissions assigned directly to a user (not via roles)
        Task<IList<string>> GetDirectUserPermissionsAsync(string userId);
    }
}
