using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using WebApi.Extensions;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace WebApi
{
    public class Startup
    {

        private readonly string MyAllowSpecificOrigins = "MyAllowSpecificOrigins";
        public static IConfigurationRoot Config { get; private set; }
        public Startup(IWebHostEnvironment env)
        {
            var Config = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env}.json", optional: true)
               .AddEnvironmentVariables();
            Configuration = Config.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddControllers()

                .AddNewtonsoftJson(s =>
                {
                    s.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    s.SerializerSettings.Converters.Add(new StringEnumConverter());
                });


            services.ConfigureSwaggerGen();
            services.ConfigureSqlContext(Configuration);
            services.ConfigureAuthentication(Configuration);
            services.ConfigureAuthorization();
            services.ConfigureCors(Configuration, MyAllowSpecificOrigins);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerCustom();
                app.UseSwaggerUICustom();

            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();

            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                    .RequireAuthorization(); // aplica o filtro de autorização global em todos os endpoints do controlador
            });
        }
    }

}
