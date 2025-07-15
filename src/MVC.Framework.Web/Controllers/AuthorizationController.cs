using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using MVC.Framework.Web.Helpers;

namespace MVC.Framework.Web.Controllers
{
    public class AuthorizationController : Controller
    {
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;


        [AllowAnonymous]
        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                // This will trigger a redirect to the external identity provider.
                // The RedirectUri specifies where to go *after* a successful login.
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties {RedirectUri = "/"},
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void SignOut()
        {

            var authResult = HttpContext.GetOwinContext().Authentication.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationType).Result;
            var authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();

            HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
        }

        // This action is no longer needed with the corrected SignOut method.
        // public ActionResult SignOutCallback() { ... }

        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }

        public ActionResult Callback()
        {
            var result = HttpContext.GetOwinContext().Authentication.AuthenticateAsync(OpenIdConnectAuthenticationDefaults.AuthenticationType).Result;

            var accessToken = HttpContext.Request.Headers["Authorization"];
            var refreshToken = HttpContext.Request.Headers["RefreshToken"];

            var decodedToken = Helper.DecodeToken(accessToken);
            var username = decodedToken.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value;
            var fullname = decodedToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
            var claims = new[]
            {
                new Claim("UserName", username ?? string.Empty),
                new Claim("FullName", fullname ?? string.Empty),
                new Claim("AccessToken", accessToken?.ToString() ?? string.Empty),
                new Claim("RefreshToken", refreshToken?.ToString() ?? string.Empty),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationType);
            var principal = new ClaimsPrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            if (HttpContext != null)
            {
                HttpContext.User = principal;
            }
            Request.GetOwinContext().Authentication.SignIn(identity);
            return RedirectToAction("Index", "Home");
        }

    }
}