using KARacter.WarehouseTest.Domain.Entities;

namespace KARacter.WarehouseTest.Application.Common.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<ProductDetails> GetDetailsBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetNonWireProductsAsync(CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);
    Task<HashSet<string>> GetExistingSkusAsync(CancellationToken cancellationToken = default);
}
