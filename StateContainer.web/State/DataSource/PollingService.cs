using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateContainer.web.State.DataSource
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using StateContainer.services;
    using StateContainer.web.State;

    public class PollingService : BackgroundService, IDataSource
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PollingService> _logger;
        private readonly MarketStateContainer _stateContainer;
        private CancellationTokenSource _cts;
        private static bool _pollingAlready = false;
        protected string selectedFiat;
        protected string selectedLabel;

        // Singleton instance
        private static PollingService _instance = null;

        // Static constructor to enforce singleton pattern
        static PollingService()
        {
            _instance = null;
        }

        // Regular constructor but with a singleton check
        public PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger, MarketStateContainer stateContainer)
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("An instance of PollingService already exists.");
            }

            _serviceProvider = serviceProvider;
            _logger = logger;
            _stateContainer = stateContainer;
            _cts = new CancellationTokenSource();
            selectedFiat = _stateContainer.getselectedFiat();
            selectedLabel = _stateContainer.getselectedLabel();
            _stateContainer.OnFiatChanged += (newval) =>
            {
                selectedFiat = newval;
            };
            _stateContainer.OnLabelChanged += (newval) =>
            {
                selectedLabel = newval;
            };

            _instance = this; // Set the static instance

            StartPolling(); // Consider moving this outside the constructor if needed
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
            _logger.LogInformation("PollingService started.");

            while (!token.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedService = scope.ServiceProvider.GetRequiredService<WorldCoinService>();
                    // Use the scoped service to get the updated state
                    var updatedState = await scopedService.GetTickersAsync(selectedLabel, selectedFiat);
                    _stateContainer.updateMarketInfo(updatedState);
                }

                // Wait for 1 minute
                await Task.Delay(1000 * 60, token);
            }

            _logger.LogInformation("PollingService stopped.");
        }

        public void StartPolling()
        {
            if (!_pollingAlready)
            {
                _pollingAlready = true;
                _cts = new CancellationTokenSource();
                ExecuteAsync(_cts.Token); // Start polling
            }
        }

        public void StopPolling()
        {
            if (_pollingAlready)
            {
                _cts.Cancel();
                _pollingAlready = false;
            }
        }

        // Method to retrieve the singleton instance
        public static PollingService GetInstance(IServiceProvider serviceProvider, ILogger<PollingService> logger, MarketStateContainer stateContainer)
        {
            if (_instance == null)
            {
                _instance = new PollingService(serviceProvider, logger, stateContainer);
            }

            return _instance;
        }
    }


}
