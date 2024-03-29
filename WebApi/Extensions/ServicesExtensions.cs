﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using System.Linq;

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

        public static void ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }

        public static void ConfigureSwaggerGen(this IServiceCollection services)
        {
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "BatteriesProject WebApi"
                });
                swagger.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

            });
        }

    }
}
