using StateContainer.dto.Market;
namespace StateContainer.web.State
{
    public class MarketStateContainer
    {
        private string selectedFiat;
        private string selectedLabel;
        public event Action<string> OnFiatChanged;
        public event Action<string> OnLabelChanged;
        public event Action<List<MarketDto>> MarketInfoUpdated;
        private  List<MarketDto> MarketInfo = new();
                

        public void SetSelectedFiat(string fiat)
        {
            if (selectedFiat != fiat)
            {
                selectedFiat = fiat;
                OnFiatChanged?.Invoke(fiat); 
            }
        }

        public void SetSelectedLabel(string label)
        {
            if (selectedLabel != label)
            {
                selectedLabel = label;
                OnLabelChanged?.Invoke(label);
            }
        }

        public void updateMarketInfo(List<MarketDto> market)
        {
            MarketInfo.AddRange(market);
            MarketInfoUpdated?.Invoke(MarketInfo);
        }

        public List<MarketDto> getCurrentState()
        {
            return MarketInfo;
        }

    }

   
}
