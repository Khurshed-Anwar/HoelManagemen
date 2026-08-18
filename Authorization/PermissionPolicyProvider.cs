using HotelManagement.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HotelManagement.Authorization
{
    // Dynamically creates authorization policies for any "Permission:{name}" policy name
    // This means we NEVER need to register individual policies in Program.cs
    // Adding [RequirePermission("Bookings.Create")] just works automatically
    //
    // IMPORTANT: We inject DefaultAuthorizationPolicyProvider directly (not IAuthorizationPolicyProvider)
    // to avoid a circular dependency: IAuthorizationPolicyProvider → PermissionPolicyProvider → IAuthorizationPolicyProvider
    public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : IAuthorizationPolicyProvider
    {
        private const string PermissionPrefix = "Permission:";

        // Instantiate once — safe because AuthorizationOptions is Singleton
        private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName[PermissionPrefix.Length..];

                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            // Fall back to default provider for standard [Authorize(Roles = "Admin")] etc.
            return _fallback.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => _fallback.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => _fallback.GetFallbackPolicyAsync();
    }
}
