
using System.Security.Claims;
using CivicSense.Facade.Interface;
using Microsoft.AspNetCore.Authorization;

namespace CivicSense.Web.UI.Security
{
    public sealed class PermissionRequirement
        : IAuthorizationRequirement
    {
        public PermissionRequirement(string code)
        {
            Code = code;
        }

        public string Code { get; }
    }

    public sealed class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissions;

        public PermissionAuthorizationHandler(
            IPermissionService permissions)
        {
            _permissions = permissions;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
                return;

            if (!int.TryParse(
                context.User.FindFirstValue(
                    ClaimTypes.NameIdentifier),
                out int userId) || userId <= 0)
            {
                return;
            }

            if (await _permissions.HasPermissionAsync(
                userId, requirement.Code))
            {
                context.Succeed(requirement);
            }
        }
    }
}
