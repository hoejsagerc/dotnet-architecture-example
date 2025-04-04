using Example.SharedKernel.Interfaces;
using Example.SharedKernel.Services.NotifierService;

namespace Example.SharedKernel.Models;

public class DomainEventDispatcher
{
    private readonly Publisher _notifier;

    public DomainEventDispatcher(Publisher notifier)
    {
        _notifier = notifier;
    }


    public async Task DispatchEventsAsync(IHasDomainEvents entity,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in entity.DomainEvents)
        {
            await _notifier.Handle(domainEvent, cancellationToken);
        }

        entity.ClearDomainEvents();
    }
}