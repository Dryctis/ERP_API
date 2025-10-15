using ERP_API.Data;
using ERP_API.Repositories.Implementations;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Implementations;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddErpServices(this IServiceCollection services, IConfiguration cfg)
    {
        
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(cfg.GetConnectionString("SqlServer"))); 

        
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
