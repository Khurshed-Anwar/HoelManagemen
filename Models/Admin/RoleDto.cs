namespace HotelManagement.Models.Admin
{
    public class RoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int UserCount { get; set; }
        public int PermissionCount { get; set; }
    }

    public class CreateRoleDto
    {
        public string Name { get; set; }
        public int? DepartmentId { get; set; }
    }

    public class AssignPermissionsDto
    {
        // List of permission IDs to assign to the role
        public IList<int> PermissionIds { get; set; } = [];
    }

    public class RolePermissionDto
    {
        public int Id { get; set; }
        public string Resource { get; set; }
        public string Action { get; set; }
        public string FullName { get; set; }
    }

    public class RoleUserDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
