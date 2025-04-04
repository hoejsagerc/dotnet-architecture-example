using ErrorOr;
using Example.SharedKernel.Interfaces;
using FluentValidation;

namespace Example.Api.Features.Orders.GetOrderByIds;

public record GetOrderByIdQuery(Guid id)
{
    public class Validator : AbstractValidator<GetOrderByIdQuery>
    {
        public Validator()
        {
            RuleFor(x => x.id)
                .NotEmpty()
                .WithMessage("Id is required")
                .Must(x => x != Guid.Empty)
                .WithMessage("Id must be a valid GUID");
        }
    }
}


public sealed class GetOrderByIdHandler : IHandler<GetOrderByIdQuery, ErrorOr<Order?>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ErrorOr<Order?>> Handle(GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .GetByIdAsync(request.id, cancellationToken);

        return order;
    }
}