using ErrorOr;
using Example.Api.Features.Products.Domain;
using Example.SharedKernel.Interfaces;
using FluentValidation;

namespace Example.Api.Features.Orders.CreateOrders;

public record CreateOrderCommand(
    Guid ProductId,
    int Quantity,
    CreateCustomerCommand Customer,
    CreateAddressCommand ShippingAddress)
{

    public class Validator : AbstractValidator<CreateOrderCommand>
    {
        public Validator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.");
        }
    }
}

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email)
{
    public class Validator : AbstractValidator<CreateCustomerCommand>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.");

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Invalid email address.");
        }
    }
}


public record CreateAddressCommand(
    string Street,
    string City,
    string State,
    string ZipCode)
{
    public class Validator : AbstractValidator<CreateAddressCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Street)
                .NotEmpty()
                .WithMessage("Street is required.");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("City is required.");

            RuleFor(x => x.State)
                .NotEmpty()
                .WithMessage("State is required.");

            RuleFor(x => x.ZipCode)
                .NotEmpty()
                .WithMessage("Zip code is required.");
        }
    }
}


public sealed class CreateOrderHandler
    : IHandler<CreateOrderCommand, ErrorOr<Order>>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ErrorOr<Order>> Handle(CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = Order.Create(
            ProductId.Create(request.ProductId),
            request.Quantity,
            Customer.Create(
                request.Customer.FirstName,
                request.Customer.LastName,
                request.Customer.Email),
            Address.Create(
                request.ShippingAddress.Street,
                request.ShippingAddress.City,
                request.ShippingAddress.State,
                request.ShippingAddress.ZipCode
            ));
        await _orderRepository.AddAsync(order, cancellationToken);
        return order;
    }
}
