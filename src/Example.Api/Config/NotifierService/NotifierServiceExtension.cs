using Example.SharedKernel.Services.NotifierService;

namespace Example.Api.Config.NotifierService;

public static class NotifierServiceExtension
{
    public static IServiceCollection AddNotifier(this IServiceCollection services)
    {
        // Find all handler implementations
        var handlerTypes = typeof(Program).Assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                && !t.IsAbstract && !t.IsInterface);

        foreach (var handlerType in handlerTypes)
        {
            // Get all interfaces that match INotificationHandler<T>
            var handlerInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>));

            foreach (var handlerInterface in handlerInterfaces)
            {
                // Register each handler with its corresponding interface
                services.AddTransient(handlerInterface, handlerType);
            }
        }

        // Register the notifier as a singleton to ensure consistent lifetime
        services.AddSingleton<Publisher>();

        return services;
    }
}