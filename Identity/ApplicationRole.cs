using HotelManagement.Data;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Identity
{
    public class ApplicationRole : IdentityRole
    {
        // Links role to a department (null = system role like Admin/Reader)
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        // Navigation
        public IList<RolePermission> RolePermissions { get; set; } = [];
    }
}
