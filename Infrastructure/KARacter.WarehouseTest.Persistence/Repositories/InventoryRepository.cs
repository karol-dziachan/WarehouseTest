using Dapper;
using KARacter.WarehouseTest.Application.Common.Interfaces.Factories;
using KARacter.WarehouseTest.Application.Common.Interfaces.Repositories;
using KARacter.WarehouseTest.Domain.Entities;
using KARacter.WarehouseTest.Persistence.Repositories.Queries;
using Microsoft.Extensions.Logging;

namespace KARacter.WarehouseTest.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<InventoryRepository> _logger;

    public InventoryRepository(ISqlConnectionFactory connectionFactory, ILogger<InventoryRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Inventory?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting inventory for SKU: {SKU}", sku);
            using var connection = _connectionFactory.CreateConnection();
            var inventory = await connection.QuerySingleOrDefaultAsync<Inventory>(
                InventoryQueries.GetBySkuQuery, new { SKU = sku });

            if (inventory == null)
                _logger.LogInformation("Inventory for SKU: {SKU} not found", sku);
            else
                _logger.LogDebug("Successfully retrieved inventory for SKU: {SKU}", sku);

            return inventory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting inventory for SKU: {SKU}", sku);
            throw;
        }
    }

    public async Task<IEnumerable<Inventory>> GetByShippingTimeAsync(string shippingTime, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting inventory items with shipping time: {ShippingTime}", shippingTime);
            using var connection = _connectionFactory.CreateConnection();
            var inventories = await connection.QueryAsync<Inventory>(
                InventoryQueries.GetByShippingTimeQuery, new { ShippingTime = shippingTime });

            _logger.LogInformation("Retrieved {Count} inventory items with shipping time: {ShippingTime}",
                inventories.Count(), shippingTime);
            return inventories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting inventory items with shipping time: {ShippingTime}", shippingTime);
            throw;
        }
    }

    public async Task AddRangeAsync(IEnumerable<Inventory> inventories, CancellationToken cancellationToken = default)
    {
        try
        {
            var uniqueInventories = inventories
                .GroupBy(i => i.SKU)
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("Adding {Count} unique inventory items to database (from {TotalCount} total)", 
                uniqueInventories.Count, inventories.Count());

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(InventoryQueries.AddRangeQuery, uniqueInventories);
            
            _logger.LogInformation("Successfully added {Count} inventory items to database", uniqueInventories.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding inventory items to database");
            throw;
        }
    }
}