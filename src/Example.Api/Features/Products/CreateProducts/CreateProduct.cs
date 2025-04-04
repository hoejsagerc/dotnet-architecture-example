using ErrorOr;
using Example.Api.Features.Products.Domain;
using Example.SharedKernel.Interfaces;
using FluentValidation;

namespace Example.Api.Features.Products.CreateProducts;

public record CreateProductCommand(
    string Name, decimal Price, string ImageUrl, int Quantity, string Description) : IRequestCommand
{
    public string SourceIpAddress { get; set; } = string.Empty;

    public class Validator : AbstractValidator<CreateProductCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Product name is required.");

            RuleFor(x => x.Description)
                .MaximumLength(200)
                .WithMessage("Product description must be less than 200 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Product price must be greater than 0.");

            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("Product image URL is required.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Product quantity must be greater than or equal to 0.");
        }
    }
}


public sealed class CreateProductHandler
    : IHandler<CreateProductCommand, ErrorOr<Product>>
{
    private readonly IProductRepository _productRepository;

    public CreateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ErrorOr<Product>> Handle(CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.Quantity);


        var result = await _productRepository.AddAsync(product, cancellationToken);
        return result;
    }
}