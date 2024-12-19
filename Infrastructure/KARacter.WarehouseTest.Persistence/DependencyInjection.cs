using KARacter.WarehouseTest.Application.Common.Interfaces.Factories;
using KARacter.WarehouseTest.Application.Common.Interfaces.Repositories;
using KARacter.WarehouseTest.Persistence.Database;
using KARacter.WarehouseTest.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KARacter.WarehouseTest.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("KARacter.WarehouseTestDb");
            services.AddTransient<ISqlConnectionFactory, SqlConnectionFactory>(provider => new SqlConnectionFactory(connectionString));
            services.AddTransient<IPriceRepository, PriceRepository>();
            services.AddTransient<IInventoryRepository, InventoryRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            return services;
        }
    }
}
