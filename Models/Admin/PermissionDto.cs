namespace HotelManagement.Models.Admin
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Resource { get; set; }
        public string Action { get; set; }
        public string FullName { get; set; }
        public string? Description { get; set; }
        public int AssignedRoleCount { get; set; }
    }

    // Permissions grouped by resource for UI display
    public class ResourcePermissionsDto
    {
        public string Resource { get; set; }
        public IList<PermissionDto> Permissions { get; set; } = [];
    }

    public class GeneratePermissionsDto
    {
        public string ResourceName { get; set; }
    }
}
