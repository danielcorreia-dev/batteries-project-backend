using System;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using WebApi.Interfaces;
using WebApi.Services;
using WebApi.Services.AWS.S3;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Domain.Models.Params;
using Microsoft.Extensions.Configuration;


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
            services.AddScoped<IS3Service, S3Service>();
            services.AddAWSService<IAmazonS3>();

            
            return services;
        }

        private static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("BatteriesConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) 
                ? connectionString 
                : BuildConnectionStringFromUrl(databaseUrl);
        }
        
        private static string BuildConnectionStringFromUrl(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(":");
            var builder = new NpgsqlConnectionStringBuilder()
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }
    }
}