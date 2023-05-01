using Microsoft.AspNetCore.Builder;

namespace WebApi.Extensions
{
    public static class ApplicationsExtensions
    {
        public static void UseSwaggerCustom(this IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "/swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
        }

        public static void UseSwaggerUICustom(this IApplicationBuilder app)
        {
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
        }
    }
}
