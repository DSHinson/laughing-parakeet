using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateContainer.services
{
    public class ApiKeyProvider
    {
        private readonly IConfiguration _configuration;

        // Constructor for regular use
        public ApiKeyProvider()
        {
            var builder = new ConfigurationBuilder().AddUserSecrets<ApiKeyProvider>();

            _configuration = builder.Build();
        }

        // Overloaded constructor for testing
        public ApiKeyProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetApiKey()
        {
            string apiKey = _configuration["ApiKeys:WorldCoinIndex"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("API key not found. Please set the user secret with key 'ApiKeys:WorldCoinIndex'.");
            }

            return apiKey;
        }
    }
}
