using Microsoft.Extensions.Configuration;
using StateContainer.services;

namespace StateContainer.Tests
{
    public class ApiKeyProviderTests
    {
        private IConfiguration _configuration;
        [SetUp]
        public void Setup()
        {
            var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(new[] {
                new KeyValuePair<string, string>("ApiKeys:WorldCoinIndex", "test_api_key")
            });

            _configuration = builder.Build();
        }

        [Test]
        public void Test1()
        {
            // Arrange
            var apiKeyProvider = new ApiKeyProvider(_configuration);

            // Act
            string apiKey = apiKeyProvider.GetApiKey();

            // Assert
            Assert.NotNull(apiKey);
            Assert.That(apiKey, Is.EqualTo("test_api_key"));
            ;
        }
    }
}