using OpenTelemetry.Metrics;

namespace StateContainer.web.Logging
{
    public static class FileMetricExporterExtensions
    {
        public static MeterProviderBuilder AddFileExporter(this MeterProviderBuilder builder, string filePath)
        {
            return builder.AddReader(new PeriodicExportingMetricReader(new FileMetricExporter(filePath)));
        }
    }
}
