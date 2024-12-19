using KARacter.WarehouseTest.Domain.Entities;

namespace KARacter.WarehouseTest.Application.Common.Interfaces.Services;

public interface IDataProcessingService
{
    /// <summary>
    /// Wczytuje i przetwarza dane produktów z pliku CSV
    /// </summary>
    Task ProcessProductsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Wczytuje i przetwarza dane inventory z pliku CSV
    /// </summary>
    Task ProcessInventoryAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Wczytuje i przetwarza dane cen z pliku CSV
    /// </summary>
    Task ProcessPricesAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Wczytuje dane inventory z pliku CSV do pamięci
    /// </summary>
    Task<List<Inventory>> LoadInventoryDataAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Wczytuje i filtruje dane produktów z pliku CSV do pamięci
    /// </summary>
    Task<List<Product>> LoadProductsDataAsync(
        string filePath,
        HashSet<string> validSkus,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Zapisuje przefiltrowane produkty do bazy danych
    /// </summary>
    Task SaveProductsAsync(List<Product> products, CancellationToken cancellationToken = default);

    /// <summary>
    /// Zapisuje przefiltrowane dane inventory do bazy danych
    /// </summary>
    Task SaveInventoryAsync(List<Inventory> inventory, CancellationToken cancellationToken = default);
}