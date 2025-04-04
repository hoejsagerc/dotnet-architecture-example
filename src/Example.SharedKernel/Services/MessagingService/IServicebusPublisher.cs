namespace Example.SharedKernel.Services.MessagingService;

public interface IServiceBusPublisher
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : class;
}