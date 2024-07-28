
using StateContainer.dto.Market;
using StateContainer.Repository;
using System.Text.Json;

namespace StateContainer.web.State
{
    public class SaveToDbService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MarketStateContainer _stateContainer;
        private static bool Saving =false;
        private long lastTimeStamp = 0;
        string WriteContent = "";

        public SaveToDbService(IServiceProvider serviceProvider, MarketStateContainer stateContainer)
        {
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _stateContainer = stateContainer ?? throw new ArgumentNullException(nameof(_stateContainer));
            _stateContainer.MarketInfoUpdated += OnMarketInfoUpdated;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initialization logic, if any
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Clean-up logic, if any
            _stateContainer.MarketInfoUpdated -= OnMarketInfoUpdated;
            return Task.CompletedTask;
        }

        private void OnMarketInfoUpdated(List<MarketDto> newInfo)
        {
            if (Saving)
            {
                return;
            }
            try
            {
                Saving = true;
                newInfo = newInfo?.Where(x => x.Timestamp > lastTimeStamp).ToList();
                if (newInfo.Count > 0)
                {
                    lastTimeStamp = newInfo?.Max(x => x.Timestamp) ?? 0;
                    string temp = JsonSerializer.Serialize(newInfo);
                    if (!string.IsNullOrEmpty(WriteContent))
                    {
                        WriteContent = WriteContent.Substring(0, WriteContent.Length - 1);
                        WriteContent += ",";
                    }
                    else
                    {
                        WriteContent = "[";
                    }
                   

                    WriteContent += temp;
                    WriteContent += "]";
                }
            }
            catch (Exception ex)
            {
            }
            finally {
                Saving = false;
            }
        }
    }
}
