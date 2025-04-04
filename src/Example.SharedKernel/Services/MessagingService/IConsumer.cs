namespace Example.SharedKernel.Services.MessagingService;


public interface IConsumer<T>
{
    Task Consume(T message, CancellationToken cancellationToken = default);
}