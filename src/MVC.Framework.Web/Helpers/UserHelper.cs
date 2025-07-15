using System.Linq;
using System.Security.Claims;
using System.Web;

namespace MVC.Framework.Web.Helpers
{
    public static class UserHelper
    {
        public static ClaimsPrincipal CurrentUser => HttpContext.Current?.User as ClaimsPrincipal;

        public static bool IsAuthenticated => CurrentUser?.Identity?.IsAuthenticated ?? false;

        public static string UserName => IsAuthenticated ? CurrentUser.Identity.Name : "Unknown";

        public static string AuthenticationType => CurrentUser?.Identity?.AuthenticationType ?? "None";

        public static string GetClaim(string claimType)
        {
            return CurrentUser?.Claims?.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
    }
}

