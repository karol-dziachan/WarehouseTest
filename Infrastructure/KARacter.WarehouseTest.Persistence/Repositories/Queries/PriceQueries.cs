namespace KARacter.WarehouseTest.Persistence.Repositories.Queries;

internal static class PriceQueries
{
    internal const string GetBySkuQuery = "SELECT * FROM Prices WHERE SKU = @SKU";

    internal const string AddRangeQuery = @"
        INSERT INTO Prices (SKU, NetPrice, LogisticUnitNetPrice)
        VALUES (@SKU, @NetPrice, @LogisticUnitNetPrice)";
} 