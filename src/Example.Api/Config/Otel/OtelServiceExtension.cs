using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Example.Api.Config.Otel;

public static class OtelExtensions
{
    public static WebApplicationBuilder AddOtelLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddOpenTelemetry(x =>
        {
            x.SetResourceBuilder(ResourceBuilder.CreateEmpty()
                .AddService("Example.Api")
                .AddAttributes(new Dictionary<string, object>
                {
                    { "service.name", "Example.Api" },
                    { "deployment.environment", builder.Environment.EnvironmentName },
                    { "service.namespace", "Example" },
                    { "service.instance.id", Environment.MachineName },
                }));

            x.IncludeScopes = true;
            x.IncludeFormattedMessage = true;

            x.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/logs");
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                o.Headers = "X-Seq-ApiKey=NejmUF7HosNjThj9mCwH";
            });
        });

        return builder;
    }
}