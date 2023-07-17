using System.Security.Claims;

namespace Application.Helpers
{
    public static class ClaimsHelpers
    {
        // ClaimPrincipal
        public static Guid GetId(this ClaimsPrincipal principal)
        {
            try
            {
                return Guid.Parse(principal.FindFirstValue("id"));
            }
            catch
            {
                return Guid.Empty;
            }

        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Email);
        }

        public static string GetRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Role);
        }

        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Role) == "ADMIN";
        }
    }
}