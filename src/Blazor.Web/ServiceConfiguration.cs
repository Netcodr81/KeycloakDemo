using System.Security.Claims;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

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
                options.RoleClaimType = KeycloakConstants.RoleClaimType;
            })
            .AddAuthorizationBuilder()
            .AddPolicy("Admin", policy => policy.RequireRole("BlazorWeb_Client_Admin"))
            .AddPolicy("User", policy => policy.RequireRole("BlazorWeb_Client_User"));

        return services;
    }

    public static IServiceCollection AddOpenTelemetryServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        var serviceName = "Blazor.Web";
        var serviceVersion = "1.0.0";
        var otlpEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");

        // Continue with OpenTelemetry configuration
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options => { options.Endpoint = new Uri(otlpEndpoint); });
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
                tracing.AddOtlpExporter(options => { options.Endpoint = new Uri(otlpEndpoint); });
            })
            .WithLogging(logging =>
            {
                logging.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });
            });

        return services;
    }
}