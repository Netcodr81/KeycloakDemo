using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

                    options.Events = new OpenIdConnectEvents
                    {

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

        // This configuration will now correctly map realm roles and the roles
        // for the specified resource to the standard ClaimTypes.Role.
        services.AddAuthorization()
            .AddKeycloakAuthorization(options =>
            {
                options.Realm = configuration.GetValue<string>("Keycloak:realm");
                options.AuthServerUrl = configuration.GetValue<string>("Keycloak:auth-server-url");
                options.SslRequired = configuration.GetValue<string>("Keycloak:ssl-required");
                options.Resource = configuration.GetValue<string>("Keycloak:resource");
                options.VerifyTokenAudience = true;
                options.Credentials = new KeycloakClientInstallationCredentials
                {
                    Secret = configuration.GetValue<string>("Keycloak:credentials:secret")
                };
                options.EnableRolesMapping = RolesClaimTransformationSource.All;
                options.RolesResource = configuration.GetValue<string>("Keycloak:resource");
                // This should now point to the standard role claim type.
                options.RoleClaimType = KeycloakConstants.RoleClaimType;
            })
            .AddAuthorizationBuilder()
            .AddPolicy("Admin", policy => policy.RequireRole("MVC_Web_Admin"))
            .AddPolicy("User", policy => policy.RequireRole("MVC_Web_User"));

        return services;
    }

    public static IServiceCollection AddOpenTelemetryServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        var serviceName = "MVC.Web";
        var serviceVersion = "1.0.0";

        // Continue with OpenTelemetry configuration
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options => { options.Endpoint = new Uri("http://localhost:4317"); });
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request.headers.user_agent", request.Headers.UserAgent);
                            activity.SetTag("http.request.headers.host", request.Headers.Host);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            if (request.RequestUri != null)
                            {
                                activity.SetTag("http.request.uri", request.RequestUri.ToString());
                            }
                        };
                    })
                    .SetSampler(new AlwaysOnSampler());


                // Add OTLP exporter for Aspire Dashboard
                tracing.AddOtlpExporter(options => { options.Endpoint = new Uri("http://localhost:4317"); });
            });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;
            logging.AddOtlpExporter(options => { options.Endpoint = new Uri("http://localhost:4317"); });
        });

        return services;
    }
}