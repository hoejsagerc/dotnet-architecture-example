using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Example.SharedKernel.Services.MessagingService;

public class ServiceBusPublisher : IServiceBusPublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusPublisher> _logger;
    private readonly Dictionary<Type, string> _messageTypeToQueueMap;

    public ServiceBusPublisher(string connectionString, ILogger<ServiceBusPublisher> logger, Dictionary<Type, string> messageTypeToQueueMap)
    {
        _client = new ServiceBusClient(connectionString);
        _logger = logger;
        _messageTypeToQueueMap = messageTypeToQueueMap;
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
    {
        if (!_messageTypeToQueueMap.TryGetValue(typeof(TMessage), out var queueName))
        {
            throw new InvalidOperationException($"No queue registered for message type {typeof(TMessage).Name}");
        }

        try
        {
            var sender = _client.CreateSender(queueName);
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody);

            await sender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation("Message of type {MessageType} sent to queue {QueueName}", typeof(TMessage).Name, queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message of type {MessageType} to queue {QueueName}", typeof(TMessage).Name, queueName);
            throw;
        }
    }
}
