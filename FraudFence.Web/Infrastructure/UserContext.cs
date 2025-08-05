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

        public int Id
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

                var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? throw new InvalidOperationException("User ID claim missing.");

                if (!int.TryParse(idClaim, out var id))
                    throw new InvalidOperationException($"Invalid user ID format: '{idClaim}'.");

                return id;
            }
        }
    }
}
