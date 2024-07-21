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
        public WorldCoinService(string apiKey, HttpClient httpClient)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            // Set User-Agent header
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

            // Set Accept headers (if needed)
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<string> GetLabels()
        {
            return labels;
        }
        public List<string> GetFiats() 
        { 
            return fiats; 
        }

        public async Task<MarketResponseDto> GetMarketsAsync(string fiat = null, string label = null)
        {
            // Build the request URL
            var url = "https://www.worldcoinindex.com/apiservice/v2getmarkets";
            var queryParameters = new List<string>();

            queryParameters.Add($"key={_apiKey}");

            if (!string.IsNullOrEmpty(fiat))
            {
                queryParameters.Add($"fiat={fiat}");
            }
            else
            {
                queryParameters.Add($"fiat={fiats[0]}");
            }
            if (!string.IsNullOrEmpty(label))
            {
                queryParameters.Add($"label={label}");
            }
            else
            {
                queryParameters.Add($"label={labels[0]}");
            }
            if (queryParameters.Count > 0)
            {
                url += "?" + string.Join("&", queryParameters);
            }
            url = "https://www.worldcoinindex.com/apiservice/ticker?key=qDMhOyHvbaKs1ABU21sMBwA2mf4QHURLBmL&label=ethbtc-ltcbtc&fiat=btc";



            HttpResponseMessage response = await _httpClient.GetAsync(url);



            // Read and deserialize the response
            string responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            var marketResponse = JsonSerializer.Deserialize<MarketResponseDto>(responseContent);

            return marketResponse;
        }

        public async Task<List<MarketDto>> GetTickersAsync(string label = null, string fiat = null)
        {
            // Build the request URL
            var url = "https://www.worldcoinindex.com/apiservice/ticker";
            var queryParameters = new List<string>();
            queryParameters.Add($"key={_apiKey}");

            if (!string.IsNullOrEmpty(label))
            {
                queryParameters.Add($"label={label}");
            }
            if (!string.IsNullOrEmpty(fiat))
            {
                queryParameters.Add($"fiat={fiat}");
            }

            if (queryParameters.Count > 0)
            {
                url += "?" + string.Join("&", queryParameters);
            }
            url = "https://www.worldcoinindex.com/apiservice/ticker?key=qDMhOyHvbaKs1ABU21sMBwA2mf4QHURLBmL&label=ethbtc-ltcbtc&fiat=btc";
            // Perform the HTTP GET request
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Read and deserialize the response
            string responseContent = await response.Content.ReadAsStringAsync();
            var tickerResponse = JsonSerializer.Deserialize<MarketResponseDto>(responseContent);

            return tickerResponse.Markets;
        }
    }
}
