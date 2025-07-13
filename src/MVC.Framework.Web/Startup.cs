using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(MVC.Framework.Web.Startup))]

namespace MVC.Framework.Web
{
    public class Startup
    {
        // Reading Keycloak configuration from Web.config
        private static string KeycloakRealm => ConfigurationManager.AppSettings["Keycloak:Realm"];
        private static string KeycloakAuthority => ConfigurationManager.AppSettings["Keycloak:Authority"];
        private static string KeycloakClientId => ConfigurationManager.AppSettings["Keycloak:ClientId"];
        private static string KeycloakClientSecret => ConfigurationManager.AppSettings["Keycloak:ClientSecret"];
        private static string KeycloakRedirectUri => ConfigurationManager.AppSettings["Keycloak:RedirectUri"];
        private static string KeycloakPostLogoutRedirectUri => ConfigurationManager.AppSettings["Keycloak:PostLogoutRedirectUri"];

        public void Configuration(IAppBuilder app)
        {
            // Set the default authentication type to cookies
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            // Enable cookie authentication
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Account/SignIn") // Optional: specify a login path
            });

            // Configure OpenID Connect middleware for Keycloak
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = KeycloakClientId,
                ClientSecret = KeycloakClientSecret,
                Authority = $"{KeycloakAuthority}{KeycloakRealm}",
                RedirectUri = KeycloakRedirectUri,
                PostLogoutRedirectUri = KeycloakPostLogoutRedirectUri,
                ResponseType = OpenIdConnectResponseType.Code,
                Scope = "openid profile email",
                RequireHttpsMetadata = false, // Set to true in production

                // Link the OIDC middleware to the cookie middleware
                SignInAsAuthenticationType = CookieAuthenticationDefaults.AuthenticationType,

                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    NameClaimType = "preferred_username", // Use 'preferred_username' as the name claim
                    RoleClaimType = "roles" // Use 'roles' as the role claim (requires Keycloak mapper)
                },

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = notification =>
                    {
                        var identity = notification.AuthenticationTicket.Identity;
                        var protocolMessage = notification.ProtocolMessage;

                        // Persist tokens in the authentication cookie if needed
                        if (!string.IsNullOrEmpty(protocolMessage.AccessToken))
                        {
                            identity.AddClaim(new Claim("access_token", protocolMessage.AccessToken));
                        }

                        if (!string.IsNullOrEmpty(protocolMessage.IdToken))
                        {
                            identity.AddClaim(new Claim("id_token", protocolMessage.IdToken));
                        }

                        return Task.FromResult(0);
                    },
                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Home/Error?message=" + Uri.EscapeDataString(context.Exception.Message));
                        return Task.FromResult(0);
                    },
                    RedirectToIdentityProvider = notification =>
                    {
                        // Handle logout redirection
                        if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                        {
                            var logoutUri = $"{KeycloakAuthority}{KeycloakRealm}/protocol/openid-connect/logout?post_logout_redirect_uri={KeycloakPostLogoutRedirectUri}&client_id={KeycloakClientId}";
                            notification.Response.Redirect(logoutUri);
                            notification.HandleResponse();
                        }

                        return Task.FromResult(0);
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
    }
}
