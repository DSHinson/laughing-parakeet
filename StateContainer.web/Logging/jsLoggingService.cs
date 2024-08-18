using Microsoft.JSInterop;

namespace StateContainer.web.Logging
{
    public class jsLoggingService
    {
        private readonly ILogger<jsLoggingService> _logger;

        public jsLoggingService(ILogger<jsLoggingService> logger)
        {
            _logger = logger;
        }

        [JSInvokable]
        public Task LogButtonClick(string buttonText)
        {
            _logger.LogInformation($"Button clicked: {buttonText}");
            return Task.CompletedTask;
        }
    }
}
