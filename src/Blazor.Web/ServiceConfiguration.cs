using System.Diagnostics.Metrics;
using System.Reflection;
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
        const string serviceName = "Blazor.Web";
        var otlpEndpoint = new Uri(builder.Configuration.GetValue<string>("OTLP_Endpoint")!);

        // Continue with OpenTelemetry configuration
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService(serviceName)
                    .AddAttributes(new[]
                    {
                        new KeyValuePair<string, object>("service.version",
                            Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                    });
            })
            .WithTracing(tracing =>
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter()
                    .AddOtlpExporter(options =>
                        options.Endpoint = otlpEndpoint)
            )
            .WithMetrics(metrics =>
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Metrics provides by ASP.NET
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter(ApplicationDiagnostics.Meter.Name)
                    .AddConsoleExporter()
                    .AddOtlpExporter(options =>
                        options.Endpoint = otlpEndpoint)
            )
            .WithLogging(
                logging=>
                    logging
                        .AddConsoleExporter()
                        .AddOtlpExporter(options =>
                            options.Endpoint = otlpEndpoint)
            );

        return services;
    }
}

public static class ApplicationDiagnostics
{
    private const string ServiceName = "Blazor.Web";
    public static readonly Meter Meter = new(ServiceName);

    public static readonly Counter<long> ClientsCreatedCounter = Meter.CreateCounter<long>("clients.created");
}