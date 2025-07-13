using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace MVC.Web;

public static class ServiceConfiguration
{
    public static IServiceCollection AddKeycloakAuthenticationAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddKeycloakWebApp(
                configuration.GetSection(KeycloakAuthenticationOptions.Section),
                configureOpenIdConnectOptions:
                options =>
                {
                    options.SaveTokens = true;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.RequireHttpsMetadata = false;
                    options.UseSecurityTokenValidator = true;
                    options.MapInboundClaims = true;
                    options.Events = new OpenIdConnectEvents
                    {
                         OnTokenValidated = context =>
                        {
                            var identity = (ClaimsIdentity)context.Principal.Identity;

                            // Find and remove the original complex 'realm_access' and 'resource_access' claims
                            var realmAccessClaim = identity.FindFirst("realm_access");
                            var resourceAccessClaim = identity.FindFirst("resource_access");

                            if (realmAccessClaim != null)
                            {
                                identity.RemoveClaim(realmAccessClaim);
                                using var realmAccessDoc = JsonDocument.Parse(realmAccessClaim.Value);
                                var realmRoles = realmAccessDoc.RootElement.GetProperty("roles");
                                foreach (var role in realmRoles.EnumerateArray())
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()));
                                }
                            }

                            if (resourceAccessClaim != null)
                            {
                                identity.RemoveClaim(resourceAccessClaim);
                                // Optionally, you can parse client-specific roles from here if needed
                                // Example for a client named 'my-client':
                                // using var resourceAccessDoc = JsonDocument.Parse(resourceAccessClaim.Value);
                                // if (resourceAccessDoc.RootElement.TryGetProperty("my-client", out var client))
                                // {
                                //     var clientRoles = client.GetProperty("roles");
                                //     foreach (var role in clientRoles.EnumerateArray())
                                //     {
                                //         identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()));
                                //     }
                                // }
                            }

                            return Task.CompletedTask;
                        },
                        OnSignedOutCallbackRedirect = context =>
                        {
                            context.Response.Redirect($"/Home/Index");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                },
                configureCookieAuthenticationOptions: opt =>
                {
                    opt.LogoutPath = "/logout";
                    opt.AccessDeniedPath = "/unauthorized";
                });

        // Register Keycloak authorization services
        services.AddKeycloakAuthorization(configuration);

        // Add authorization policies
        services.AddAuthorizationBuilder()
            .AddPolicy("Admin", policy => policy.RequireResourceRoles("basic_user"));

        return services;
    }
}