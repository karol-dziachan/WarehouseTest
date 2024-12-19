namespace KARacter.WarehouseTest.Domain.Entities;

public class ProductDetails
{
    // Dane z Products
    public string Name { get; set; } = default!;
    public string? EAN { get; set; }
    public string? ProducerName { get; set; }
    public string? Category { get; set; }
    public string? DefaultImage { get; set; }

    // Dane z Inventory
    public decimal? StockQuantity { get; set; }
    public string? LogisticUnit { get; set; }
    public decimal? ShippingCost { get; set; }

    // Dane z Prices
    public decimal? NetPrice { get; set; }
} 