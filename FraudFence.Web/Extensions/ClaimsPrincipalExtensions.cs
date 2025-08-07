using System.Security.Claims;

namespace FraudFence.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            return user.FindFirst("FullName")?.Value ?? user.Identity?.Name ?? "Unknown";
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst("email")?.Value ?? string.Empty;
        }
    }
}
