using OpenTelemetry;
using System.Diagnostics;

namespace StateContainer.web.Logging
{
    public class FileTraceExporter : BaseExporter<Activity>
    {
        private readonly string _path;

        public FileTraceExporter(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }
        public override ExportResult Export(in Batch<Activity> batch)
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(_path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using (StreamWriter writer = new StreamWriter(_path, true))
            {
                foreach (var activity in batch)
                {
                    writer.WriteLine($"TraceId: {activity.TraceId}");
                    writer.WriteLine($"SpanId: {activity.SpanId}");
                    writer.WriteLine($"ParentId: {activity.ParentSpanId}");
                    writer.WriteLine($"Operation Name: {activity.DisplayName}");
                    writer.WriteLine($"Start Time: {activity.StartTimeUtc}");
                    writer.WriteLine($"Duration: {activity.Duration}");
                    writer.WriteLine("Attributes:");
                    foreach (var tag in activity.TagObjects)
                    {
                        writer.WriteLine($"\t{tag.Key}: {tag.Value}");
                    }
                    writer.WriteLine("Events:");
                    foreach (var evnt in activity.Events)
                    {
                        writer.WriteLine($"\t{evnt.Name} at {evnt.Timestamp}");
                        foreach (var attr in evnt.Tags)
                        {
                            writer.WriteLine($"\t\t{attr.Key}: {attr.Value}");
                        }
                    }
                    writer.WriteLine(new string('-', 50));
                }
            }

            return ExportResult.Success;
        }
    }
}
