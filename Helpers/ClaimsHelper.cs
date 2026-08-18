using System.Security.Claims;

namespace HotelManagement.Helpers
{
    public static class ClaimsHelper
    {
        // Extracts userId from JWT sub claim
        public static string? GetUserId(ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier);

        // Extracts email from JWT email claim
        public static string? GetEmail(ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Email);

        // Extracts all role names from JWT role claims
        public static IList<string> GetRoles(ClaimsPrincipal user)
            => user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // Extracts all permission names from JWT permission claims
        public static IList<string> GetPermissions(ClaimsPrincipal user)
            => user.FindAll("permission").Select(c => c.Value).ToList();

        // Checks if the JWT contains a specific permission claim
        public static bool HasPermission(ClaimsPrincipal user, string permission)
            => user.FindAll("permission").Any(c => c.Value == permission);
    }
}
