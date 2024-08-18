using Google.Protobuf;
using StateContainer.dto.Market;

namespace StateContainer.web.State
{
    public interface ISaveToDbService
    {
        protected Task ExecuteAsync(CancellationToken stoppingToken); 
        
        public Task StopAsync(CancellationToken cancellationToken);

        public void OnMarketInfoUpdated(List<MarketDto> newInfo);

    } 
}
