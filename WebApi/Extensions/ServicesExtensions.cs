using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Text;

namespace WebApi.Extensions
{
    public static class ServicesExtensions
    {
        public static IConfiguration configuration;

        public static void ConfigureAuthentication(this IServiceCollection services,
            IConfiguration configuration)
        {

            var key = Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretyKey"));

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

        }

        public static void ConfigureCors(this IServiceCollection services,
            IConfiguration configuration, string policy)
        {

            var hosts = configuration.GetSection("AllowedOrigins").Get<List<string>>();

            services.AddCors(options =>
            {
                options.AddPolicy(policy, builder =>
                {
                    builder
                        .WithOrigins(hosts.ToArray())
                        .AllowAnyMethod()
                        .AllowAnyHeader();

                });
            });
        }
       
    }
}
