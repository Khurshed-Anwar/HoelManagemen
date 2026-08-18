using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace HotelManagement.Middleware
{
    public class GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await WriteProblemResponseAsync(context, ex);
            }
        }

        private Task WriteProblemResponseAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

            var problem = new ProblemDetails
            {
                Status   = context.Response.StatusCode,
                Title    = "An unexpected error occurred.",
                Detail   = env.IsDevelopment() ? ex.Message : "Please contact support if the problem persists.",
                Instance = context.Request.Path
            };

            // Only expose stack trace in Development
            if (env.IsDevelopment())
                problem.Extensions["stackTrace"] = ex.StackTrace;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
        }
    }
}
