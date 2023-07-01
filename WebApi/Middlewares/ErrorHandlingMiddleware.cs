using System;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Models;
using Domain.Models.Results;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebApi.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task<IActionResult> HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {

            var dbContext =
                (BatteriesProjectDbContext)httpContext.RequestServices.GetService(typeof(BatteriesProjectDbContext));

            var errorLog = new ErrorLog()
            {
                Message = ex.Message,
                Source = ex.TargetSite?.ToString(),
                Timestamp = DateTimeOffset.Now.ToString(),
                Type = ex.GetType().FullName,
                StackTrace = ex.StackTrace,
                TraceId = Guid.NewGuid(),
            };

            await dbContext!.ErrorLogs.AddAsync(errorLog);
            await dbContext.SaveChangesAsync();
            
            var responseObj = new ProblemDetailsModel()
            {
                StatusCode = 500,
                ContentType = "application/json",
                TraceId = errorLog.TraceId,
                Type = errorLog.Type,
            };

            await httpContext.Response.WriteAsJsonAsync(responseObj);
            
            return new EmptyResult();
        }
    }
}