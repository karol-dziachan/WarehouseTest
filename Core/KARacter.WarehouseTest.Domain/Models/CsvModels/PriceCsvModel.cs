namespace KARacter.WarehouseTest.Domain.Models.CsvModels;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using KARacter.WarehouseTest.Domain.Common.Interfaces;

public sealed record PriceCsvModel : IValidatable
{
    public string SKU { get; init; } = default!;
    public string NetPrice { get; init; } = default!;
    public string LogisticUnitNetPrice { get; init; } = default!;

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(SKU))
            return false;

        var netPriceValue = NetPrice?.Trim('"').Trim();
        if (string.IsNullOrWhiteSpace(netPriceValue))
            return false;

        if (!decimal.TryParse(netPriceValue.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            return false;

        var logisticUnitNetPriceValue = LogisticUnitNetPrice?.Trim('"').Trim();
        if (!string.IsNullOrWhiteSpace(logisticUnitNetPriceValue))
        {
            if (!decimal.TryParse(logisticUnitNetPriceValue.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return false;
        }

        return true;
    }

    public void Validate()
    {
        // Pusta implementacja, bo używamy IsValid()
    }
}