using Dapper;
using KARacter.WarehouseTest.Domain.Entities;
using Microsoft.Extensions.Logging;
using KARacter.WarehouseTest.Persistence.Repositories.Queries;
using KARacter.WarehouseTest.Application.Common.Interfaces.Factories;
using KARacter.WarehouseTest.Application.Common.Interfaces.Repositories;

namespace KARacter.WarehouseTest.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(ISqlConnectionFactory connectionFactory, ILogger<ProductRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting product with SKU: {SKU}", sku);
            using var connection = _connectionFactory.CreateConnection();
            var product = await connection.QuerySingleOrDefaultAsync<Product>(ProductQueries.GetBySkuQuery, new { SKU = sku });

            if (product == null)
                _logger.LogInformation("Product with SKU: {SKU} not found", sku);
            else
                _logger.LogDebug("Successfully retrieved product with SKU: {SKU}", sku);

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product with SKU: {SKU}", sku);
            throw;
        }
    }

    public async Task<ProductDetails?> GetDetailsBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting product details for SKU: {SKU}", sku);
            using var connection = _connectionFactory.CreateConnection();
            var details = await connection.QuerySingleOrDefaultAsync<ProductDetails>(
                ProductQueries.GetDetailsQuery, new { SKU = sku });

            if (details == null)
                _logger.LogInformation("Product details for SKU: {SKU} not found", sku);
            else
                _logger.LogDebug("Successfully retrieved product details for SKU: {SKU}", sku);

            return details;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product details for SKU: {SKU}", sku);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if product exists with SKU: {SKU}", sku);
            using var connection = _connectionFactory.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<bool>(
                ProductQueries.ExistsQuery, new { SKU = sku });

            _logger.LogDebug("Product with SKU: {SKU} exists: {Exists}", sku, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if product exists with SKU: {SKU}", sku);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetNonWireProductsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all non-wire products");
            using var connection = _connectionFactory.CreateConnection();
            var products = await connection.QueryAsync<Product>(ProductQueries.GetNonWireQuery);

            _logger.LogInformation("Retrieved {Count} non-wire products", products.Count());
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting non-wire products");
            throw;
        }
    }

    public async Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        try
        {
            var productsList = products.ToList();
            _logger.LogInformation("Adding {Count} products to database", productsList.Count);

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(ProductQueries.AddRangeQuery, productsList);
            _logger.LogInformation("Successfully added {Count} products to database", productsList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding products to database");
            throw;
        }
    }

    public async Task<HashSet<string>> GetExistingSkusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var skus = await connection.QueryAsync<string>("SELECT SKU FROM Products");
            return skus.ToHashSet();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting existing SKUs");
            throw;
        }
    }
}