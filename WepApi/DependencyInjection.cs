using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WepApi.Interfaces;
using WepApi.Services;

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
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();


            return services;

        }
    }
}
