using System;
using System.Configuration;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Owin;
using Owin.Security.Keycloak;

namespace MVC.Framework.Web
{
    public partial class Startup
    {
        // Reading Keycloak configuration from Web.config
        private static string KeycloakRealm => ConfigurationManager.AppSettings["Keycloak:Realm"];
        private static string KeycloakAuthority => ConfigurationManager.AppSettings["Keycloak:Authority"];
        private static string KeycloakClientId => ConfigurationManager.AppSettings["Keycloak:ClientId"];
        private static string KeycloakClientSecret => ConfigurationManager.AppSettings["Keycloak:ClientSecret"];
        private static string KeycloakRedirectUri => ConfigurationManager.AppSettings["Keycloak:RedirectUri"];
        private static string KeycloakPostLogoutRedirectUri => ConfigurationManager.AppSettings["Keycloak:PostLogoutRedirectUri"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                CookieHttpOnly = true,
                CookieName = "KeycloakCookie",
                CookieSameSite = SameSiteMode.None,
                CookieSecure = CookieSecureOption.Always,
                CookieDomain = "localhost",
                CookiePath = "/",

            });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    Authority = KeycloakAuthority,
                    ClientId = KeycloakClientId,
                    ClientSecret = KeycloakClientSecret,
                    ResponseType = OpenIdConnectResponseType.Code,
                    SaveTokens = true,
                    Scope = "openid profile email",
                    RedirectUri = KeycloakRedirectUri,
                    RedeemCode = true,
                    MetadataAddress = "http://localhost:8080/realms/keycloak_demo/.well-known/openid-configuration",
                    RequireHttpsMetadata = false,
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        TokenResponseReceived = async (responseToken) =>
                        {
                            responseToken.Request.Headers.Add("Authorization", new[] { responseToken.TokenEndpointResponse.AccessToken });
                            responseToken.Request.Headers.Add("RefreshToken", new[] { responseToken.TokenEndpointResponse.RefreshToken });

                            responseToken.SkipToNextMiddleware();
                        },
                    }
                });

        }

        private void MapKeycloakClaimsToIdentity(ClaimsIdentity identity, SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            // Mapping Keycloak claims to standard claims
            var nameClaim = identity.FindFirst("preferred_username") ?? identity.FindFirst("name");
            if (nameClaim != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));
            }

            var emailClaim = identity.FindFirst("email");
            if (emailClaim != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim.Value));
            }

            var userIdClaim = identity.FindFirst("sub");
            if (userIdClaim != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userIdClaim.Value));
            }
        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}