using System.Configuration;
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

            // Retrieve the ID token
            var idToken = authResult?.Identity?.Claims
                .FirstOrDefault(c => c.Type == "id_token")?.Value;

            // Get Keycloak configuration
            var realm = ConfigurationManager.AppSettings["Keycloak:Realm"];
            var authority = ConfigurationManager.AppSettings["Keycloak:Authority"];
            var postLogoutRedirectUri = ConfigurationManager.AppSettings["Keycloak:PostLogoutRedirectUri"];

            // Construct the logout URL manually
            var logoutUrl = $"http://localhost:8080/realms/keycloak_demo/protocol/openid-connect/logout" +
                            $"?id_token_hint={HttpUtility.UrlEncode(idToken)}" +
                            $"&post_logout_redirect_uri={HttpUtility.UrlEncode(postLogoutRedirectUri)}";

            // Clear local authentication
            HttpContext.GetOwinContext().Authentication.SignOut(
                CookieAuthenticationDefaults.AuthenticationType,
                OpenIdConnectAuthenticationDefaults.AuthenticationType
            );

            // Redirect to the constructed logout URL
            Response.Redirect(logoutUrl);


        }


        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }


    }
}