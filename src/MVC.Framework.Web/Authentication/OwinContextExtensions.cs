using System.Security.Claims;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace MVC.Framework.Web.Authentication
{
    public static class OwinContextExtensions
    {
        /// <summary>
        /// Gets the current OWIN context from the HttpContext
        /// </summary>
        public static IOwinContext GetOwinContext(this HttpContext context)
        {
            return HttpContextBaseExtensions.GetOwinContext(new HttpContextWrapper(context));
        }

        /// <summary>
        /// Gets a token from the authentication properties
        /// </summary>
        public static string GetToken(this IOwinContext context, string tokenName)
        {
            var authResult = context.Authentication.AuthenticateAsync("Cookies").Result;
            if (authResult == null)
            {
                return null;
            }

            string token = null;
            if (authResult.Properties.Dictionary.TryGetValue($".Token.{tokenName}", out token))
            {
                return token;
            }

            return null;
        }

        /// <summary>
        /// Gets a token from the authentication properties via the current HttpContext
        /// </summary>
        public static string GetToken(this HttpContext context, string tokenName)
        {
            return context.GetOwinContext().GetToken(tokenName);
        }
    }
}
