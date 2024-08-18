using System.Reflection;

namespace StateContainer.web.Logging
{
    public class ServiceLoggingProxy<T> : DispatchProxy
    {
        private T _instance;
        private ILogger<T> _logger;
        public static T Create(T instance, ILogger<T> logger)
        {
            // Step 1: Create an instance of the ServiceLoggingProxy<T> proxy.
            // Create<T, ServiceLoggingProxy<T>>` is a method of `DispatchProxy` that creates a new proxy instance.
            // This instance is a dynamically generated class that derives from `ServiceLoggingProxy<T>`.
            object proxy = Create<T, ServiceLoggingProxy<T>>();

            // Step 2: Initialize the proxy instance with the decorated object and logger.
            // The `SetParameters` method is called to set up the proxy instance with the necessary parameters.
            SetParameters();

            // Step 3: Return the proxy cast to type `T`.
            // The proxy created and initialized is cast to type `T`, which is the interface or base class type.
            return (T)proxy;

            // Local method to set up the proxy instance.
            // This method assigns the provided decorated object and logger to the proxy's internal fields.
            void SetParameters()
            {
                // Step 4: Cast the proxy object to `ServiceLoggingProxy<T>`.
                // This cast allows access to the private fields of the `ServiceLoggingProxy<T>` class.
                var me = ((ServiceLoggingProxy<T>)proxy) ?? throw new ArgumentNullException(nameof(proxy));

                // Step 5: Assign the logger to the proxy's `_logger` field.
                me._logger = logger ?? throw new ArgumentNullException(nameof(logger));

                // Step 6: Assign the decorated object to the proxy's `_instance` field.
                me._instance = instance ?? throw new ArgumentNullException(nameof(instance));
            }
        }


        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            _ = targetMethod ?? throw new ArgumentException(nameof(targetMethod));

            try
            {
                _logger.LogInformation($"Invoking method {targetMethod.Name} with arguments {string.Join(", ", args ?? Array.Empty<object>())}");
                 var result = targetMethod.Invoke(_instance, args);
                _logger.LogInformation($"Method {targetMethod.Name} completed with result {result}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Method {targetMethod.Name} threw an exception");
                throw;
            }
        }
    }


}
