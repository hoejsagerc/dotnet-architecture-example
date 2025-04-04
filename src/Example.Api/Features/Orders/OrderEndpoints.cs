using Asp.Versioning;
using Example.Api.Config.ProblemDetails;
using Example.Api.Config.RequestPipeline;
using Example.Api.Features.Orders.CreateOrders;
using Example.Api.Features.Orders.GetOrderByIds;

namespace Example.Api.Features.Orders;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersion = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var endpoints = app.MapGroup("/api/v{apiVersion:apiVersion}/orders")
            .WithApiVersionSet(apiVersion)
            .WithOpenApi()
            .WithTags("Orders");

        endpoints.MapPost("/", AddOrder)
            .WithName("CreateOrder")
            .WithSummary("Create a new order")
            .Accepts<CreateOrderCommand>("application/json")
            .Produces<Order>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapGet("/{id:guid}", GetOrderById)
            .WithName("GetOrderById")
            .WithSummary("Get an order by ID")
            .Produces<Order>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    static async Task<IResult> GetOrderById(Guid id,
        GetOrderByIdHandler handler, CancellationToken cancellationToken, RequestPipeline pipe)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await pipe.InvokeAsync(query, handler.Handle, cancellationToken);

        return result.Match(
            order => Results.Ok(order),
            errors => Problem.Response(errors));
    }

    static async Task<IResult> AddOrder(CreateOrderCommand command,
        CreateOrderHandler handler, CancellationToken cancellationToken, RequestPipeline pipe)
    {
        var result = await pipe.InvokeAsync(command, handler.Handle, cancellationToken);

        return result.Match(
            order => Results.CreatedAtRoute("GetOrderById", new { id = order.Id.Value }, order),
            errors => Problem.Response(errors));
    }
}