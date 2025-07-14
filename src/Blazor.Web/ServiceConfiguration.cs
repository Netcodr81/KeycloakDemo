using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Blazor.Web;

public static class ServiceConfiguration
{
    public static IServiceCollection AddKeycloakAuthenticationAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddKeycloakWebApp(
                configuration.GetSection(KeycloakAuthenticationOptions.Section),
                configureOpenIdConnectOptions: options =>
                {
                    // we need this for front-channel sign-out
                    options.SaveTokens = true;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.RequireHttpsMetadata = false;
                    options.UseSecurityTokenValidator = true;
                    options.MapInboundClaims = true;
                    options.Events = new OpenIdConnectEvents
                    {
                        OnSignedOutCallbackRedirect = context =>
                        {
                            context.Response.Redirect("/");
                            context.HandleResponse();

                            return Task.CompletedTask;
                        }
                    };
                }
            );

        services.AddKeycloakAuthorization(configuration)
            .AddAuthorizationBuilder()
            .AddPolicy("Admin", policy => policy.RequireResourceRoles("admin"))
            .AddPolicy("user", policy => policy.RequireResourceRoles("user"));

        return services;
    }
}