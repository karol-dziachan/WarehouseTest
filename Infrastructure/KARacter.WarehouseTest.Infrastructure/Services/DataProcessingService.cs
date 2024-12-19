using KARacter.WarehouseTest.Application.Common.Interfaces.Services;
using KARacter.WarehouseTest.Application.Common.Interfaces.Repositories;
using KARacter.WarehouseTest.Domain.Models.CsvModels;
using Microsoft.Extensions.Logging;
using KARacter.WarehouseTest.Domain.Entities;
using System.Globalization;

namespace KARacter.WarehouseTest.Infrastructure.Services;

public class DataProcessingService : IDataProcessingService
{
    private readonly ILogger<DataProcessingService> _logger;
    private readonly IFileService _fileService;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPriceRepository _priceRepository;

    public DataProcessingService(
        ILogger<DataProcessingService> logger,
        IFileService fileService,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        IPriceRepository priceRepository)
    {
        _logger = logger;
        _fileService = fileService;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _priceRepository = priceRepository;
    }

    /// <summary>
    /// Przeczyta produkty z powyższego pliku, dane produktów które nie są kablami
    ///oraz są wysyłane w przeciągu 24h zapisze w tabeli SQL
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ProcessProductsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting to process products from file: {FilePath}", filePath);

            // Najpierw pobieramy SKU produktów z 24h shipping time z inventory
            var productsWithFastShipping = await _inventoryRepository.GetByShippingTimeAsync("24h", cancellationToken);
            var fastShippingSkus = productsWithFastShipping.Select(i => i.SKU).ToHashSet();

            _logger.LogInformation("Found {Count} products with 24h shipping", fastShippingSkus.Count);

            var csvRecords = await _fileService.ReadCsvFileAsync<ProductCsvModel>(filePath, true, cancellationToken);
            _logger.LogInformation("Read {Count} records from CSV file", csvRecords.Count());

            var productsToSave = csvRecords
                .Where(p => p.IsWire == "0") // nie są kablami (0 = false)
                .Where(p => fastShippingSkus.Contains(p.SKU)) // mają 24h shipping
                .Select(MapToProduct)
                .ToList();

            await _productRepository.AddRangeAsync(productsToSave, cancellationToken);

            _logger.LogInformation("Successfully processed {Count} products (non-wire with 24h shipping)",
                productsToSave.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing products from file: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Przeczyta dane o stanie magazynowym z powyższego pliku i zapisze stany
    ///produktów które są wysyłane w przeciągu 24h w tabeli SQL
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ProcessInventoryAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing inventory start");

            var csvRecords = await _fileService.ReadCsvFileAsync<string[]>(filePath, false, cancellationToken);
            _logger.LogInformation("CSV records read: {Count}", csvRecords.Count());

            var parsedRecords = csvRecords
                .Select(ParseInventoryLine)
                .Where(i => i != null)
                .ToList();
            _logger.LogInformation("Parsed records: {Count}", parsedRecords.Count);

            var inventoryToSave = parsedRecords
                .Where(i => i!.ShippingTime?.Contains("24h") == true)
                .ToList();
            _logger.LogInformation("Records with 24h shipping: {Count}", inventoryToSave.Count);

            if (inventoryToSave.Any())
            {
                await _inventoryRepository.AddRangeAsync(inventoryToSave, cancellationToken);
                _logger.LogInformation("Saved to database");
            }
            else
            {
                _logger.LogWarning("No records to save");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inventory");
            throw;
        }
    }

    /// <summary>
    /// Przeczyta ceny produktów z powyższego pliku i zapisze ich dane w tabeli
    /// SQL.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ProcessPricesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting to process prices from file: {FilePath}", filePath);

            var csvRecords = await _fileService.ReadCsvFileAsync<PriceCsvModel>(filePath, true, cancellationToken);
            _logger.LogInformation("Read {Count} records from CSV file", csvRecords.Count());

            // Pobierz istniejące SKU z repozytorium
            var existingSkus = await _productRepository.GetExistingSkusAsync(cancellationToken);

            var validRecords = csvRecords
                .Where(r => r.IsValid())
                .Where(r => existingSkus.Contains(r.SKU)) // Filtruj tylko istniejące SKU
                .ToList();

            _logger.LogInformation(
                "Found {ValidCount} valid records (matching existing products) from {TotalCount} total",
                validRecords.Count,
                csvRecords.Count());

            if (validRecords.Any())
            {
                var pricesToSave = validRecords
                    .Select(MapToPrice)
                    .ToList();

                await _priceRepository.AddRangeAsync(pricesToSave, cancellationToken);
                _logger.LogInformation("Successfully processed {Count} price records", pricesToSave.Count);
            }
            else
            {
                _logger.LogWarning("No valid price records found for existing products");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing prices from file: {FilePath}", filePath);
            throw;
        }
    }

    private static Product MapToProduct(ProductCsvModel model)
    {
        return new Product(
            sku: model.SKU,
            name: model.Name,
            ean: model.EAN,
            producerName: model.ProducerName,
            category: model.Category,
            defaultImage: model.DefaultImage,
            isWire: model.IsWire == "1"
        );
    }

    private Inventory? ParseInventoryLine(string[] line)
    {
        try
        {
            if (line == null || line.Length == 0) return null;

            var data = line.Length == 1
                ? line[0].Split(',', StringSplitOptions.TrimEntries)
                : line;

            if (data.Length < 7) return null;

            // Parsowanie quantity z obsługą formatowania
            if (!decimal.TryParse(data[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var rawQty))
            {
                return null;
            }

            // Konwersja na int jeśli nie ma części ułamkowej
            var qty = rawQty % 1 == 0 ? Math.Round(rawQty, 0) : rawQty;

            var shippingTime = data[6].Trim().Replace("\"", "");

            // Parsowanie shipping cost z obsługą formatowania
            decimal? shippingCost = null;
            if (data.Length > 7 && !string.IsNullOrWhiteSpace(data[7]))
            {
                if (decimal.TryParse(data[7].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var rawCost))
                {
                    // Konwersja na int jeśli nie ma części ułamkowej
                    shippingCost = rawCost % 1 == 0 ? Math.Round(rawCost, 0) : rawCost;
                }
            }

            return new Inventory(
                sku: data[1].Trim(),
                unit: data[2].Trim(),
                quantity: qty,
                shippingTime: shippingTime,
                shippingCost: shippingCost
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parse error");
            return null;
        }
    }

    private static Price MapToPrice(PriceCsvModel model)
    {
        var netPrice = decimal.Parse(model.NetPrice.Trim('"').Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture);

        decimal logisticUnitNetPrice = 0;
        var logisticUnitNetPriceValue = model.LogisticUnitNetPrice?.Trim('"').Trim();
        if (!string.IsNullOrWhiteSpace(logisticUnitNetPriceValue))
        {
            logisticUnitNetPrice = decimal.Parse(logisticUnitNetPriceValue.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        return new Price(
            sku: model.SKU,
            netPrice: netPrice,
            logisticUnitNetPrice: logisticUnitNetPrice
        );
    }

    public async Task<List<Inventory>> LoadInventoryDataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var csvRecords = await _fileService.ReadCsvFileAsync<string[]>(filePath, false, cancellationToken);
        return csvRecords
            .Select(ParseInventoryLine)
            .Where(i => i != null)
            .ToList()!;
    }

    public async Task<List<Product>> LoadProductsDataAsync(
        string filePath,
        HashSet<string> validSkus,
        CancellationToken cancellationToken = default)
    {
        var csvRecords = await _fileService.ReadCsvFileAsync<ProductCsvModel>(filePath, true, cancellationToken);
        return csvRecords
            .Where(p => p.IsWire == "0") // nie są kablami
            .Where(p => validSkus.Contains(p.SKU)) // mają 24h shipping
            .Select(MapToProduct)
            .ToList();
    }

    public async Task SaveProductsAsync(List<Product> products, CancellationToken cancellationToken = default)
    {
        await _productRepository.AddRangeAsync(products, cancellationToken);
    }

    public async Task SaveInventoryAsync(List<Inventory> inventory, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting to save inventory data. Total records: {Count}", inventory.Count);

            // Pobierz istniejące SKU z repozytorium
            var existingSkus = await _productRepository.GetExistingSkusAsync(cancellationToken);

            // Filtruj inventory tylko dla istniejących produktów
            var filteredInventory = inventory
                .Where(i => existingSkus.Contains(i.SKU))
                .ToList();

            _logger.LogInformation(
                "Filtered inventory records: {FilteredCount} (matching existing products from {TotalCount} total)",
                filteredInventory.Count,
                inventory.Count);

            if (filteredInventory.Any())
            {
                await _inventoryRepository.AddRangeAsync(filteredInventory, cancellationToken);
                _logger.LogInformation("Successfully saved inventory data");
            }
            else
            {
                _logger.LogWarning("No matching inventory records found for existing products");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving inventory data");
            throw;
        }
    }
}
