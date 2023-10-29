using Microsoft.EntityFrameworkCore;
using SmartWay.WebApi.Data;
using SmartWay.WebApi.Interfaces;

namespace SmartWay.WebApi.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ApplicationDbContextInitializer>();
        
        return services;
    }
}