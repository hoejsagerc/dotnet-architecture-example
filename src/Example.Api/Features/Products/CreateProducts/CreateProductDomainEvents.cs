using Example.Api.Features.Products.Domain;
using Example.SharedKernel.Interfaces;
using Example.SharedKernel.Services.NotifierService;

namespace Example.Api.Features.Products.CreateProduct;

public record ProductCreatedDomainEvent(Product Product) : IDomainEvent;

public sealed class Handler1
    : INotificationHandler<ProductCreatedDomainEvent>
{
    private readonly ILogger<Handler1> _logger;

    public Handler1(ILogger<Handler1> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductCreatedDomainEvent notification,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Product {ProductId} created with name {ProductName}",
            notification.Product.Id,
            notification.Product.Name);

        await Task.CompletedTask;
    }
}
