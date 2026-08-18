using HotelManagement.Identity;

namespace HotelManagement.Data
{
    public class UserPermission
    {
        public string UserId { get; set; }
        public int PermissionId { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}
