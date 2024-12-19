namespace KARacter.WarehouseTest.Domain.Entities;

public sealed record Inventory
{
    public Inventory() { } // dla Dappera

    public Inventory(
        string sku,
        string? unit,
        decimal? quantity,
        string? shippingTime,
        decimal? shippingCost)
    {
        SKU = !string.IsNullOrWhiteSpace(sku) ? sku : throw new ArgumentException("SKU cannot be empty", nameof(sku));
        Unit = unit;
        Quantity = quantity;
        ShippingTime = shippingTime;
        ShippingCost = shippingCost is null or >= 0 ? shippingCost : throw new ArgumentException("Shipping cost cannot be negative", nameof(shippingCost));
    }

    public string SKU { get; init; }
    public string? Unit { get; init; }
    public decimal? Quantity { get; init; }
    public string? ShippingTime { get; init; }
    public decimal? ShippingCost { get; init; }
}