using System;
using System.Configuration;
using System.Linq;
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
using MVC.Framework.Web.Helpers;
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
                    Scope = "openid profile email offline_access",
                    RedirectUri = KeycloakRedirectUri,
                    RedeemCode = true,
                    MetadataAddress = "http://localhost:8080/realms/keycloak_demo/.well-known/openid-configuration",
                    RequireHttpsMetadata = false,
                    PostLogoutRedirectUri = KeycloakPostLogoutRedirectUri,
                    ProtocolValidator = new OpenIdConnectProtocolValidator
                    {
                        RequireNonce = false,
                        NonceLifetime = TimeSpan.FromMinutes(30),
                        // Set RequireStateValidation to false if you're having persistent issues
                        // This is less secure but will resolve the state validation error
                        RequireStateValidation = false
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {

                        SecurityTokenValidated = context =>
                        {
                            var identity = context.AuthenticationTicket.Identity;

                            // Extract tokens from the protocol message
                            var idToken = context.ProtocolMessage.IdToken;
                            var accessToken = context.ProtocolMessage.AccessToken;
                            var refreshToken = context.ProtocolMessage.RefreshToken;

                            // Add tokens as claims to the identity
                            if (!string.IsNullOrEmpty(idToken))
                            {
                                identity.AddClaim(new Claim("id_token", idToken));
                            }
                            // Note: It's often better to store the access_token in a claim as well
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                identity.AddClaim(new Claim("access_token", accessToken));
                            }

                            // Your custom logic from the Callback action should go here
                            var decodedToken = Helper.DecodeToken(accessToken); // Assuming Helper.DecodeToken exists
                            var username = decodedToken.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value;
                            var fullname = decodedToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value;

                            identity.AddClaim(new Claim("UserName", username ?? string.Empty));
                            identity.AddClaim(new Claim("FullName", fullname ?? string.Empty));
                            identity.AddClaim(new Claim("AccessToken", accessToken ?? string.Empty));
                            identity.AddClaim(new Claim("RefreshToken", refreshToken ?? string.Empty));

                            return Task.CompletedTask;
                        }

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