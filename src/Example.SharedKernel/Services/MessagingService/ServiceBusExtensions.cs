using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.SharedKernel.Services.MessagingService
{
    public static class ServiceBusExtensions
    {
        public static IServiceCollection AddServiceBus(this IServiceCollection services, Action<ServiceBusOptions> configure)
        {
            var options = new ServiceBusOptions();
            configure(options);

            // Register the publisher
            services.AddSingleton<IServiceBusPublisher>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusPublisher>>();
                return new ServiceBusPublisher(options.ConnectionString, logger, options.MessageTypeToQueueMap);
            });

            // Register the listener
            services.AddSingleton<ServiceBusListener>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusListener>>();
                var listener = new ServiceBusListener(options.ConnectionString, sp, logger);

                // Register all consumers
                foreach (var registerConsumer in options.ConsumerRegistrations)
                {
                    registerConsumer(listener);
                }

                return listener;
            });

            // Register a hosted service to manage ServiceBusListener lifecycle
            services.AddHostedService<ServiceBusListenerService>();

            return services;
        }

        // This extension method remains for backward compatibility
        // but now delegates to the ServiceBusListenerService
        public static IHostApplicationBuilder UseServiceBusListeners(this IHostApplicationBuilder builder)
        {
            // No need to do anything here as the IHostedService will handle starting the listeners
            return builder;
        }
    }

    public class ServiceBusListenerService : IHostedService
    {
        private readonly ServiceBusListener _listener;
        private readonly ILogger<ServiceBusListenerService> _logger;
        private Task? _listeningTask;

        public ServiceBusListenerService(
            ServiceBusListener listener,
            ILogger<ServiceBusListenerService> logger)
        {
            _listener = listener;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting ServiceBus listener service");
            _listeningTask = Task.Run(() => _listener.StartListening(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping ServiceBus listener service");

            try
            {
                if (_listener is IAsyncDisposable disposableListener)
                {
                    await disposableListener.DisposeAsync();
                }

                if (_listeningTask != null)
                {
                    try
                    {
                        // Wait for the task to complete with a timeout
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                        var completedTask = await Task.WhenAny(_listeningTask, timeoutTask);

                        if (completedTask == timeoutTask)
                        {
                            _logger.LogWarning("Timed out waiting for ServiceBus listener to stop");
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Error occurred while stopping ServiceBus listener");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping ServiceBus listener service");
            }
        }
    }
}
