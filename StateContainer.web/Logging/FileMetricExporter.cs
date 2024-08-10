using OpenTelemetry.Metrics;
using OpenTelemetry;

namespace StateContainer.web.Logging
{
    public class FileMetricExporter : BaseExporter<Metric>
    {
        private readonly string _path;

        public FileMetricExporter(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public override ExportResult Export(in Batch<Metric> batch)
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(_path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using (StreamWriter writer = new StreamWriter(_path, true))
            {
                foreach (var metric in batch)
                {
                    writer.WriteLine($"Metric: {metric.Name}");
                    foreach (var point in metric.GetMetricPoints())
                    {
                        writer.WriteLine($"\tTimestamp: {point.EndTime}");
                        writer.WriteLine($"\tMetric Type: {metric.MetricType}");

                        // Handle different metric types
                        switch (metric.MetricType)
                        {
                            case MetricType.LongSum:
                                writer.WriteLine($"\tValue: {point.GetSumLong()}");
                                break;
                            case MetricType.DoubleSum:
                                writer.WriteLine($"\tValue: {point.GetSumDouble()}");
                                break;
                            case MetricType.LongGauge:
                                writer.WriteLine($"\tValue: {point.GetGaugeLastValueLong()}");
                                break;
                            case MetricType.DoubleGauge:
                                writer.WriteLine($"\tValue: {point.GetGaugeLastValueDouble()}");
                                break;
                            case MetricType.Histogram:
                                writer.WriteLine($"\tHistogram Count: {point.GetHistogramCount()}");
                                writer.WriteLine($"\tHistogram Sum: {point.GetHistogramSum()}");
                                break;
                            // Add cases for other MetricTypes if needed
                            default:
                                writer.WriteLine($"\tUnsupported metric type: {metric.MetricType}");
                                break;
                        }
                    }
                }
                writer.WriteLine(new string('-', 50));
            }

            return ExportResult.Success;
        }
    }
}
