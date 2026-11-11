
using System.Security.Claims;
using CivicSense.DataAccess.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CivicSense.Web.UI.Security
{
    public class CustomCookieEvents
        : CookieAuthenticationEvents
    {
        private readonly IUserRepository _users;

        public CustomCookieEvents(IUserRepository users)
        {
            _users = users;
        }

        public override async Task ValidatePrincipal(
            CookieValidatePrincipalContext context)
        {
            string? userIdValue =
                context.Principal?.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                await RejectAsync(context);
                return;
            }

            var user = await _users.GetByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                await RejectAsync(context);
                return;
            }

            string? cookieRole =
                context.Principal?.FindFirstValue(
                    ClaimTypes.Role);

            string? cookieEmail =
                context.Principal?.FindFirstValue(
                    ClaimTypes.Email);

            if (cookieRole != user.Role.ToString() ||
                !string.Equals(
                    cookieEmail,
                    user.Email,
                    StringComparison.Ordinal))
            {
                await RejectAsync(context);
            }
        }

        private static async Task RejectAsync(
            CookieValidatePrincipalContext context)
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
