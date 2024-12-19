using Dapper;
using KARacter.WarehouseTest.Application.Common.Interfaces.Factories;
using KARacter.WarehouseTest.Application.Common.Interfaces.Repositories;
using KARacter.WarehouseTest.Domain.Entities;
using KARacter.WarehouseTest.Persistence.Repositories.Queries;
using Microsoft.Extensions.Logging;

namespace KARacter.WarehouseTest.Persistence.Repositories;

public class PriceRepository : IPriceRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<PriceRepository> _logger;

    public PriceRepository(ISqlConnectionFactory connectionFactory, ILogger<PriceRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Price?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting price for SKU: {SKU}", sku);
            using var connection = _connectionFactory.CreateConnection();
            var price = await connection.QuerySingleOrDefaultAsync<Price>(
                PriceQueries.GetBySkuQuery, new { SKU = sku });

            if (price == null)
                _logger.LogInformation("Price for SKU: {SKU} not found", sku);
            else
                _logger.LogDebug("Successfully retrieved price for SKU: {SKU}", sku);

            return price;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting price for SKU: {SKU}", sku);
            throw;
        }
    }

    public async Task AddRangeAsync(IEnumerable<Price> prices, CancellationToken cancellationToken = default)
    {
        try
        {
            var pricesList = prices.ToList();
            _logger.LogInformation("Adding {Count} prices to database", pricesList.Count);

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(PriceQueries.AddRangeQuery, pricesList);
            _logger.LogInformation("Successfully added {Count} prices to database", pricesList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding prices to database");
            throw;
        }
    }
}