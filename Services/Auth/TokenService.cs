using HotelManagement.Data;
using HotelManagement.Data.AppSettings;
using HotelManagement.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HotelManagement.Services.Auth
{
    public class TokenService(IOptions<JwtSettings> jwtOptions, HotelListringDbContext context) : ITokenService
    {
        private readonly JwtSettings _jwt = jwtOptions.Value;

        // ── Access Token ──────────────────────────────────────────────────────────

        public string GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<string> permissions)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub,   user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new(ClaimTypes.Name,               user.UserName!)
            };

            // Add each role as a separate claim
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // Add each permission as a separate claim
            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:             _jwt.Issuer,
                audience:           _jwt.Audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = false, // ← ignore expiry intentionally
                ValidateIssuerSigningKey = true,
                ValidIssuer              = _jwt.Issuer,
                ValidAudience            = _jwt.Audience,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey))
            };

            try
            {
                var principal = new JwtSecurityTokenHandler()
                    .ValidateToken(accessToken, parameters, out var validatedToken);

                // Ensure the token used HS256 signing
                if (validatedToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

        // ── Refresh Token ─────────────────────────────────────────────────────────

        public async Task<string> GenerateRefreshTokenAsync(string userId)
        {
            // Generate cryptographically secure raw token
            var rawToken  = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenHash = HashToken(rawToken);

            var refreshToken = new RefreshToken
            {
                TokenHash   = tokenHash,
                UserId      = userId,
                ExpiryDate  = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays),
                IsRevoked   = false,
                CreatedDate = DateTime.UtcNow
            };

            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            // Return the raw token — the hash stays in DB
            return rawToken;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);

            var stored = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == userId);

            if (stored is null)
                return false;

            // ── Reuse Detection ──────────────────────────────────────────────────
            // If the token was found but already revoked → it's being reused
            // This means a token was potentially stolen → revoke ALL user tokens immediately
            if (stored.IsRevoked)
            {
                await RevokeAllUserRefreshTokensAsync(userId);
                return false;
            }

            if (stored.ExpiryDate < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task<string> RotateRefreshTokenAsync(string userId, string oldRefreshToken)
        {
            // Revoke the old token
            await RevokeRefreshTokenAsync(oldRefreshToken);

            // Issue a brand new refresh token
            return await GenerateRefreshTokenAsync(userId);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);

            var stored = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (stored is not null && !stored.IsRevoked)
            {
                stored.IsRevoked = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(string userId)
        {
            var tokens = await context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
                token.IsRevoked = true;

            await context.SaveChangesAsync();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        // SHA256 hash of raw token — never store plain refresh tokens in DB
        private static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToBase64String(bytes);
        }
    }
}
