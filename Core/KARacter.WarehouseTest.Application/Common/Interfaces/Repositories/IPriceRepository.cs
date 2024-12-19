using KARacter.WarehouseTest.Domain.Entities;

namespace KARacter.WarehouseTest.Application.Common.Interfaces.Repositories;

public interface IPriceRepository
{
    Task<Price?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Price> prices, CancellationToken cancellationToken = default);
}
