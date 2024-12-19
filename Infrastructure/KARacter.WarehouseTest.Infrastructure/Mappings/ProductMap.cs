using CsvHelper.Configuration;
using KARacter.WarehouseTest.Domain.Models.CsvModels;

namespace KARacter.WarehouseTest.Infrastructure.Mappings;

public sealed class ProductMap : ClassMap<ProductCsvModel>
{
    public ProductMap()
    {
        Map(m => m.ID).Name("id").Optional();
        Map(m => m.SKU).Name("sku");  // Walidacja w modelu
        Map(m => m.Name).Name("name"); // Walidacja w modelu
        Map(m => m.EAN).Name("ean").Optional();
        Map(m => m.ProducerName).Name("producer_name").Optional();
        Map(m => m.Category).Name("category").Optional();
        Map(m => m.IsWire).Name("is_wire").Default("0");
        Map(m => m.DefaultImage).Name("default_image").Optional();
    }
}