namespace Example.SharedKernel.Interfaces;

public interface IHandler<TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}