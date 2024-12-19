namespace KARacter.WarehouseTest.Domain.Models.CsvModels;
using System.ComponentModel.DataAnnotations;
using KARacter.WarehouseTest.Domain.Common.Interfaces;

public sealed record ProductCsvModel : IValidatable
{
    public string ID { get; init; } = default!;
    public string SKU { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string EAN { get; init; } = default!;
    public string ProducerName { get; init; } = default!;
    public string Category { get; init; } = default!;
    public string IsWire { get; init; } = default!;
    public string DefaultImage { get; init; } = default!;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SKU) && !string.IsNullOrWhiteSpace(Name);
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SKU))
            throw new ValidationException("SKU cannot be empty");
        // Nie sprawdzamy Name - rekordy z pustym Name będą ignorowane
    }
}