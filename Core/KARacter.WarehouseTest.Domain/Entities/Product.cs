namespace KARacter.WarehouseTest.Domain.Entities;

public sealed record Product
{
    public Product() { } // dla Dappera

    public Product(
        string sku,
        string name,
        string? ean,
        string? producerName,
        string? category,
        string? defaultImage,
        bool isWire)
    {
        SKU = !string.IsNullOrWhiteSpace(sku) ? sku : throw new ArgumentException("SKU cannot be empty", nameof(sku));
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name cannot be empty", nameof(name));
        EAN = ean;
        ProducerName = producerName;
        Category = category;
        DefaultImage = defaultImage;
        IsWire = isWire;
    }

    public string SKU { get; init; }
    public string Name { get; init; }
    public string? EAN { get; init; }
    public string? ProducerName { get; init; }
    public string? Category { get; init; }
    public string? DefaultImage { get; init; }
    public bool IsWire { get; init; }
}