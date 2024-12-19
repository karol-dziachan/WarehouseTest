namespace KARacter.WarehouseTest.Domain.Models.CsvModels;
using System.ComponentModel.DataAnnotations;
using KARacter.WarehouseTest.Domain.Common.Interfaces;

public sealed record InventoryCsvModel : IValidatable
{
    public string ProductId { get; init; } = default!;
    public string SKU { get; init; } = default!;
    public string Unit { get; init; } = default!;
    public string Qty { get; init; } = default!;
    public string? ManufacturerName { get; init; }
    public string? ManufacturerRefNum { get; init; }
    public string Shipping { get; init; } = default!;
    public string? ShippingCost { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SKU))
            throw new ValidationException("SKU cannot be empty");

        if (string.IsNullOrWhiteSpace(Unit))
            throw new ValidationException("Unit cannot be empty");

        if (!decimal.TryParse(Qty, out _))
            throw new ValidationException("Invalid Quantity format");

        if (!string.IsNullOrWhiteSpace(ShippingCost) && !decimal.TryParse(ShippingCost, out _))
            throw new ValidationException("Invalid Shipping Cost format");
    }
}