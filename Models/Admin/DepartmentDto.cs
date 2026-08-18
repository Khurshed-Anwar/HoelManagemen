namespace HotelManagement.Models.Admin
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int RoleCount { get; set; }
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class DepartmentRoleDto
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }

        // "Admin" or "Reader"
        public string RoleType { get; set; }
        public int UserCount { get; set; }
        public int PermissionCount { get; set; }
    }
}
