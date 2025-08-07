using FraudFence.EntityModels.Constants;
using FraudFence.Interface.Common;
using System.Security.Claims;

namespace FraudFence.Web.Infrastructure
{
    internal class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Id
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // If no HTTP context (e.g., during seeding), return system user ID
                if (httpContext == null)
                    return SystemConstants.SystemUserId;

                // If the user is not authenticated, treat as guest
                if (!httpContext.User.Identity?.IsAuthenticated ?? true)
                    return SystemConstants.GuestUserId;

                // In Cognito JWTs, the user's unique ID is in the 'sub' claim, which maps to NameIdentifier.
                var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return idClaim ?? throw new InvalidOperationException("User ID claim (sub) missing.");
            }
        }
    }
}
