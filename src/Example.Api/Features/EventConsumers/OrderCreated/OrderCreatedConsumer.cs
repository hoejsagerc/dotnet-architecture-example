using Example.Api.Features.Products.Domain;
using Example.SharedKernel.Services.MessagingService;

namespace Example.Api.Features.EventConsumers.OrderCreated;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid ProductId,
    int Quantity);

public sealed class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IProductRepository productRepository, ILogger<OrderCreatedConsumer> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task Consume(OrderCreatedEvent message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "OrderCreatedEvent received for order {OrderId} and product {ProductId}",
            message.OrderId, message.ProductId);

        var product = await _productRepository.GetByIdAsync(
            ProductId.Create(message.ProductId), cancellationToken);

        if (product is null)
        {
            _logger.LogWarning(
                "Product {ProductId} not found for order {OrderId}",
                message.ProductId, message.OrderId);

            return;
        }

        product.DecreaseQuantity(message.Quantity);
        await _productRepository.UpdateAsync(product, cancellationToken);
        return;
    }
}
