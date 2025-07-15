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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void SignOut()
        {
            // To sign out, issue a sign-out challenge to both cookie and OIDC middleware.
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties {RedirectUri = Url.Action("Index", "Home")},
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
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
            //in web api: HttpContext.Current.Request.Headers
            var accessToken = HttpContext.Request.Headers["Authorization"];
            var refreshToken = HttpContext.Request.Headers["RefreshToken"];
            var decodedToken = Helper.DecodeToken(accessToken);
            var username = decodedToken.Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
            var fullname = decodedToken.Claims.FirstOrDefault(x => x.Type == "name").Value;
            var claims = new[]
            {
                new Claim("UserName",username),
                new Claim("FullName",fullname),
                new Claim("AccessToken",accessToken.ToString()),
                new Claim("RefreshToken",refreshToken.ToString()),
            };

            var identity = new ClaimsIdentity(claims, "keycloak_sso_auth");

            Request.GetOwinContext().Authentication.SignIn(identity);
            return RedirectToAction("Index","Home");
        }

    }
}