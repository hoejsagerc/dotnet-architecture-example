using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Example.SharedKernel.Services.MessagingService
{
    public class ServiceBusListener : IAsyncDisposable
    {
        private readonly string _connectionString;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServiceBusListener> _logger;
        private readonly List<QueueConsumerMapping> _consumerMappings = new();
        private readonly List<ServiceBusProcessor> _processors = new();
        private ServiceBusClient? _client;
        private CancellationTokenSource? _stoppingCts;

        public ServiceBusListener(string connectionString, IServiceProvider serviceProvider, ILogger<ServiceBusListener> logger)
        {
            _connectionString = connectionString;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void RegisterConsumer<TMessage, TConsumer>(string queueName)
            where TMessage : class
            where TConsumer : class, IConsumer<TMessage>
        {
            _consumerMappings.Add(new QueueConsumerMapping(queueName, typeof(TMessage), typeof(TConsumer)));
        }

        public async Task StartListening(CancellationToken cancellationToken = default)
        {
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _client = new ServiceBusClient(_connectionString);

            try
            {
                _logger.LogInformation("Starting ServiceBus listeners for {Count} queues", _consumerMappings.Count);

                foreach (var mapping in _consumerMappings)
                {
                    try
                    {
                        var processor = _client.CreateProcessor(mapping.QueueName, new ServiceBusProcessorOptions
                        {
                            AutoCompleteMessages = false,
                            MaxConcurrentCalls = 1
                        });

                        processor.ProcessMessageAsync += async args => await ProcessMessageAsync(args, mapping);
                        processor.ProcessErrorAsync += args => ProcessErrorAsync(args, mapping.QueueName);

                        _processors.Add(processor);
                        await processor.StartProcessingAsync(_stoppingCts.Token);
                        _logger.LogInformation("Started listening on queue {QueueName}", mapping.QueueName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error starting listener for queue {QueueName}", mapping.QueueName);
                    }
                }

                // Keep the task running until cancellation is requested
                if (_processors.Count > 0)
                {
                    try
                    {
                        await Task.Delay(Timeout.Infinite, _stoppingCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // This is expected when cancellation occurs
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ServiceBus listener");
                throw;
            }
        }

        public async Task StopListeningAsync(CancellationToken cancellationToken = default)
        {
            if (_stoppingCts != null && !_stoppingCts.IsCancellationRequested)
            {
                _stoppingCts.Cancel();
            }

            foreach (var processor in _processors)
            {
                try
                {
                    await processor.StopProcessingAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping processor for queue");
                }
            }

            _logger.LogInformation("All ServiceBus listeners stopped");
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args, QueueConsumerMapping mapping)
        {
            try
            {
                var body = args.Message.Body.ToString();
                _logger.LogDebug("Received message from queue {QueueName}: {MessageId}", mapping.QueueName, args.Message.MessageId);

                var message = JsonSerializer.Deserialize(body, mapping.MessageType);
                if (message != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var consumer = scope.ServiceProvider.GetRequiredService(mapping.ConsumerType);

                    // Create a typed consumer interface to get the correct method
                    var consumerInterfaceType = typeof(IConsumer<>).MakeGenericType(mapping.MessageType);

                    // Get the method info with its parameters
                    var consumeMethod = consumerInterfaceType.GetMethod(nameof(IConsumer<object>.Consume));
                    if (consumeMethod == null)
                    {
                        throw new InvalidOperationException($"Consume method not found on {consumerInterfaceType.Name}");
                    }

                    // Get the parameters to check their count
                    var parameters = consumeMethod.GetParameters();
                    _logger.LogDebug("Consume method has {ParameterCount} parameters", parameters.Length);

                    // Create the appropriate parameter array based on parameter count
                    object[] invokeParams;
                    if (parameters.Length == 1)
                    {
                        // Standard case - just the message
                        invokeParams = new[] { message };
                    }
                    else if (parameters.Length == 2)
                    {
                        // If it takes 2 parameters, the second might be a cancellation token
                        // Assuming second parameter is CancellationToken
                        invokeParams = new[] { message, CancellationToken.None };
                    }
                    else
                    {
                        throw new InvalidOperationException($"Consume method has unexpected parameter count: {parameters.Length}");
                    }

                    var result = consumeMethod.Invoke(consumer, invokeParams);
                    if (result == null)
                    {
                        throw new InvalidOperationException("Consume method returned null instead of Task");
                    }

                    // Wait for the task to complete
                    await (Task)result;

                    await args.CompleteMessageAsync(args.Message);
                    _logger.LogDebug("Successfully processed message {MessageId}", args.Message.MessageId);
                }
                else
                {
                    _logger.LogWarning("Could not deserialize message from queue {QueueName}: {MessageId}",
                        mapping.QueueName, args.Message.MessageId);
                    await args.DeadLetterMessageAsync(args.Message, "DeserializationFailure", "Message could not be deserialized");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue {QueueName}: {MessageId}",
                    mapping.QueueName, args.Message.MessageId);

                // Send to dead-letter queue instead of silently completing
                try
                {
                    await args.DeadLetterMessageAsync(args.Message, "ProcessingError", ex.Message);
                }
                catch (Exception deadLetterEx)
                {
                    _logger.LogError(deadLetterEx, "Failed to dead-letter message {MessageId}", args.Message.MessageId);
                    // As a fallback, abandon the message to retry later
                    await args.AbandonMessageAsync(args.Message);
                }
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args, string queueName)
        {
            _logger.LogError(args.Exception, "Service Bus error in queue {QueueName}: {ErrorSource}",
                queueName, args.ErrorSource);
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await StopListeningAsync();

                foreach (var processor in _processors)
                {
                    await processor.DisposeAsync();
                }
                _processors.Clear();

                if (_client != null)
                {
                    await _client.DisposeAsync();
                    _client = null;
                }

                _stoppingCts?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing ServiceBusListener");
            }
        }

        private record QueueConsumerMapping(string QueueName, Type MessageType, Type ConsumerType);
    }
}
