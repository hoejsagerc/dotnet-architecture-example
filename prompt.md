## Context 

I am building a .NET custom minimal version of MassTransit for working with an azure Service bus.
The goals is that i can easily register consumers and publish messages to either topics or queues
in an azure service bus.

I need to have a way of easily registering the consumers for either queues or topics, and then i need
to be able to publish messages to either a queue or a topic.

## Task

You need to help me complete building the library. You need to make a plan of what i need to create 
for actually having a library that i can use.

You are not to think about extra features or what would be nice to have. Only the desired specs.

Whats the plan ? 



### Infrastructure Files

- ServiceBusConnection.cs

Manages connections to Azure Service Bus
Provides session pooling and connection management


- MessageSerializer.cs

Handles serialization/deserialization of messages
Maintains type information for routing


- ServiceCollectionExtensions.cs

Extension methods for dependency injection registration
Makes it easy to configure in startup



### Optional Utility Files

-RetryPolicy.cs

Defines retry behavior for failed operations
Implements exponential backoff or other strategies


- MessageTypeRegistry.cs

Maps string type names to CLR types
Helps with message deserialization and routing


- LoggingExtensions.cs

Logging helpers for consistent logging across the library


## Current Files

````csharp

namespace Example.SharedKernel.Services.MessagingService.Configurations;

/// <summary>
/// Configuration for a specific queue.
/// </summary>
public class QueueConfiguration
{
    /// <summary>
    /// Gets or sets the name of the queue.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets whether the queue should be created if it doesn't exist.
    /// </summary>
    public bool CreateIfNotExists { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum delivery count before messages are sent to the dead-letter queue.
    /// </summary>
    public int MaxDeliveryCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets the default time-to-live for messages in this queue.
    /// </summary>
    public TimeSpan? DefaultMessageTimeToLive { get; set; }

    /// <summary>
    /// Gets or sets whether dead-lettered messages can be reprocessed.
    /// </summary>
    public bool EnableDeadLetteringOnMessageExpiration { get; set; } = true;
}


namespace Example.SharedKernel.Services.MessagingService.Configurations;

/// <summary>
/// Configuration settings for Azure Service Bus connections and behavior.
/// </summary>
public class ServiceBusConfiguration
{
    /// <summary>
    /// Gets or sets the connection string for Azure Service Bus.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the retry options for Service Bus operations.
    /// </summary>
    public RetryOptions RetryOptions { get; set; } = new RetryOptions();

    /// <summary>
    /// Gets or sets the prefetch count for message receivers.
    /// </summary>
    /// <remarks>
    /// Higher values improve throughput but increase memory usage.
    /// </remarks>
    public int PrefetchCount { get; set; } = 20;

    /// <summary>
    /// Gets or sets the maximum number of concurrent calls to process messages.
    /// </summary>
    public int MaxConcurrentCalls { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to auto-complete messages after successful processing.
    /// </summary>
    /// <remarks>
    /// When false, handlers must explicitly complete messages.
    /// </remarks>
    public bool AutoComplete { get; set; } = false;

    /// <summary>
    /// Gets or sets the receive mode for messages.
    /// </summary>
    public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;

    /// <summary>
    /// Gets or sets whether to track message processing time for metrics.
    /// </summary>
    public bool TrackMessageProcessingTime { get; set; } = true;

    /// <summary>
    /// Gets the queue configurations mapped by name.
    /// </summary>
    public Dictionary<string, QueueConfiguration> Queues { get; } = new Dictionary<string, QueueConfiguration>();

    /// <summary>
    /// Gets the topic configurations mapped by name.
    /// </summary>
    public Dictionary<string, TopicConfiguration> Topics { get; } = new Dictionary<string, TopicConfiguration>();

    /// <summary>
    /// Gets or sets the default queue to use when no queue name is specified.
    /// </summary>
    public string? DefaultQueueName { get; set; }

    /// <summary>
    /// Gets or sets the default topic to use when no topic name is specified.
    /// </summary>
    public string? DefaultTopicName { get; set; }

    /// <summary>
    /// Gets or sets the mapping of message types to their default destinations.
    /// </summary>
    public Dictionary<string, MessageDestination> MessageTypeDestinations { get; } = new Dictionary<string, MessageDestination>();
}




/// <summary>
/// Options for retry behavior.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts.
    /// </summary>
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets the maximum delay between retry attempts.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the retry mode.
    /// </summary>
    public RetryMode Mode { get; set; } = RetryMode.Exponential;
}

/// <summary>
/// Defines how retries are calculated.
/// </summary>
public enum RetryMode
{
    /// <summary>
    /// Fixed delay between retries.
    /// </summary>
    Fixed,

    /// <summary>
    /// Exponentially increasing delay between retries.
    /// </summary>
    Exponential
}

/// <summary>
/// Defines the receive mode for messages.
/// </summary>
public enum ReceiveMode
{
    /// <summary>
    /// Messages are not deleted until explicitly completed.
    /// </summary>
    PeekLock,

    /// <summary>
    /// Messages are deleted from the queue as soon as they are received.
    /// </summary>
    ReceiveAndDelete
}

/// <summary>
/// Defines a destination for a message type.
/// </summary>
public class MessageDestination
{
    /// <summary>
    /// Gets or sets the destination type.
    /// </summary>
    public DestinationType Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the destination (queue or topic).
    /// </summary>
    public required string Name { get; set; }
}

/// <summary>
/// Types of message destinations.
/// </summary>
public enum DestinationType
{
    /// <summary>
    /// Message is sent to a queue.
    /// </summary>
    Queue,

    /// <summary>
    /// Message is sent to a topic.
    /// </summary>
    Topic
}


namespace Example.SharedKernel.Services.MessagingService.Configurations;

/// <summary>
/// Configuration for a topic subscription.
/// </summary>
public class SubscriptionConfiguration
{
    /// <summary>
    /// Gets or sets the name of the subscription.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the SQL filter expression for the subscription.
    /// </summary>
    public string? SqlFilter { get; set; }

    /// <summary>
    /// Gets or sets the maximum delivery count before messages are sent to the dead-letter queue.
    /// </summary>
    public int MaxDeliveryCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether dead-lettered messages can be reprocessed.
    /// </summary>
    public bool EnableDeadLetteringOnMessageExpiration { get; set; } = true;
}

namespace Example.SharedKernel.Services.MessagingService.Configurations;

/// <summary>
/// Configuration for a specific topic.
/// </summary>
public class TopicConfiguration
{
    /// <summary>
    /// Gets or sets the name of the topic.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets whether the topic should be created if it doesn't exist.
    /// </summary>
    public bool CreateIfNotExists { get; set; } = false;

    /// <summary>
    /// Gets or sets the default time-to-live for messages in this topic.
    /// </summary>
    public TimeSpan? DefaultMessageTimeToLive { get; set; }

    /// <summary>
    /// Gets or sets whether dead-lettered messages can be reprocessed.
    /// </summary>
    public bool EnableDeadLetteringOnFilterEvaluationExceptions { get; set; } = true;

    /// <summary>
    /// Gets the subscription configurations mapped by name.
    /// </summary>
    public Dictionary<string, SubscriptionConfiguration> Subscriptions { get; } = new Dictionary<string, SubscriptionConfiguration>();
}

namespace Example.SharedKernel.Services.MessagingService.Interfaces;

public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to a queue.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="queue">The name of the queue. If null, uses the default queue for this message type.</param>
    /// <param name="options">Optional publishing options.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishToQueueAsync<TMessage>(
        TMessage message,
        string queue = null!,
        PublishOptions options = null!,
        CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Publishes a message to a topic.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="topic">The name of the topic. If null, uses the default topic for this message type.</param>
    /// <param name="options">Optional publishing options.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishToTopicAsync<TMessage>(
        TMessage message,
        string topic = null!,
        PublishOptions options = null!,
        CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Publishes multiple messages to a queue in a single batch operation.
    /// </summary>
    /// <typeparam name="TMessage">The type of messages to publish.</typeparam>
    /// <param name="messages">The collection of messages to publish.</param>
    /// <param name="queue">The name of the queue. If null, uses the default queue for this message type.</param>
    /// <param name="options">Optional publishing options.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishBatchToQueueAsync<TMessage>(
        IEnumerable<TMessage> messages,
        string queue = null!,
        PublishOptions options = null!,
        CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Publishes multiple messages to a topic in a single batch operation.
    /// </summary>
    /// <typeparam name="TMessage">The type of messages to publish.</typeparam>
    /// <param name="messages">The collection of messages to publish.</param>
    /// <param name="topic">The name of the topic. If null, uses the default topic for this message type.</param>
    /// <param name="options">Optional publishing options.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishBatchToTopicAsync<TMessage>(
        IEnumerable<TMessage> messages,
        string topic = null!,
        PublishOptions options = null!,
        CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Options for message publishing.
    /// </summary>
    public class PublishOptions
    {
        /// <summary>
        /// Gets or sets the time span after which the message will be available for processing.
        /// </summary>
        public TimeSpan? ScheduledEnqueueDelay { get; set; }

        /// <summary>
        /// Gets or sets the time-to-live value for the message.
        /// </summary>
        public TimeSpan? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the session identifier for the message.
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier for the message.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets additional custom properties for the message.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}


using Example.SharedKernel.Services.MessagingService.Models;

namespace Example.SharedKernel.Services.MessagingService.Interfaces;


/// <summary>
/// Defines a handler for processing messages of type <typeparamref name="TMessage"/>.
/// </summary>
/// <typeparam name="TMessage">The type of message to handle.</typeparam>
public interface IMessageHandler<in TMessage>
    where TMessage : class
{
    /// <summary>
    /// Handles a message asynchronously.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="context">The context of the message being processed.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(TMessage message, MessageContext context, CancellationToken cancellationToken = default);
}

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Example.SharedKernel.Services.MessagingService.Models;

/// <summary>
/// Handles serialization and deserialization of messages.
/// </summary>
public class MessageSerializer
{
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageSerializer"/> class.
    /// </summary>
    public MessageSerializer()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Deserializes a message from its binary representation.
    /// </summary>
    /// <param name="data">The binary data to deserialize.</param>
    /// <param name="messageType">The type to deserialize to.</param>
    /// <param name="contentType">The content type of the message.</param>
    /// <returns>The deserialized message object.</returns>
    public object Deserialize(byte[] data, Type messageType, string contentType)
    {
        if (data == null || data.Length == 0)
        {
            throw new ArgumentException("Cannot deserialize null or empty data.", nameof(data));
        }

        if (messageType == null)
        {
            throw new ArgumentNullException(nameof(messageType));
        }

        // Default to JSON if content type is not specified
        contentType = contentType?.ToLowerInvariant() ?? "application/json";

        switch (contentType)
        {
            case "application/json":
            case "text/json":
                return DeserializeJson(data, messageType);

            // Add other content types as needed (XML, etc.)

            default:
                throw new NotSupportedException($"Content type '{contentType}' is not supported.");
        }
    }

    /// <summary>
    /// Serializes a message to its binary representation.
    /// </summary>
    /// <param name="message">The message to serialize.</param>
    /// <param name="contentType">The desired content type.</param>
    /// <returns>The serialized message as a byte array.</returns>
    public byte[] Serialize(object message, string contentType = "application/json")
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        contentType = contentType?.ToLowerInvariant() ?? "application/json";

        switch (contentType)
        {
            case "application/json":
            case "text/json":
                return SerializeJson(message);

            // Add other content types as needed (XML, etc.)

            default:
                throw new NotSupportedException($"Content type '{contentType}' is not supported.");
        }
    }

    private object DeserializeJson(byte[] data, Type type)
    {
        string json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize(json, type, _jsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize {type.Name}");
    }

    private byte[] SerializeJson(object message)
    {
        string json = JsonSerializer.Serialize(message, message.GetType(), _jsonOptions);
        return Encoding.UTF8.GetBytes(json);
    }

    /// <summary>
    /// Deserializes a message from its binary representation to a specific type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="data">The binary data to deserialize.</param>
    /// <param name="contentType">The content type of the message.</param>
    /// <returns>The deserialized message.</returns>
    public T Deserialize<T>(byte[] data, string contentType = "application/json")
    {
        return (T)Deserialize(data, typeof(T), contentType);
    }
}


using Azure.Messaging.ServiceBus;

namespace Example.SharedKernel.Services.MessagingService.Models;

public class MessageContext
{
    private readonly ServiceBusReceivedMessage _receivedMessage;
    private readonly Func<ServiceBusReceivedMessage, CancellationToken, Task> _completeMessageFunc;
    private readonly Func<ServiceBusReceivedMessage, IDictionary<string, object>, CancellationToken, Task> _abandonMessageFunc;
    private readonly Func<ServiceBusReceivedMessage, IDictionary<string, object>, CancellationToken, Task> _deferMessageFunc;
    private readonly Func<ServiceBusReceivedMessage, string, string, CancellationToken, Task> _deadLetterMessageFunc;


    internal MessageContext(
        ServiceBusReceivedMessage receivedMessage,
        Func<ServiceBusReceivedMessage, CancellationToken, Task> completeMessageFunc,
        Func<ServiceBusReceivedMessage, IDictionary<string, object>, CancellationToken, Task> abandonMessageFunc,
        Func<ServiceBusReceivedMessage, IDictionary<string, object>, CancellationToken, Task> deferMessageFunc,
        Func<ServiceBusReceivedMessage, string, string, CancellationToken, Task> deadLetterMessageFunc)
    {
        _receivedMessage = receivedMessage ?? throw new ArgumentNullException(nameof(receivedMessage));
        _completeMessageFunc = completeMessageFunc ?? throw new ArgumentNullException(nameof(completeMessageFunc));
        _abandonMessageFunc = abandonMessageFunc ?? throw new ArgumentNullException(nameof(abandonMessageFunc));
        _deferMessageFunc = deferMessageFunc ?? throw new ArgumentNullException(nameof(deferMessageFunc));
        _deadLetterMessageFunc = deadLetterMessageFunc ?? throw new ArgumentNullException(nameof(deadLetterMessageFunc));

        // Set properties from the received message
        MessageId = receivedMessage.MessageId;
        CorrelationId = receivedMessage.CorrelationId;
        SessionId = receivedMessage.SessionId;
        ContentType = receivedMessage.ContentType;
        DeliveryCount = receivedMessage.DeliveryCount;
        EnqueuedTime = receivedMessage.EnqueuedTime;
        SequenceNumber = receivedMessage.SequenceNumber;
        Subject = receivedMessage.Subject;
        ReplyTo = receivedMessage.ReplyTo;
        Properties = receivedMessage.ApplicationProperties.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value);
    }

    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public string MessageId { get; }

    /// <summary>
    /// Gets the correlation identifier for the message.
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Gets the session identifier for the message.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the content type of the message.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the number of delivery attempts for this message.
    /// </summary>
    public int DeliveryCount { get; }

    /// <summary>
    /// Gets the time the message was enqueued.
    /// </summary>
    public DateTimeOffset EnqueuedTime { get; }

    /// <summary>
    /// Gets the sequence number of the message.
    /// </summary>
    public long SequenceNumber { get; }

    /// <summary>
    /// Gets the subject of the message.
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// Gets the reply-to address of the message.
    /// </summary>
    public string ReplyTo { get; }

    /// <summary>
    /// Gets the custom properties for the message.
    /// </summary>
    public IDictionary<string, object> Properties { get; }

    /// <summary>
    /// Completes the message, indicating successful processing.
    /// </summary>
    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        return _completeMessageFunc(_receivedMessage, cancellationToken);
    }

    /// <summary>
    /// Abandons the message, making it available for immediate reprocessing.
    /// </summary>
    public Task AbandonAsync(
        IDictionary<string, object> propertiesToModify = null!,
        CancellationToken cancellationToken = default)
    {
        return _abandonMessageFunc(_receivedMessage, propertiesToModify, cancellationToken);
    }

    /// <summary>
    /// Defers the message, making it unavailable for regular processing until explicitly retrieved.
    /// </summary>
    public Task DeferAsync(
        IDictionary<string, object> propertiesToModify = null!,
        CancellationToken cancellationToken = default)
    {
        return _deferMessageFunc(_receivedMessage, propertiesToModify, cancellationToken);
    }

    /// <summary>
    /// Dead-letters the message, removing it from the queue and placing it in the dead-letter queue.
    /// </summary>
    public Task DeadLetterAsync(
        string reason = null!,
        string errorDescription = null!,
        CancellationToken cancellationToken = default)
    {
        return _deadLetterMessageFunc(_receivedMessage, reason, errorDescription, cancellationToken);
    }

    /// <summary>
    /// Gets the underlying Service Bus received message.
    /// </summary>
    internal ServiceBusReceivedMessage GetReceivedMessage()
    {
        return _receivedMessage;
    }
}


using System.Collections.Concurrent;
using Example.SharedKernel.Services.MessagingService.Interfaces;

namespace Example.SharedKernel.Services.MessagingService.Models;

/// <summary>
/// Registry for message handlers that maps message types to their handler types.
/// </summary>
public class ConsumerRegistry
{
    private readonly ConcurrentDictionary<Type, Type> _messageHandlerTypes = new ConcurrentDictionary<Type, Type>();
    private readonly ConcurrentDictionary<Type, MessageRegistration> _messageRegistrations = new ConcurrentDictionary<Type, MessageRegistration>();
    private readonly IServiceProvider _serviceProvider;

    public ConsumerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Registers a handler type for a specific message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to handle.</typeparam>
    /// <typeparam name="THandler">The type of handler that processes the message.</typeparam>
    /// <param name="name">The name of the queue or topic to listen on. If null, uses the default from configuration.</param>
    /// <param name="isQueue">Whether the destination is a queue or topic.</param>
    /// <param name="subscriptionName">For topics, the subscription name to listen on.</param>
    /// <returns>The consumer registry for method chaining.</returns>
    public ConsumerRegistry Register<TMessage, THandler>(
        string name = null!,
        bool isQueue = true,
        string subscriptionName = null!)
        where TMessage : class
        where THandler : class, IMessageHandler<TMessage>
    {
        Type messageType = typeof(TMessage);
        Type handlerType = typeof(THandler);

        _messageHandlerTypes[messageType] = handlerType;

        _messageRegistrations[messageType] = new MessageRegistration
        {
            MessageType = messageType,
            HandlerType = handlerType,
            EntityName = name,
            IsQueue = isQueue,
            SubscriptionName = subscriptionName
        };

        return this;
    }

    /// <summary>
    /// Gets all registered message types.
    /// </summary>
    /// <returns>A collection of registered message types.</returns>
    public IEnumerable<Type> GetRegisteredMessageTypes()
    {
        return _messageHandlerTypes.Keys;
    }

    /// <summary>
    /// Gets all message registrations.
    /// </summary>
    /// <returns>A collection of message registrations.</returns>
    public IEnumerable<MessageRegistration> GetMessageRegistrations()
    {
        return _messageRegistrations.Values;
    }

    /// <summary>
    /// Gets the registration for a specific message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message.</typeparam>
    /// <returns>The message registration, or null if not registered.</returns>
    public MessageRegistration? GetRegistrationForMessage<TMessage>() where TMessage : class
    {
        return GetRegistrationForMessage(typeof(TMessage));
    }

    /// <summary>
    /// Gets the registration for a specific message type.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    /// <returns>The message registration, or null if not registered.</returns>
    public MessageRegistration? GetRegistrationForMessage(Type messageType)
    {
        if (messageType == null)
            throw new ArgumentNullException(nameof(messageType));

        return _messageRegistrations.TryGetValue(messageType, out var registration)
            ? registration
            : null;
    }

    /// <summary>
    /// Creates a handler instance for a specific message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to handle.</typeparam>
    /// <returns>An instance of the handler for the message type.</returns>
    public IMessageHandler<TMessage> CreateHandler<TMessage>() where TMessage : class
    {
        Type messageType = typeof(TMessage);

        if (!_messageHandlerTypes.TryGetValue(messageType, out Type? handlerType))
        {
            throw new InvalidOperationException($"No handler registered for message type {messageType.Name}");
        }

        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException(
                $"Failed to resolve handler of type {handlerType.Name}. " +
                "Make sure it is registered with the dependency injection container.");
        }

        return (IMessageHandler<TMessage>)handler;
    }
}

/// <summary>
/// Contains registration information for a message type.
/// </summary>
public class MessageRegistration
{
    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    public required Type MessageType { get; set; }

    /// <summary>
    /// Gets or sets the handler type.
    /// </summary>
    public required Type HandlerType { get; set; }

    /// <summary>
    /// Gets or sets the name of the queue or topic.
    /// </summary>
    public required string EntityName { get; set; }

    /// <summary>
    /// Gets or sets whether the entity is a queue (true) or topic (false).
    /// </summary>
    public bool IsQueue { get; set; }

    /// <summary>
    /// Gets or sets the subscription name for topics.
    /// </summary>
    public string? SubscriptionName { get; set; }
}

using System.Reflection;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Example.SharedKernel.Services.MessagingService.Configurations;
using Example.SharedKernel.Services.MessagingService.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YourNamespace.Messaging
{
    /// <summary>
    /// Hosts Service Bus consumers and manages their lifecycle.
    /// </summary>
    public class ConsumerHost : IHostedService, IDisposable
    {
        private readonly ILogger<ConsumerHost> _logger;
        private readonly ConsumerRegistry _registry;
        private readonly ServiceBusConfiguration _configuration;
        private readonly List<ServiceBusProcessor> _processors = new List<ServiceBusProcessor>();
        private readonly ServiceBusClient _serviceBusClient;
        private readonly MessageSerializer _serializer;
        private bool _disposed;
        private readonly Dictionary<string, (List<MessageRegistration> Registrations, int MaxDeliveryCount)>
            _processorInfo = new Dictionary<string, (List<MessageRegistration>, int)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumerHost"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="registry">The consumer registry.</param>
        /// <param name="configuration">The Service Bus configuration.</param>
        /// <param name="serializer">The message serializer.</param>
        public ConsumerHost(
            ILogger<ConsumerHost> logger,
            ConsumerRegistry registry,
            ServiceBusConfiguration configuration,
            MessageSerializer serializer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            // Create the Service Bus client
            var clientOptions = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = _configuration.RetryOptions.MaxRetryCount,
                    Delay = _configuration.RetryOptions.Delay,
                    MaxDelay = _configuration.RetryOptions.MaxDelay,
                    Mode = _configuration.RetryOptions.Mode == RetryMode.Exponential
                        ? ServiceBusRetryMode.Exponential
                        : ServiceBusRetryMode.Fixed
                }
            };

            _serviceBusClient = new ServiceBusClient(_configuration.ConnectionString, clientOptions);
            _serializer = serializer;
        }

        /// <summary>
        /// Starts the consumer host.
        /// </summary>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Service Bus consumer host...");

            var registrations = _registry.GetMessageRegistrations().ToList();

            if (registrations.Count == 0)
            {
                _logger.LogWarning("No message handlers are registered. The host will start but won't process any messages.");
                return;
            }

            // Group registrations by entity (queue/topic + subscription)
            var entities = registrations
                .GroupBy(r => new { r.EntityName, r.IsQueue, r.SubscriptionName })
                .ToList();

            foreach (var entity in entities)
            {
                string entityName = entity.Key.EntityName;
                bool isQueue = entity.Key.IsQueue;
                string? subscriptionName = entity.Key.SubscriptionName;

                if (isQueue)
                {
                    await SetupQueueProcessorAsync(entityName, entity.ToList(), cancellationToken);
                }
                else
                {
                    await SetupTopicProcessorAsync(entityName, subscriptionName, entity.ToList(), cancellationToken);
                }
            }

            _logger.LogInformation("Service Bus consumer host started successfully.");
        }

        /// <summary>
        /// Stops the consumer host.
        /// </summary>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service Bus consumer host...");

            var stopTasks = _processors.Select(p => p.StopProcessingAsync(cancellationToken)).ToArray();
            await Task.WhenAll(stopTasks);

            _logger.LogInformation("Service Bus consumer host stopped successfully.");
        }

        /// <summary>
        /// Disposes resources used by the consumer host.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources used by the consumer host.
        /// </summary>
        /// <param name="disposing">Whether the method is being called from Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                foreach (var processor in _processors)
                {
                    processor.ProcessMessageAsync -= ProcessMessageAsync;
                    processor.ProcessErrorAsync -= ProcessErrorAsync;
                    processor.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }

                _processors.Clear();
                _serviceBusClient?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }

            _disposed = true;
        }

        private async Task SetupQueueProcessorAsync(
            string? queueName,
            List<MessageRegistration> registrations,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                queueName = _configuration.DefaultQueueName;
                if (string.IsNullOrEmpty(queueName))
                {
                    throw new InvalidOperationException(
                        "Queue name is not specified and no default queue name is configured.");
                }
            }

            _logger.LogInformation("Setting up processor for queue {QueueName}...", queueName);

            // Check if we need to create the queue
            if (_configuration.Queues.TryGetValue(queueName, out var queueConfig) && queueConfig.CreateIfNotExists)
            {
                await EnsureQueueExistsAsync(queueName, queueConfig);
            }

            // Create the processor for this queue
            var processorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = _configuration.MaxConcurrentCalls,
                AutoCompleteMessages = _configuration.AutoComplete,
                PrefetchCount = _configuration.PrefetchCount,
                ReceiveMode = _configuration.ReceiveMode == ReceiveMode.PeekLock
                    ? ServiceBusReceiveMode.PeekLock
                    : ServiceBusReceiveMode.ReceiveAndDelete
            };

            int maxDeliveryCount = 10;
            if (queueConfig is not null)
            {
                maxDeliveryCount = queueConfig.MaxDeliveryCount;
            }
            var processor = _serviceBusClient.CreateProcessor(queueName, processorOptions);
            _processorInfo[processor.Identifier] = (registrations, maxDeliveryCount);

            // Register the handlers
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;

            // Start processing
            await processor.StartProcessingAsync(cancellationToken);

            _processors.Add(processor);

            _logger.LogInformation("Processor for queue {QueueName} started successfully.", queueName);
        }

        private async Task SetupTopicProcessorAsync(
            string? topicName,
            string? subscriptionName,
            List<MessageRegistration> registrations,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(topicName))
            {
                topicName = _configuration.DefaultTopicName;
                if (string.IsNullOrEmpty(topicName))
                {
                    throw new InvalidOperationException(
                        "Topic name is not specified and no default topic name is configured.");
                }
            }

            if (string.IsNullOrEmpty(subscriptionName))
            {
                throw new InvalidOperationException(
                    $"Subscription name is required for topic {topicName}.");
            }

            _logger.LogInformation("Setting up processor for topic {TopicName} and subscription {SubscriptionName}...",
                topicName, subscriptionName);

            // Check if we need to create the topic and subscription
            if (_configuration.Topics.TryGetValue(topicName, out var topicConfig) && topicConfig.CreateIfNotExists)
            {
                await EnsureTopicAndSubscriptionExistAsync(topicName, subscriptionName, topicConfig);
            }

            // Create the processor for this topic subscription
            var processorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = _configuration.MaxConcurrentCalls,
                AutoCompleteMessages = _configuration.AutoComplete,
                PrefetchCount = _configuration.PrefetchCount,
                ReceiveMode = _configuration.ReceiveMode == ReceiveMode.PeekLock
                    ? ServiceBusReceiveMode.PeekLock
                    : ServiceBusReceiveMode.ReceiveAndDelete
            };

            int maxDeliveryCount = 10; // Default
            if (topicConfig is not null &&
                topicConfig.Subscriptions.TryGetValue(subscriptionName, out var subscriptionConfig))
            {
                maxDeliveryCount = subscriptionConfig.MaxDeliveryCount;
            }
            var processor = _serviceBusClient.CreateProcessor(topicName, subscriptionName, processorOptions);
            _processorInfo[processor.Identifier] = (registrations, maxDeliveryCount);

            // Register the handlers
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;

            // Start processing
            await processor.StartProcessingAsync(cancellationToken);

            _processors.Add(processor);

            _logger.LogInformation(
                "Processor for topic {TopicName} and subscription {SubscriptionName} started successfully.",
                topicName, subscriptionName);
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            ServiceBusReceivedMessage receivedMessage = args.Message;
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogDebug("Received message: {MessageId}", receivedMessage.MessageId);

                // Get the content type from the message properties
                string contentType = receivedMessage.ContentType ?? "application/json";

                // Try to determine the message type from the content type or a custom property
                Type? messageType = null;

                if (receivedMessage.ApplicationProperties.TryGetValue("MessageType", out object? messageTypeValue))
                {
                    string? messageTypeName = messageTypeValue.ToString();
                    messageType = Type.GetType(messageTypeName ?? string.Empty);
                }

                // If no type was found by name, try to find a handler based on registrations
                if (messageType == null)
                {
                    var registrations = _processorInfo[args.Identifier];

                    // If only one handler is registered for this queue/topic, use that
                    if (registrations.Registrations.Count == 1)
                    {
                        messageType = registrations.Registrations[0].MessageType;
                    }
                    else
                    {
                        // Try to determine message type from content
                        // This is a simplified approach - in a real implementation, you might need 
                        // more sophisticated type determination logic
                        _logger.LogWarning(
                            "Multiple message handlers registered for this entity and no MessageType property found. " +
                            "Will attempt to determine type from message content.");
                    }
                }

                if (messageType == null)
                {
                    throw new InvalidOperationException("Unable to determine message type.");
                }

                // Deserialize the message
                object message = _serializer.Deserialize(receivedMessage.Body.ToArray(), messageType, contentType);

                // Create message context
                var messageContext = new MessageContext(
                    receivedMessage,
                    args.CompleteMessageAsync,
                    args.AbandonMessageAsync,
                    args.DeferMessageAsync,
                    args.DeadLetterMessageAsync);

                // Create and invoke the appropriate handler
                await InvokeHandlerAsync(message, messageType, messageContext, args);

                // If auto-complete is disabled, we need to complete the message here
                if (!_configuration.AutoComplete)
                {
                    await args.CompleteMessageAsync(receivedMessage);
                }

                if (_configuration.TrackMessageProcessingTime)
                {
                    var processingTime = DateTime.UtcNow - startTime;
                    _logger.LogInformation(
                        "Processed message {MessageId} in {ProcessingTimeMs}ms",
                        receivedMessage.MessageId,
                        processingTime.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}", receivedMessage.MessageId);

                // Don't complete the message so it can be retried
                // Consider implementing a retry policy with dead-lettering after X attempts
                if (!_configuration.AutoComplete)
                {
                    // Check delivery count to see if we should dead-letter
                    int maxDeliveryCount = 10; // Default

                    if (_processorInfo.TryGetValue(args.Identifier, out var info))
                    {
                        maxDeliveryCount = info.MaxDeliveryCount;
                    }

                    if (receivedMessage.DeliveryCount >= maxDeliveryCount)
                    {
                        _logger.LogWarning(
                            "Message {MessageId} has reached the maximum delivery count of {MaxDeliveryCount}. " +
                            "Moving to dead-letter queue.",
                            receivedMessage.MessageId,
                            maxDeliveryCount);

                        await args.DeadLetterMessageAsync(
                            receivedMessage,
                            "MaxDeliveryCountExceeded",
                            ex.Message);
                    }
                    else
                    {
                        // Otherwise, abandon the message to retry
                        await args.AbandonMessageAsync(receivedMessage);
                    }
                }

                // Re-throw to let the Service Bus SDK handle the exception
                throw;
            }
        }

        private async Task InvokeHandlerAsync(
            object message,
            Type messageType,
            MessageContext context,
            ProcessMessageEventArgs args)
        {
            // Create the handler through the registry
            MethodInfo? handlerMethod = typeof(ConsumerRegistry)
                .GetMethod(nameof(ConsumerRegistry.CreateHandler))?
                .MakeGenericMethod(messageType);

            if (handlerMethod is null)
            {
                throw new InvalidOperationException($"Handler method not found for message type {messageType.Name}");
            }

            dynamic handler = handlerMethod.Invoke(_registry, Array.Empty<object>())!;

            // Invoke the handler
            await handler.HandleAsync((dynamic)message, context, args.CancellationToken);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error in message processing: {ErrorSource}, {EntityPath}",
                args.ErrorSource, args.EntityPath);

            return Task.CompletedTask;
        }

        private async Task EnsureQueueExistsAsync(string queueName, QueueConfiguration queueConfig)
        {
            try
            {
                var adminClient = new ServiceBusAdministrationClient(_configuration.ConnectionString);

                if (!await adminClient.QueueExistsAsync(queueName))
                {
                    _logger.LogInformation("Creating queue {QueueName} as it doesn't exist...", queueName);

                    var options = new CreateQueueOptions(queueName)
                    {
                        MaxDeliveryCount = queueConfig.MaxDeliveryCount,
                        DeadLetteringOnMessageExpiration = queueConfig.EnableDeadLetteringOnMessageExpiration
                    };

                    if (queueConfig.DefaultMessageTimeToLive.HasValue)
                    {
                        options.DefaultMessageTimeToLive = queueConfig.DefaultMessageTimeToLive.Value;
                    }

                    await adminClient.CreateQueueAsync(options);

                    _logger.LogInformation("Queue {QueueName} created successfully.", queueName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring queue {QueueName} exists", queueName);
                throw;
            }
        }

        private async Task EnsureTopicAndSubscriptionExistAsync(
            string topicName,
            string subscriptionName,
            TopicConfiguration topicConfig)
        {
            try
            {
                var adminClient = new ServiceBusAdministrationClient(_configuration.ConnectionString);

                // Ensure topic exists
                if (!await adminClient.TopicExistsAsync(topicName))
                {
                    _logger.LogInformation("Creating topic {TopicName} as it doesn't exist...", topicName);

                    var options = new CreateTopicOptions(topicName)
                    {
                        EnableBatchedOperations = true
                    };

                    if (topicConfig.DefaultMessageTimeToLive.HasValue)
                    {
                        options.DefaultMessageTimeToLive = topicConfig.DefaultMessageTimeToLive.Value;
                    }

                    await adminClient.CreateTopicAsync(options);

                    _logger.LogInformation("Topic {TopicName} created successfully.", topicName);
                }

                // Ensure subscription exists
                if (!await adminClient.SubscriptionExistsAsync(topicName, subscriptionName))
                {
                    _logger.LogInformation(
                        "Creating subscription {SubscriptionName} for topic {TopicName} as it doesn't exist...",
                        subscriptionName, topicName);

                    SubscriptionConfiguration? subscriptionConfig = null!;
                    if (topicConfig.Subscriptions.TryGetValue(subscriptionName, out subscriptionConfig))
                    {
                        var options = new CreateSubscriptionOptions(topicName, subscriptionName)
                        {
                            MaxDeliveryCount = subscriptionConfig.MaxDeliveryCount,
                            DeadLetteringOnMessageExpiration = subscriptionConfig.EnableDeadLetteringOnMessageExpiration
                        };

                        await adminClient.CreateSubscriptionAsync(options);

                        // Add filter if specified
                        if (!string.IsNullOrEmpty(subscriptionConfig.SqlFilter))
                        {
                            var rule = new CreateRuleOptions("DefaultFilter",
                                new SqlRuleFilter(subscriptionConfig.SqlFilter));

                            await adminClient.CreateRuleAsync(topicName, subscriptionName, rule);
                        }
                    }
                    else
                    {
                        // Create with default settings
                        await adminClient.CreateSubscriptionAsync(topicName, subscriptionName);
                    }

                    _logger.LogInformation(
                        "Subscription {SubscriptionName} for topic {TopicName} created successfully.",
                        subscriptionName, topicName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error ensuring topic {TopicName} and subscription {SubscriptionName} exist",
                    topicName, subscriptionName);
                throw;
            }
        }
    }
}
```