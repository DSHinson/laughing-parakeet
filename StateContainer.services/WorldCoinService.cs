using StateContainer.dto.Market;
using System.Net.Http.Headers;
using System.Text.Json;

namespace StateContainer.services
{
    public class WorldCoinService
    {
        List<string> labels = new List<string>
        {
            "btcusd",
            "ethbtc",
            "ltcbtc",
            "ethusd",
            "ltcusd"
        };

        List<string> fiats = new List<string>
        {
            "usd",
            "eur",
            "btc",
            "eth"
        };

        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private static bool GetTickersAsyncRequesting = false;
        public WorldCoinService(string apiKey, HttpClient httpClient)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<string> GetLabels() => labels;
        public List<string> GetFiats() => fiats;

        public async Task<MarketResponseDto> GetMarketsAsync(string fiat = null, string label = null)
        {
            var url = "https://www.worldcoinindex.com/apiservice/v2getmarkets";
            var queryParameters = new List<string> { $"key={_apiKey}" };

            queryParameters.Add($"fiat={fiat ?? fiats[0]}");
            queryParameters.Add($"label={label ?? labels[0]}");

            url += "?" + string.Join("&", queryParameters);

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            var marketResponse = JsonSerializer.Deserialize<MarketResponseDto>(responseContent);

            return marketResponse;
        }

        public async Task<List<MarketDto>> GetTickersAsync(string label = null, string fiat = null)
        {
            if (GetTickersAsyncRequesting)
            {
                return null;
            }
            try
            {
                GetTickersAsyncRequesting = true;
                var url = "https://www.worldcoinindex.com/apiservice/ticker";
                var queryParameters = new List<string> { $"key={_apiKey}" };

                if (!string.IsNullOrEmpty(label))
                {
                    queryParameters.Add($"label={label}");
                }
                if (!string.IsNullOrEmpty(fiat))
                {
                    queryParameters.Add($"fiat={fiat}");
                }

                url += "?" + string.Join("&", queryParameters);

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var tickerResponse = JsonSerializer.Deserialize<MarketResponseDto>(responseContent);

                return tickerResponse.Markets;
            }
            catch {
                return null;
            }
            finally {
                GetTickersAsyncRequesting = false;
            }
            
        }
    }
}
