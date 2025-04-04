using ErrorOr;
using Example.Api.Features.Products.Domain;
using Example.SharedKernel.Interfaces;
using FluentValidation;

namespace Example.Api.Features.Products.GetProductByIds;

public record GetProductByIdQueryV2beta(Guid Id)
{
    public class Validator : AbstractValidator<GetProductByIdQueryV2beta>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required")
                .Must(x => x != Guid.Empty)
                .WithMessage("Id must be a valid GUID");
        }
    }
}


public sealed class GetProductByIdHandlerV2beta
    : IHandler<GetProductByIdQueryV2beta, ErrorOr<Product?>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdHandlerV2beta(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }


    public async Task<ErrorOr<Product?>> Handle(GetProductByIdQueryV2beta request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository
            .GetByIdAsync(ProductId.Create(request.Id), cancellationToken);

        return product;
    }
}