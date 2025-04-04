using Example.SharedKernel.Services.NotifierService;

namespace Example.Api.Features.Orders;


public class OrderRepository : IOrderRepository
{
    private readonly Publisher _notifier;

    public OrderRepository(Publisher notifier)
    {
        _notifier = notifier;
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken)
    {
        // simulate adding the order to a database

        await _notifier.PublishDomainEventsAsync(order);
        return order;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}