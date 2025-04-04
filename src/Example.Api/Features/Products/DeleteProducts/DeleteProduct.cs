using ErrorOr;
using Example.Api.Features.Products.Domain;
using Example.SharedKernel.Interfaces;
using FluentValidation;

namespace Example.Api.Features.Products.DeleteProducts;


public record DeleteProductCommand(Guid Id)
{
    public class Validator : AbstractValidator<DeleteProductCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Product Id is required.")
                .Must(x => x != Guid.Empty)
                .WithMessage("Product Id must be a valid GUID.");
        }
    }
}


public sealed class DeleteProductHandler
    : IHandler<DeleteProductCommand, ErrorOr<Deleted>>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            ProductId.Create(request.Id), cancellationToken);
        if (product is null)
        {
            return Error.NotFound("Product.NotFound",
                $"Product with id {request.Id} not found.");
        }


        await _productRepository.DeleteAsync(product, cancellationToken);

        return new Deleted();
    }
}
