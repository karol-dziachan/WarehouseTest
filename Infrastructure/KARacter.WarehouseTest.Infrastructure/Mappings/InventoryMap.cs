using CsvHelper.Configuration;
using KARacter.WarehouseTest.Domain.Models.CsvModels;

namespace KARacter.WarehouseTest.Infrastructure.Mappings;

public sealed class InventoryMap : ClassMap<InventoryCsvModel>
{
    public InventoryMap()
    {
        Map(m => m.ProductId).Name("product_id");
        Map(m => m.SKU).Name("sku");
        Map(m => m.Unit).Name("unit");
        Map(m => m.Qty).Name("qty");
        Map(m => m.ManufacturerName).Name("manufacturer_name");
        Map(m => m.ManufacturerRefNum).Name("manufacturer_ref_num");
        Map(m => m.Shipping).Name("shipping");
        Map(m => m.ShippingCost).Name("shipping_cost");
    }
} 