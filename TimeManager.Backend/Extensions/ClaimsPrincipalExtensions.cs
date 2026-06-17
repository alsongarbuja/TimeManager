using System.Security.Claims;

namespace TimeManager.Backend.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
        {
            return roles.Any(user.IsInRole);
        }
    }
}
