using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using StateContainer.dto.Market;
using StateContainer.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StateContainer.Tests
{
    public class WorldCoinServiceTests
    {
        private HttpClient _httpClient;
        private WorldCoinService _worldCoinService;
        private MarketResponseDto expectedResponse;

        [SetUp]
        public void Setup()
        {
            expectedResponse = new MarketResponseDto();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var marketResponse = JsonConvert.SerializeObject(expectedResponse);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(marketResponse), 
                });

            _httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var apiKey = "test_api_key";
            _worldCoinService = new WorldCoinService(apiKey, _httpClient);
        }

        [TearDown]
        public void TearDown() 
        { 
            _httpClient.Dispose(); 
        }

        [Test]
        public async Task GetMarketsAsyncTest1()
        {
            
            // Act
            var result = await _worldCoinService.GetMarketsAsync();

            // Assert
            Assert.That(result, Is.EqualTo(expectedResponse));
        }
        [Test]
        public async Task GetTickerAsyncTest1()
        {

            // Act
            var result = await _worldCoinService.GetTickersAsync();

            // Assert
            //Assert.That(result, Is.EqualTo(expectedResponse));
        }
    }
}
