using Dapper;
using Example.Api.Infrastructure.Persistence;
using Example.SharedKernel.Models;
using Example.SharedKernel.Services.NotifierService;

namespace Example.Api.Features.Products.Domain;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly Publisher _notifier;

    public ProductRepository(IDbConnectionFactory connectionFactory, Publisher notifier)
    {
        _connectionFactory = connectionFactory;
        _notifier = notifier;
    }

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var sql = """
            SELECT * FROM "Products" WHERE "Id" = @Id
            """;

        var productData = await connection.QuerySingleOrDefaultAsync<ProductDto>(
            sql,
            new { Id = id.Value });

        if (productData == null)
            return null;

        return MapToEntity(productData);
    }

    public async Task<PagedList<Product>> GetAsync(CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var countSql = """
            SELECT COUNT(*) FROM "Products"
            """;

        var pageSql = """
            SELECT * FROM "Products" 
            ORDER BY "Id" 
            LIMIT @PageSize OFFSET @Offset
            """;

        var offset = (pageNumber - 1) * pageSize;

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var products = (await connection.QueryAsync<ProductDto>(
            pageSql,
            new { PageSize = pageSize, Offset = offset }))
            .Select(MapToEntity)
            .ToList();

        return new PagedList<Product>(
            items: products,
            page: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount,
            queryCount: totalCount
        );
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var sql = """
            INSERT INTO "Products" ("Id", "Name", "Description", "Price", "ImageUrl", "Quantity")
            VALUES (@Id, @Name, @Description, @Price, @ImageUrl, @Quantity)
            RETURNING *
            """;

        var productData = await connection.QuerySingleAsync<ProductDto>(
            sql,
            new
            {
                Id = product.Id.Value,
                product.Name,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.Quantity
            });

        await _notifier.PublishDomainEventsAsync(product);

        return MapToEntity(productData);
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var sql = """
            UPDATE "Products"
            SET "Name" = @Name,
                "Description" = @Description,
                "Price" = @Price,
                "ImageUrl" = @ImageUrl,
                "Quantity" = @Quantity
            WHERE "Id" = @Id
            RETURNING *
            """;

        var productData = await connection.QuerySingleAsync<ProductDto>(
            sql,
            new
            {
                Id = product.Id.Value,
                product.Name,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.Quantity
            });

        return MapToEntity(productData);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var sql = """
            DELETE FROM "Products" WHERE "Id" = @Id
            """;

        await connection.ExecuteAsync(
            sql,
            new { Id = product.Id.Value });
    }

    // DTO for Dapper mapping
    private class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    // Map from DTO to entity
    private static Product MapToEntity(ProductDto dto)
    {
        var id = ProductId.Create(dto.Id);
        return new Product(
            id,
            dto.Name,
            dto.Description,
            dto.Price,
            dto.ImageUrl,
            dto.Quantity);
    }
}

public interface IProductRepository
{
    /// <summary>
    /// Get a product by its ID.
    /// </summary>
    /// <param name="id">The id of the product <see cref="ProductId"/></param>
    /// <param name="cancellationToken">The stopping token for canaling the process</param>
    /// <returns>Nullable <see cref="Product"/></returns>
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken);

    /// <summary>
    /// Get a paginated list of products.
    /// </summary>
    /// <param name="cancellationToken">The stopping token for canaling the process</param>
    /// <param name="pageNumber">The page number of the selected data</param>
    /// <param name="pageSize">The number of entities in each pag</param>
    /// <returns><see cref="PagedList<Product>"/></returns>
    Task<PagedList<Product>> GetAsync(CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Add a new product to the database.
    /// </summary>
    /// <param name="product">The <see cref="Product"/> to be added</param>
    /// <param name="cancellationToken">The stopping token for canaling the process</param>
    /// <returns><see cref="Product"/></returns>
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing product in the database.
    /// </summary>
    /// <param name="product"><see cref="Product"/> to be updated</param>
    /// <param name="cancellationToken">The stopping token for canaling the process</param>
    /// <returns></returns>
    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a product from the database.
    /// </summary>
    /// <param name="product"><see cref="Product"/> to be deleted</param>
    /// <param name="cancellationToken">The stopping token for canaling the process</param>
    /// <returns></returns>
    Task DeleteAsync(Product product, CancellationToken cancellationToken);
}