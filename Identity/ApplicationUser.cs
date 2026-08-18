using Microsoft.AspNetCore.Identity;
using HotelManagement.Data;

namespace HotelManagement.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        // Navigation
        public IList<RefreshToken> RefreshTokens { get; set; } = [];
        public IList<UserPermission> UserPermissions { get; set; } = [];
    }
}
