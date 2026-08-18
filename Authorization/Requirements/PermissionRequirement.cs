using Microsoft.AspNetCore.Authorization;

namespace HotelManagement.Authorization.Requirements
{
    // Data carrier — holds the permission name that needs to be checked
    // e.g. "Hotels.Create", "Countries.Delete"
    public class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}
