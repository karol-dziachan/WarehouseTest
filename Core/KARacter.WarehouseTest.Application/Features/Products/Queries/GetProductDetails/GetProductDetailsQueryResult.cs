namespace KARacter.WarehouseTest.Application.Features.Products.Queries.GetProductDetails;

public sealed record GetProductDetailsQueryResult
{
    public string SKU { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? EAN { get; init; }
    public string? ProducerName { get; init; }
    public string? Category { get; init; }
    public decimal? Quantity { get; init; }
    public string? ShippingTime { get; init; }
    public decimal? ShippingCost { get; init; }
    public decimal? NetPrice { get; init; }
    public decimal? LogisticUnitNetPrice { get; init; }
    public string? DefaultImage { get; init; }
}