using HotelManagement.Identity;

namespace HotelManagement.Data
{
    public class RefreshToken
    {
        public int Id { get; set; }

        // SHA256 hash of the raw token — raw token is only ever held by the client
        public string TokenHash { get; set; }

        public string UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }
}
