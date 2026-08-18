using HotelManagement.Data;
using HotelManagement.Identity;
using HotelManagement.Models.Auth;
using HotelManagement.Services.Authorization;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Services.Auth
{
    public class AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IPermissionService permissionService) : IAuthService
    {
        public async Task<IEnumerable<string>> RegisterAsync(RegistrationDetails registration)
        {
            var user = new ApplicationUser
            {
                UserName    = registration.Email,
                Email       = registration.Email,
                FirstName   = registration.FirstName,
                LastName    = registration.LastName,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, registration.Password);

            if (!result.Succeeded)
                return result.Errors.Select(e => e.Description);

            // All new users get Reader role by default
            await userManager.AddToRoleAsync(user, "Reader");

            return [];
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDetails login)
        {
            var user = await userManager.FindByEmailAsync(login.Email);
            if (user is null)
                return null;

            var passwordValid = await userManager.CheckPasswordAsync(user, login.Password);
            if (!passwordValid)
                return null;

            // Update last login timestamp
            user.LastLoginDate = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            return await BuildLoginResponseAsync(user);
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(string userId, string refreshToken)
        {
            var isValid = await tokenService.ValidateRefreshTokenAsync(userId, refreshToken);
            if (!isValid)
                return null;

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return null;

            // Rotate: revoke old token, issue new one
            var newRefreshToken = await tokenService.RotateRefreshTokenAsync(userId, refreshToken);

            var roles       = await userManager.GetRolesAsync(user);
            var permissions = await permissionService.GetUserPermissionsAsync(userId);
            var accessToken = tokenService.GenerateAccessToken(user, roles, permissions);

            return new LoginResponseDto
            {
                AccessToken  = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn    = 300,
                User         = BuildUserInfo(user, roles, permissions)
            };
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            await tokenService.RevokeRefreshTokenAsync(refreshToken);
        }

        public async Task<UserInfoDto?> GetCurrentUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return null;

            var roles       = await userManager.GetRolesAsync(user);
            var permissions = await permissionService.GetUserPermissionsAsync(userId);

            return BuildUserInfo(user, roles, permissions);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private async Task<LoginResponseDto> BuildLoginResponseAsync(ApplicationUser user)
        {
            var roles       = await userManager.GetRolesAsync(user);
            var permissions = await permissionService.GetUserPermissionsAsync(user.Id);
            var accessToken = tokenService.GenerateAccessToken(user, roles, permissions);
            var refreshToken = await tokenService.GenerateRefreshTokenAsync(user.Id);

            return new LoginResponseDto
            {
                AccessToken  = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn    = 300,
                User         = BuildUserInfo(user, roles, permissions)
            };
        }

        private static UserInfoDto BuildUserInfo(
            ApplicationUser user,
            IList<string> roles,
            IList<string> permissions) => new()
        {
            Id          = user.Id,
            Email       = user.Email!,
            FirstName   = user.FirstName,
            LastName    = user.LastName,
            Roles       = roles,
            Permissions = permissions
        };
    }
}
