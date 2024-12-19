using KARacter.WarehouseTest.Domain.Entities;

namespace KARacter.WarehouseTest.Application.Common.Interfaces.Repositories;

public interface IInventoryRepository
{
    Task<Inventory?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Inventory>> GetByShippingTimeAsync(string shippingTime, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Inventory> inventories, CancellationToken cancellationToken = default);
}
