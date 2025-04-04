using Example.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Example.SharedKernel.Services.NotifierService;


public sealed class Publisher
{
    private readonly IServiceProvider _sp;

    public Publisher(IServiceProvider sp)
    {
        _sp = sp;
    }


    public async Task PublishDomainEventsAsync(IHasDomainEvents entity, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in entity.DomainEvents)
        {
            await Handle(domainEvent, cancellationToken);
        }

        entity.ClearDomainEvents();
    }

    public async Task Handle<T>(T notification, CancellationToken cancellationToken = default)
    {
        if (notification is null)
        {
            return;
        }

        Type concreteType = notification.GetType();
        Type handlerType = typeof(INotificationHandler<>).MakeGenericType(concreteType);

        var handlers = _sp.GetServices(handlerType);

        if (handlers.Any())
        {
            var method = handlerType.GetMethod("Handle");

            await Task.WhenAll(handlers.Select(handler =>
            {
                return (Task)method?.Invoke(handler, new object[] { notification, cancellationToken })!;
            }));
        }
    }
}