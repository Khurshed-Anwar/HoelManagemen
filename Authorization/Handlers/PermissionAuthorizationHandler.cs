using HotelManagement.Authorization.Requirements;
using HotelManagement.Services.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HotelManagement.Authorization.Handlers
{
    public class PermissionAuthorizationHandler(IPermissionService permissionService)
        : AuthorizationHandler<PermissionRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // User must be authenticated
            if (string.IsNullOrEmpty(userId))
            {
                context.Fail();
                return;
            }

            // Check if the user has the required permission
            var hasPermission = await permissionService.UserHasPermissionAsync(userId, requirement.Permission);

            if (hasPermission)
                context.Succeed(requirement);
            else
                context.Fail();
        }
    }
}
