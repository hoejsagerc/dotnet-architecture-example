using ErrorOr;
using Example.Api.Features.Products.Domain;
using Example.SharedKernel.Interfaces;
using FluentValidation;

namespace Example.Api.Features.Products.GetProductByIds;

public record GetProductByIdQuery(Guid Id)
{
    public class Validator : AbstractValidator<GetProductByIdQuery>
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


public sealed class GetProductByIdHandler
    : IHandler<GetProductByIdQuery, ErrorOr<Product?>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }


    public async Task<ErrorOr<Product?>> Handle(GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository
            .GetByIdAsync(ProductId.Create(request.Id), cancellationToken);

        return product;
    }
}