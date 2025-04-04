using ErrorOr;
using Example.Api.Features.Products.Domain;
using Example.SharedKernel.Interfaces;
using FluentValidation;

namespace Example.Api.Features.Products.UpdateProducts;

public record UpdateProductCommand(Guid Id, string Name, string Description,
    decimal Price, string ImageUrl, int Quantity)
{
    public class Validator : AbstractValidator<UpdateProductCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required")
                .Must(x => x != Guid.Empty)
                .WithMessage("Id must be a valid GUID");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .MaximumLength(100)
                .WithMessage("Name must be less than 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .MaximumLength(500)
                .WithMessage("Description must be less than 500 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0");

            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("ImageUrl is required")
                .Must(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
                .WithMessage("ImageUrl must be a valid URL");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity must be greater than or equal to 0");
        }
    }
}


public sealed class UpdateProductHandler
    : IHandler<UpdateProductCommand, ErrorOr<Product>>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ErrorOr<Product>> Handle(UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
                ProductId.Create(request.Id), cancellationToken);
        if (product is null)
        {
            return Error.NotFound("Product.NotFound",
                $"Product with id {request.Id} not found");
        }

        product.Update(request);

        await _productRepository.UpdateAsync(product, cancellationToken);

        return product;
    }
}
