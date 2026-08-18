using HotelManagement.Data;
using HotelManagement.Models.Auth;

namespace HotelManagement.Services.Auth
{
    public interface IAuthService
    {
        // Registers a new user with default Reader role
        // Returns error messages if registration fails
        Task<IEnumerable<string>> RegisterAsync(RegistrationDetails registration);

        // Validates credentials and returns tokens + user info on success
        // Returns null if credentials are invalid
        Task<LoginResponseDto?> LoginAsync(LoginDetails login);

        // Validates the refresh token, rotates it and returns a new LoginResponseDto
        // Returns null if refresh token is invalid, expired or revoked
        Task<LoginResponseDto?> RefreshTokenAsync(string userId, string refreshToken);

        // Revokes the refresh token — effectively logs the user out
        Task RevokeTokenAsync(string refreshToken);

        // Returns current user info including roles and permissions
        // Returns null if user not found
        Task<UserInfoDto?> GetCurrentUserAsync(string userId);
    }
}
