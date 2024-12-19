namespace KARacter.WarehouseTest.Domain.Entities;

public sealed record Price
{
    public Price() { } // dla Dappera

    public Price(
        string sku,
        decimal? netPrice,
        decimal? logisticUnitNetPrice)
    {
        SKU = !string.IsNullOrWhiteSpace(sku) ? sku : throw new ArgumentException("SKU cannot be empty", nameof(sku));
        NetPrice = netPrice is null or >= 0 ? netPrice : throw new ArgumentException("Net price cannot be negative", nameof(netPrice));
        LogisticUnitNetPrice = logisticUnitNetPrice is null or >= 0 ? logisticUnitNetPrice : throw new ArgumentException("Logistic unit net price cannot be negative", nameof(logisticUnitNetPrice));
    }

    public string SKU { get; init; }
    public decimal? NetPrice { get; init; }
    public decimal? LogisticUnitNetPrice { get; init; }
}