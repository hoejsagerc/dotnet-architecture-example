namespace Example.SharedKernel.Services.NotifierService;

public interface INotificationHandler<T>
{
    Task Handle(T notification, CancellationToken cancellationToken = default);
}