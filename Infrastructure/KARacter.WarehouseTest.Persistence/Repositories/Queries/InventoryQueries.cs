namespace KARacter.WarehouseTest.Persistence.Repositories.Queries;

internal static class InventoryQueries
{
    internal const string GetBySkuQuery = "SELECT * FROM Inventory WHERE SKU = @SKU";

    internal const string GetByShippingTimeQuery = "SELECT * FROM Inventory WHERE ShippingTime = @ShippingTime";

    internal const string AddRangeQuery = @"
        INSERT INTO Inventory (SKU, Unit, Quantity, ShippingTime, ShippingCost)
        VALUES (@SKU, @Unit, @Quantity, @ShippingTime, @ShippingCost)";
} 