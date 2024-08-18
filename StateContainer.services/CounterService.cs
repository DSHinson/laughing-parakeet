using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateContainer.services
{
    public class CounterService : ICounterService
    {
        private readonly ILogger<CounterService> _logger;

        public CounterService(ILogger<CounterService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public int incrementValue(int sum, int increment)
        {
            _logger.LogInformation("logging from within the method");
            return sum + increment;
        }
    }
}
