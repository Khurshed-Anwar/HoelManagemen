using HotelManagement.Models.Admin;

namespace HotelManagement.Services.Admin
{
    public interface IRoleManagementService
    {
        Task<IList<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(string roleId);

        // Creates a custom role — optionally linked to a department
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);

        // Deletes a role — Admin and Reader system roles are protected
        Task<bool> DeleteRoleAsync(string roleId);

        // Returns all permissions assigned to a role
        Task<IList<RolePermissionDto>> GetRolePermissionsAsync(string roleId);

        // Bulk assigns permissions to a role (skips already assigned ones)
        Task AssignPermissionsAsync(string roleId, AssignPermissionsDto dto);

        // Removes a single permission from a role
        Task<bool> RemovePermissionAsync(string roleId, int permissionId);

        // Returns all users assigned to a role
        Task<IList<RoleUserDto>> GetUsersInRoleAsync(string roleId);
    }
}
