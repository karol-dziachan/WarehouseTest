using KARacter.WarehouseTest.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KARacter.WarehouseTest.Infrastructure.Services;

public class DataImportService : IDataImportService
{
    private readonly ILogger<DataImportService> _logger;
    private readonly IDataProcessingService _dataProcessingService;
    private readonly IFileService _fileService;

    public DataImportService(
        ILogger<DataImportService> logger,
        IDataProcessingService dataProcessingService,
        IFileService fileService)
    {
        _logger = logger;
        _dataProcessingService = dataProcessingService;
        _fileService = fileService;
    }

    public async Task ImportAllDataAsync(
        string productsUrl,
        string inventoryUrl,
        string pricesUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting data import process");

            // 1. Pobieramy wszystkie pliki
            var inventoryFile = await _fileService.DownloadAndSaveFileAsync(inventoryUrl, "inventory.csv", cancellationToken);
            var pricesFile = await _fileService.DownloadAndSaveFileAsync(pricesUrl, "prices.csv", cancellationToken);
            var productsFile = await _fileService.DownloadAndSaveFileAsync(productsUrl, "products.csv", cancellationToken);

            // 2. Wczytujemy inventory do pamięci i filtrujemy produkty z 24h shipping
            var inventoryData = await _dataProcessingService.LoadInventoryDataAsync(inventoryFile, cancellationToken);
            var fastShippingSkus = inventoryData
                .Where(i => i.ShippingTime?.Contains("24h") == true)
                .Select(i => i.SKU)
                .Distinct()
                .ToHashSet();

            _logger.LogInformation("Found {Count} products with 24h shipping", fastShippingSkus.Count);

            // 3. Wczytujemy i filtrujemy produkty

            var productsToSave = await _dataProcessingService.LoadProductsDataAsync(
                productsFile,
                fastShippingSkus,
                cancellationToken);

            _logger.LogInformation("Filtered {Count} valid products", productsToSave.Count);

            // 4. Zapisujemy produkty
            await _dataProcessingService.SaveProductsAsync(productsToSave, cancellationToken);

            // 5. Zapisujemy inventory dla zapisanych produktów
            var inventoryToSave = inventoryData
                .Where(i => fastShippingSkus.Contains(i.SKU))
                .ToList();

            await _dataProcessingService.SaveInventoryAsync(inventoryToSave, cancellationToken);

            // 6. Na końcu zapisujemy ceny
            await _dataProcessingService.ProcessPricesAsync(pricesFile, cancellationToken);

            _logger.LogInformation("Data import completed successfully");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data import process");
            throw;
        }
    }
}