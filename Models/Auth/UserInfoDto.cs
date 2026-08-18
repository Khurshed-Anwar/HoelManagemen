namespace HotelManagement.Models.Auth
{
    public class UserInfoDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public IList<string> Roles { get; set; } = [];
        public IList<string> Permissions { get; set; } = [];
    }
}
