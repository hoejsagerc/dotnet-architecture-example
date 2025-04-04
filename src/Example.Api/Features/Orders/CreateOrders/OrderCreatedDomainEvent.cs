using Example.Api.Features.EventConsumers.OrderCreated;
using Example.SharedKernel.Interfaces;
using Example.SharedKernel.Services.MessagingService;
using Example.SharedKernel.Services.NotifierService;

namespace Example.Api.Features.Orders.CreateOrders;

public record OrderCreatedDomainEvent(Order order) : IDomainEvent;

public sealed class Handler
    : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly IServiceBusPublisher _publisher;

    public Handler(IServiceBusPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task Handle(OrderCreatedDomainEvent notification,
        CancellationToken cancellationToken = default)
    {
        await _publisher.PublishAsync(
            new OrderCreatedEvent(notification.order.Id.Value,
                notification.order.ProductId.Value,
                notification.order.Quantity),
                cancellationToken);
    }
}
