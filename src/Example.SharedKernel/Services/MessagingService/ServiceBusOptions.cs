namespace Example.SharedKernel.Services.MessagingService
{
    public class ServiceBusOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        internal List<Action<ServiceBusListener>> ConsumerRegistrations { get; } = new();
        internal Dictionary<Type, string> MessageTypeToQueueMap { get; } = new();

        public void AddConsumer<TConsumer, TMessage>(string queueName)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
        {
            ConsumerRegistrations.Add(listener => listener.RegisterConsumer<TMessage, TConsumer>(queueName));
            MessageTypeToQueueMap[typeof(TMessage)] = queueName;
        }

        public void AddPublisher(Dictionary<Type, string> messageTypeToQueueMap)
        {
            foreach (var mapping in messageTypeToQueueMap)
            {
                MessageTypeToQueueMap[mapping.Key] = mapping.Value;
            }
        }
    }
}
