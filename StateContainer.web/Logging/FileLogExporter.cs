using OpenTelemetry;
using OpenTelemetry.Logs;

namespace StateContainer.web.Logging
{
    public class FileLogExporter : BaseExporter<LogRecord>
    {
        private readonly string _path;

        public FileLogExporter(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(_path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (StreamWriter writer = new StreamWriter(_path, true))
            {
                foreach (var logRecord in batch)
                {
                    writer.WriteLine($"Timestamp: {logRecord.Timestamp:O}");
                    writer.WriteLine($"Log Level: {logRecord.LogLevel}");
                    writer.WriteLine($"Category: {logRecord.CategoryName}");
                    writer.WriteLine($"Event ID: {logRecord.EventId}");
                    writer.WriteLine($"Message: {logRecord.FormattedMessage}");
                    writer.WriteLine("State:");
                    if (logRecord.State != null)
                    {
                        foreach (var item in logRecord.State.ToString().Split('\n'))
                        {
                            writer.WriteLine($"\t{item}");
                        }
                    }
                    writer.WriteLine(new string('-', 50));
                }
            }

            return ExportResult.Success;
        }
    }
}
