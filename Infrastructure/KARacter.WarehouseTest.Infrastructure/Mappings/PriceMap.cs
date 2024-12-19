using CsvHelper.Configuration;
using KARacter.WarehouseTest.Domain.Models.CsvModels;
using System.Globalization;

namespace KARacter.WarehouseTest.Infrastructure.Mappings;

public sealed class PriceMap : ClassMap<PriceCsvModel>
{
    public PriceMap()
    {
        Map(m => m.SKU).Index(1);
        Map(m => m.NetPrice).Index(3);
        Map(m => m.LogisticUnitNetPrice).Index(5);
    }
} 