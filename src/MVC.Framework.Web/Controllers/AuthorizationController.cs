using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

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
    }
}