using ErrorOr;
using Example.SharedKernel.Interfaces;
using FluentValidation;

namespace Example.Api.Config.RequestPipeline;


public class ValidationPipeline : IPipeline
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ValidationPipeline> _logger;

    public ValidationPipeline(IServiceProvider sp, ILogger<ValidationPipeline> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    public async Task<ErrorOr<TResponse>> InvokeAsync<TRequest, TResponse>(TRequest request,
        Func<TRequest, CancellationToken, Task<ErrorOr<TResponse>>> handler, CancellationToken cancellationToken = default)
    {
        var validator = _sp.GetService(typeof(IValidator<TRequest>)) as IValidator<TRequest>;

        // if there are no validator on the command we can skip the validation
        if (validator is null)
        {
            _logger.LogWarning("No validator found for {RequestType}. Skipping validation.", typeof(TRequest).Name);
            return await handler(request, cancellationToken);
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        // if the validation failed we can return the validation errors
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .ConvertAll(validationFailure => Error.Validation(
                    validationFailure.PropertyName,
                    validationFailure.ErrorMessage));

            return errors;
        }

        // if the validation passed we can call the handler
        var result = await handler(request, cancellationToken);
        return result;
    }
}
