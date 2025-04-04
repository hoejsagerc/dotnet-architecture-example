using System.Diagnostics;
using ErrorOr;
using Example.SharedKernel.Interfaces;

namespace Example.Api.Config.RequestPipeline;

public class RequestPipeline : IPipeline
{
    private readonly ValidationPipeline _validationPipeline;
    private readonly ILogger<RequestPipeline> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestPipeline(ValidationPipeline validationPipeline,
        ILogger<RequestPipeline> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _validationPipeline = validationPipeline;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ErrorOr<TResponse>> InvokeAsync<TRequest, TResponse>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<ErrorOr<TResponse>>> handler,
        CancellationToken cancellationToken = default)
    {
        var sourceIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var traceId = _httpContextAccessor.HttpContext?.TraceIdentifier;

        if (request is IRequestCommand requestCommand)
        {
            requestCommand.SourceIpAddress = sourceIp ?? string.Empty;
        }

        _logger.LogInformation("Started processing {RequestType} with request: {Request}",
            typeof(TRequest).Name, request);

        var stopwatch = Stopwatch.StartNew();

        var result = await _validationPipeline.InvokeAsync(request, handler, cancellationToken);

        stopwatch.Stop();

        if (result.IsError)
        {
            _logger.LogWarning("{RequestType} failed with errors: {Errors} in {ElapsedMilliseconds}ms with {Request}",
                typeof(TRequest).Name, result.Errors, stopwatch.ElapsedMilliseconds, request);
        }
        else
        {
            _logger.LogInformation("{RequestType} completed successfully in {ElapsedMilliseconds}ms with {Request}",
                typeof(TRequest).Name, stopwatch.ElapsedMilliseconds, request);
        }

        return result;
    }
}