using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WepApi
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) 
        {

            services.AddDbContext<BatteriesProjectDbContext>(
               options => options.UseSqlServer(
                configuration.GetConnectionString("BatteriesConnection")));

            services.AddScoped<IBatteriesProjectDbContext>( provider => provider.GetService<BatteriesProjectDbContext>());

            return services;

        }
    }
}
