using MediatR;

namespace KARacter.WarehouseTest.Application.Features.DataProcessing.Commands;

public sealed record DataProcessingCommand : IRequest<DataProcessingCommandResult>
{
    public string ProductsUrl { get; init; } = default!;
    public string InventoryUrl { get; init; } = default!;
    public string PricesUrl { get; init; } = default!;
}