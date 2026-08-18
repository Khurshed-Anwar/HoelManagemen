using HotelManagement.Identity;
using System.Security.Claims;

namespace HotelManagement.Services.Auth
{
    public interface ITokenService
    {
        // Generates a signed JWT access token containing user claims, roles and permissions
        string GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<string> permissions);

        // Generates a cryptographically secure random refresh token, hashes it, stores hash in DB
        // Returns the raw token to send to the client
        Task<string> GenerateRefreshTokenAsync(string userId);

        // Validates a refresh token:
        // - Hashes incoming token and looks up in DB
        // - Checks IsRevoked = false and ExpiryDate > now
        // - If token is found but already revoked → REUSE DETECTED → revokes ALL user tokens
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);

        // Token Rotation: revokes old refresh token and generates a new one atomically
        // Called on every successful refresh request
        Task<string> RotateRefreshTokenAsync(string userId, string oldRefreshToken);

        // Revokes a single refresh token by raw value (used on logout)
        Task RevokeRefreshTokenAsync(string refreshToken);

        // Revokes ALL refresh tokens for a user (used on reuse detection or security breach)
        Task RevokeAllUserRefreshTokensAsync(string userId);

        // Extracts ClaimsPrincipal from an expired access token (used during token refresh flow)
        // Validates signature only — ignores expiry
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
    }
}
