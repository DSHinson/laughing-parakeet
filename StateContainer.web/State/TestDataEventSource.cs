using Microsoft.Extensions.Options;
using StateContainer.dto.Market;
using StateContainer.services;
using System.Text.Json;

namespace StateContainer.web.State
{

    public class TestDataEventSource : BackgroundService
    {
        List<List<MarketDto>> TestData;
        private readonly ILogger<TestDataEventSource> _logger;
        private readonly MarketStateContainer _stateContainer;
        private CancellationTokenSource _cts;

        public TestDataEventSource(ILogger<TestDataEventSource> logger, MarketStateContainer stateContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateContainer = stateContainer ?? throw new ArgumentNullException(nameof(stateContainer));
            _cts = new CancellationTokenSource();

            string directoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Combine the directory path with the filename
            string filePath = Path.Combine(directoryPath, "State/TestDataSource.json");

            // Read the JSON file contents
            string jsonString = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            TestData = JsonSerializer.Deserialize<List<List<MarketDto>>>(jsonString, options);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Combine the stopping token with the internal cancellation token source
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cts.Token))
            {
                await PollAsync(linkedCts.Token);
            }
        }

        private async Task PollAsync(CancellationToken token)
        {
            _logger.LogInformation("TestDataEventSource started.");

            while (!token.IsCancellationRequested)
            {
                if (TestData != null && TestData.Any())
                {
                    List<MarketDto> currentData = TestData.First();
                    TestData.Remove(currentData);
                    _stateContainer.updateMarketInfo(currentData);
                    await Task.Delay(1000, token);
                }
                else
                {
                    _cts.Cancel();
                }

            }

            _logger.LogInformation("TestDataEventSource stopped.");
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            // Clean-up logic, if any
            return Task.CompletedTask;
        }
    }
}
