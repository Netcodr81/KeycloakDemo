using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace MVC.Framework.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private TracerProvider tracerProvider;
        private MeterProvider meterProvider;
        public static readonly ActivitySource ActivitySource = new ActivitySource("MVC.Framework.Web");
        public static ILogger Logger;

        private static string OTLP_Endpoint => ConfigurationManager.AppSettings["OTLP_Endpoint"];


        protected void Application_Start()
        {
            tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource("MVC.Framework.Web")
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: "MVC.Framework.Web", serviceVersion: "1.0.0"))
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(OTLP_Endpoint);
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .AddConsoleExporter()
                .Build();

            meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: "MVC.Framework.Web", serviceVersion: "1.0.0"))
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(OTLP_Endpoint);
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .AddConsoleExporter()
                .Build();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(logging =>
                {
                    logging.SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: "MVC.Framework.Web", serviceVersion: "1.0.0"));
                    logging.AddConsoleExporter();
                    logging.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(OTLP_Endpoint);
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
                });
                builder.SetMinimumLevel(LogLevel.Debug); // Set appropriate log level
            });

            Logger = loggerFactory.CreateLogger<MvcApplication>();

            Logger.LogInformation("Application started with OpenTelemetry configured");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            // Create a custom activity for each request
            using (var activity = ActivitySource.StartActivity("HTTP Request"))
            {
                activity?.SetTag("http.method", Request.HttpMethod);
                activity?.SetTag("http.url", Request.Url?.ToString());
            }

            // Log each request
            Logger?.LogInformation("Processing request: {Method} {Url}", Request.HttpMethod, Request.Url);
        }

        protected void Application_End()
        {
            tracerProvider?.Dispose();
            meterProvider?.Dispose();
        }
    }
}