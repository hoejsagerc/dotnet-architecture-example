using Asp.Versioning;
using Example.Api.Config.ProblemDetails;
using Example.Api.Config.RequestPipeline;
using Example.Api.Features.Products.CreateProducts;
using Example.Api.Features.Products.DeleteProducts;
using Example.Api.Features.Products.Domain;
using Example.Api.Features.Products.GetProductByIds;
using Example.Api.Features.Products.UpdateProducts;

namespace Example.Api.Features.Products;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersion = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(2, 0, "beta"))
            .ReportApiVersions()
            .Build();

        var endpoints = app.MapGroup("/api/v{apiVersion:apiVersion}/products")
            .WithApiVersionSet(apiVersion)
            .WithOpenApi()
            .WithTags("Products");

        endpoints.MapGet("/{id:guid}", GetProductById)
            .WithName("GetProuctById")
            .WithSummary("Get a product by id")
            .Produces<Product>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapGet("/{id:guid}", GetProductByIdV2beta)
            .WithName("GetProuctByIdV2beta")
            .WithSummary("Get a product by id")
            .MapToApiVersion(new ApiVersion(2, 0, "beta"))
            .Produces<Product>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/", AddProduct)
            .WithName("AddProduct")
            .WithSummary("Add a new product")
            .Accepts<CreateProductCommand>("application/json")
            .Produces<Product>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPut("/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Update a product")
            .Accepts<UpdateProductCommand>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapDelete("/{id:guid}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }


    static async Task<IResult> GetProductById(Guid id,
        GetProductByIdHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetProductByIdQuery(id), cancellationToken);

        return result.Match<IResult>(
            success => success is null ? TypedResults.NotFound() : TypedResults.Ok(success),
            errors => Problem.Response(errors)
        );
    }

    static async Task<IResult> GetProductByIdV2beta(Guid id,
        GetProductByIdHandlerV2beta handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetProductByIdQueryV2beta(id), cancellationToken);
        return result.Match<IResult>(
            success => success is null ? TypedResults.NotFound() : TypedResults.Ok(success),
            errors => Problem.Response(errors)
        );
    }

    static async Task<IResult> AddProduct(CreateProductCommand command,
        CreateProductHandler handler, CancellationToken cancellationToken, RequestPipeline pipe)
    {
        var result = await pipe.InvokeAsync(command, handler.Handle, cancellationToken);

        return result.Match<IResult>(
            success => TypedResults.Created($"/api/v1/products/{success.Id}", success),
            errors => Problem.Response(errors)
        );
    }

    static async Task<IResult> UpdateProduct(Guid id, UpdateProductCommand command,
        UpdateProductHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(command with { Id = id }, cancellationToken);

        return result.Match<IResult>(
            success => TypedResults.NoContent(),
            errors => Problem.Response(errors)
        );
    }

    static async Task<IResult> DeleteProduct(Guid id,
        DeleteProductHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new DeleteProductCommand(id), cancellationToken);

        return result.Match<IResult>(
            success => TypedResults.NoContent(),
            errors => Problem.Response(errors)
        );
    }
}