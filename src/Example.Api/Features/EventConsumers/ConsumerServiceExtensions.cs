using Example.Api.Features.EventConsumers.OrderCreated;
using Example.SharedKernel.Services.MessagingService;

namespace Example.Api.Infrastructure.Messaging;

public static class ConsumerServiceExtensions
{
    public static IServiceCollection AddConsumerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<OrderCreatedConsumer>();
        services.AddServiceBus(opt =>
        {
            opt.ConnectionString = configuration.GetConnectionString("ServiceBus")!;

            opt.AddConsumer<OrderCreatedConsumer, OrderCreatedEvent>("orders-created");

            opt.AddPublisher(new Dictionary<Type, string>
            {
                { typeof(OrderCreatedEvent), "orders-created" }
            });
        });
        return services;
    }
}