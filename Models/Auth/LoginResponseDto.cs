namespace HotelManagement.Models.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        // Seconds until access token expires (300 = 5 minutes)
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserInfoDto User { get; set; }
    }
}
