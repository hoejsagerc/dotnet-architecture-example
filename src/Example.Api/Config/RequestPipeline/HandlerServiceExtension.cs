using System.Reflection;
using Example.SharedKernel.Interfaces;
using FluentValidation;

namespace Example.Api.Config.RequestPipeline;

public static class HandlerServiceExtension
{
    public static IServiceCollection AddApiHandlers(this IServiceCollection services)
    {
        var apiAssembly = Assembly.GetExecutingAssembly();
        // Register FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<Program>();

        // add all the api handlers to the service collection
        var handlerTypes = apiAssembly.GetTypes()
            .Where(type => !type.IsAbstract &&
                !type.IsInterface &&
                type.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IHandler<,>)));

        foreach (var handler in handlerTypes)
        {
            services.AddTransient(handler);
        }


        // add all the pipelines to the service collection
        var pipes = apiAssembly.GetTypes()
            .Where(type => !type.IsAbstract &&
                !type.IsInterface &&
                typeof(IPipeline).IsAssignableFrom(type));

        foreach (var pipe in pipes)
        {
            services.AddTransient(pipe);
        }


        return services;
    }
}