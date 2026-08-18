using HotelManagement.Identity;

namespace HotelManagement.Data
{
    public class RolePermission
    {
        public string RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation
        public ApplicationRole Role { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}
