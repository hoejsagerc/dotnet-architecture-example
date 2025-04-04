namespace Example.Api.Config.ProblemDetails;

public static class ProblemDetailsServiceExtension
{
    public static IServiceCollection AddExtendedProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var requestId = context.HttpContext?.TraceIdentifier;
                if (requestId != null)
                {
                    context.ProblemDetails.Extensions["traceId"] = requestId;
                }

                if (context.HttpContext?.Request != null)
                {
                    context.ProblemDetails.Extensions["method"] = context.HttpContext.Request.Method;
                    context.ProblemDetails.Extensions["path"] = context.HttpContext.Request.Path;
                }

                if (context.ProblemDetails.Status == StatusCodes.Status500InternalServerError)
                {
                    context.ProblemDetails.Extensions["error"] = "An unexpected error occurred.";
                }
            };
        });

        return services;
    }
}