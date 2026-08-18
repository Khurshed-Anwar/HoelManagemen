using Microsoft.AspNetCore.Authorization;

namespace HotelManagement.Authorization.Attributes
{
    // Usage: [RequirePermission("Hotels.Create")]
    // Translates to policy name: "Permission:Hotels.Create"
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public string Permission { get; }

        public RequirePermissionAttribute(string permission) : base($"Permission:{permission}")
        {
            Permission = permission;
        }
    }
}
