namespace HotelManagement.Data
{
    public class Permission
    {
        public int Id { get; set; }

        // e.g. "Hotels", "Countries", "Bookings"
        public string Resource { get; set; }

        // e.g. "Read", "Create", "Update", "Delete"
        public string Action { get; set; }

        // e.g. "Hotels.Create"
        public string FullName => $"{Resource}.{Action}";

        public string? Description { get; set; }

        // Navigation
        public IList<RolePermission> RolePermissions { get; set; } = [];
        public IList<UserPermission> UserPermissions { get; set; } = [];
    }
}
