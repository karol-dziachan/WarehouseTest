using MediatR;
using Microsoft.Extensions.Logging;
using KARacter.WarehouseTest.Application.Common.Interfaces.Services;

namespace KARacter.WarehouseTest.Application.Features.DataProcessing.Commands;

public sealed class DataProcessingCommandHandler : IRequestHandler<DataProcessingCommand, DataProcessingCommandResult>
{
    private readonly ILogger<DataProcessingCommandHandler> _logger;
    private readonly IDataImportService _dataImportService;
    private readonly IDateTime _dateTime;

    public DataProcessingCommandHandler(
        ILogger<DataProcessingCommandHandler> logger,
        IDataImportService dataImportService,
        IDateTime dateTime)
    {
        _logger = logger;
        _dataImportService = dataImportService;
        _dateTime = dateTime;
    }

    public async Task<DataProcessingCommandResult> Handle(
        DataProcessingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting data processing from URLs: Products: {ProductsUrl}, Inventory: {InventoryUrl}, Prices: {PricesUrl}",
                request.ProductsUrl, request.InventoryUrl, request.PricesUrl);

            await _dataImportService.ImportAllDataAsync(
                request.ProductsUrl,
                request.InventoryUrl,
                request.PricesUrl,
                cancellationToken);

            var now = _dateTime.Now;
            _logger.LogInformation("Data processing completed successfully at {Time}", now);

            return DataProcessingCommandResult.Succeeded(
                "Data processing completed successfully",
                now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data processing");
            return DataProcessingCommandResult.Failed(
                $"Data processing failed: {ex.Message}",
                _dateTime.Now);
        }
    }
} 