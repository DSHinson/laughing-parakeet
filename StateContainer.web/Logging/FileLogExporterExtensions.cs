using OpenTelemetry.Logs;
using OpenTelemetry;

namespace StateContainer.web.Logging
{
    public static class FileLogExporterExtensions
    {
        public static OpenTelemetryLoggerOptions AddFileExporter(this OpenTelemetryLoggerOptions options, string filePath)
        {
            return options.AddProcessor(new SimpleLogRecordExportProcessor(new FileLogExporter(filePath)));
        }
    }
}
