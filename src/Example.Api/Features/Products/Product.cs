using Example.Api.Features.Products.CreateProduct;
using Example.Api.Features.Products.UpdateProducts;
using Example.SharedKernel.Models;

namespace Example.Api.Features.Products.Domain;

public sealed class ProductId : AggregateRootId<Guid>
{
    private ProductId(Guid value) : base(value)
    {
    }

    public static ProductId CreateUnique()
    {
        return new ProductId(Guid.NewGuid());
    }

    public static ProductId Create(Guid value)
    {
        return new ProductId(value);
    }
}

public sealed class Product : AggregateRoot<ProductId, Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string ImageUrl { get; private set; }
    public int Quantity { get; private set; }


    public Product(
        ProductId id,
        string name,
        string description,
        decimal price,
        string imageUrl,
        int quantity) : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        Quantity = quantity;
    }


    public static Product Create(
        string name,
        string description,
        decimal price,
        string imageUrl,
        int quantity,
        ProductId? productId = null)
    {
        ProductId id;
        if (productId is null)
        {
            id = ProductId.CreateUnique();
        }
        else
        {
            id = productId;
        }

        var product = new Product(id, name, description, price, imageUrl, quantity);
        product.AddDomainEvent(new ProductCreatedDomainEvent(product));

        return product;
    }


    public void Update(UpdateProductCommand command)
    {
        Name = command.Name;
        Description = command.Description;
        Price = command.Price;
        ImageUrl = command.ImageUrl;
        Quantity = command.Quantity;
    }

    public void DecreaseQuantity(int quantity)
    {
        if (quantity > Quantity)
        {
            throw new InvalidOperationException("Cannot decrease quantity below zero.");
        }

        Quantity -= quantity;
    }
}


