public interface IDataImportService
{
    Task ImportAllDataAsync(
        string productsUrl, 
        string inventoryUrl, 
        string pricesUrl, 
        CancellationToken cancellationToken = default);
} 