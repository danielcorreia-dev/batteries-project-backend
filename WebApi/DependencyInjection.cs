using System;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using WebApi.Interfaces;
using WebApi.Services;

namespace WebApi
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BatteriesProjectDbContext>(
                options => options.UseNpgsql(GetConnectionString(configuration)));

            services.AddScoped<IBatteriesProjectDbContext>( provider => provider.GetService<BatteriesProjectDbContext>());
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            
            return services;
        }

        private static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("BatteriesConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DB_CONNECTION");
            return string.IsNullOrEmpty(databaseUrl) 
                ? connectionString 
                : databaseUrl;
        }
    }
}