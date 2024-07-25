
using StateContainer.dto.Market;
using StateContainer.Repository;

namespace StateContainer.web.State
{
    public class SaveToDbService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MarketStateContainer _stateContainer;
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
            // Save the new info to the database
        }
    }
}
