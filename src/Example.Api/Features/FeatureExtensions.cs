using Example.Api.Features.Orders;
using Example.Api.Features.Products.Domain;

namespace Example.Api.Features;

public static class FeatureExtensions
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        services.AddProductFeature();
        services.AddOrdersFeature();
        return services;
    }

    public static IServiceCollection AddProductFeature(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }

    public static IServiceCollection AddOrdersFeature(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }
}