using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KARacter.WarehouseTest.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using KARacter.WarehouseTest.Infrastructure.Middlewares;
using KARacter.WarehouseTest.Application.Common.Interfaces.Services;
using KARacter.WarehouseTest.Application.Common.Models.Configuration;
using System.Net;
using KARacter.WarehouseTest.Infrastructure.Constants;

namespace KARacter.WarehouseTest.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileServiceOptions>(
                configuration.GetSection(FileServiceOptions.SectionName));

            ConfigureHttpClient(services);

            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IDateTime, DateTimeService>();
            services.AddTransient<IDataProcessingService, DataProcessingService>();
            services.AddTransient<IDataImportService, DataImportService>();

            return services;
        }

        private static void ConfigureHttpClient(IServiceCollection services)
        {
            services.AddHttpClient(HttpClientNames.FileDownloader, client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "FileDownloader/1.0");
                client.Timeout = TimeSpan.FromMinutes(5);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 3,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        }

        public static void ConfigureMiddlewares(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
