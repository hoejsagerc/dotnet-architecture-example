using Example.Api.Features.Orders.CreateOrders;
using Example.Api.Features.Products.Domain;
using Example.SharedKernel.Models;

namespace Example.Api.Features.Orders;

public sealed class OrderId : AggregateRootId<Guid>
{
    private OrderId(Guid value) : base(value)
    {
    }

    public static OrderId CreateUnique()
    {
        return new OrderId(Guid.NewGuid());
    }

    public static OrderId Create(Guid value)
    {
        return new OrderId(value);
    }
}


public sealed class Order : AggregateRoot<OrderId, Guid>
{
    public ProductId ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Customer Customer { get; private set; }
    public Address ShippingAddress { get; private set; }


    public Order(
        OrderId id,
        ProductId productId,
        int quantity,
        Customer customer,
        Address shippingAddress) : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        Customer = customer;
        ShippingAddress = shippingAddress;
    }

    public static Order Create(
        ProductId productId,
        int quantity,
        Customer customer,
        Address shippingAddress,
        OrderId? orderId = null)
    {
        OrderId id;
        if (orderId is null)
        {
            id = OrderId.CreateUnique();
        }
        else
        {
            id = orderId;
        }

        var order = new Order(id, productId, quantity, customer, shippingAddress);
        order.AddDomainEvent(new OrderCreatedDomainEvent(order));
        return order;
    }
}