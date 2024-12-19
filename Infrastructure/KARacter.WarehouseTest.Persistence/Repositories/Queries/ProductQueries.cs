namespace KARacter.WarehouseTest.Persistence.Repositories.Queries;

internal static class ProductQueries
{
    internal const string GetBySkuQuery = "SELECT * FROM Products WHERE SKU = @SKU";

    internal const string GetDetailsQuery = @"
        SELECT 
            p.Name, p.EAN, p.ProducerName, p.Category, p.DefaultImage,
            i.Quantity as StockQuantity, i.Unit as LogisticUnit, i.ShippingCost,
            pr.NetPrice
        FROM Products p
        LEFT JOIN Inventory i ON p.SKU = i.SKU
        LEFT JOIN Prices pr ON p.SKU = pr.SKU
        WHERE p.SKU = @SKU";

    internal const string ExistsQuery = "SELECT COUNT(1) FROM Products WHERE SKU = @SKU";

    internal const string GetNonWireQuery = "SELECT * FROM Products WHERE IsWire = 0";

    internal const string AddRangeQuery = @"
        INSERT INTO Products (SKU, Name, EAN, ProducerName, Category, DefaultImage, IsWire)
        VALUES (@SKU, @Name, @EAN, @ProducerName, @Category, @DefaultImage, @IsWire)";
}