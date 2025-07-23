using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using MVC.Framework.Web.Helpers;
using Newtonsoft.Json.Linq;
using Owin;
using Owin.Security.Keycloak;
using SameSiteMode = Microsoft.Owin.SameSiteMode;

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
                    UseTokenLifetime = false,
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
                            context.AuthenticationTicket.Properties.AllowRefresh = true;
                            context.AuthenticationTicket.Properties.IsPersistent = true;

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

                            var resourceAccessClaim = identity.FindFirst("resource_access");
                            if (resourceAccessClaim != null)
                            {
                                var resourceAccess = JObject.Parse(resourceAccessClaim.Value);
                                var clientRoles = resourceAccess[KeycloakClientId]?["roles"];
                                if (clientRoles != null)
                                {
                                    foreach (var role in clientRoles)
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                                    }
                                }

                            }

                            var realmAccessClaim = identity.FindFirst("realm_access");
                            if (realmAccessClaim != null)
                            {
                                var realmAccess = JObject.Parse(realmAccessClaim.Value);
                                var realmRoles = realmAccess["roles"];

                                if (realmRoles != null)
                                {
                                    foreach (var role in realmRoles)
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                                    }
                                }

                            }

                            // Your custom logic from the Callback action should go here
                            var decodedToken = Helper.DecodeToken(accessToken); // Assuming Helper.DecodeToken exists
                            var username = decodedToken.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value;
                            var fullname = decodedToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value;

                            identity.AddClaim(new Claim("UserName", username ?? string.Empty));
                            identity.AddClaim(new Claim("FullName", fullname ?? string.Empty));
                           // identity.AddClaim(new Claim("AccessToken", accessToken ?? string.Empty));
                           identity.AddClaim(new Claim("refresh_token", refreshToken ?? string.Empty));

                            return Task.CompletedTask;
                        }
                    }
                });

        }


    }
}