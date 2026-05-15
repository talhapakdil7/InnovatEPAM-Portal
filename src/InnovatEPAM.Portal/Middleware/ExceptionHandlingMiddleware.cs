using InnovatEPAM.Portal.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace InnovatEPAM.Portal.Middleware;

/// <summary>
/// Catches unhandled exceptions, logs them, and redirects to a friendly error page.
/// Detects concurrency errors and passes them to the view for special handling (retry button).
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            // Avoid redirect loop when /Home/Error itself fails
            var path = context.Request.Path.Value ?? "";
            if (path.StartsWith("/Home/Error", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync("Server error. See application logs for details.");
                return;
            }

            // Detect concurrency errors to provide better UX (retry button)
            var isConcurrencyError = ex is InvalidOperationException 
                && (ex.Message.Contains("modified by another", StringComparison.OrdinalIgnoreCase) 
                    || ex.Message.Contains("deleted by another", StringComparison.OrdinalIgnoreCase));

            // Store error info in TempData for the error page
            var tempData = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>()
                .GetTempData(context);
            
            tempData["ErrorMessage"] = ex.Message;
            tempData["IsConcurrencyError"] = isConcurrencyError;
            tempData["RequestId"] = Activity.Current?.Id ?? context.TraceIdentifier;

            context.Response.Redirect("/Home/Error");
        }
    }
}
