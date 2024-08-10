using OpenTelemetry.Trace;
using OpenTelemetry;

namespace StateContainer.web.Logging
{
    public static class FileTraceExporterExtensions
    {
        public static TracerProviderBuilder AddFileExporter(this TracerProviderBuilder builder, string filePath)
        {
            return builder.AddProcessor(new SimpleActivityExportProcessor(new FileTraceExporter(filePath)));
        }
    }
}
