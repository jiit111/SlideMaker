using System.Net;
using System.Text.Json;

namespace SlideMaker.Web.Middleware;

/// <summary>
/// Catches unhandled exceptions so stack traces/connection strings/keys never reach the
/// browser (spec §41). Full technical detail is logged server-side only.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Browsers requesting a page send "Accept: text/html"; fetch()/XHR calls from our own
            // JS (see wwwroot/js/*.js) generally don't, so default to a JSON error for anything
            // that doesn't explicitly ask for HTML — an HTML redirect would break `.then(r => r.json())`.
            var wantsHtml = context.Request.Headers.Accept.Any(h => h != null && h.Contains("text/html"));

            if (!wantsHtml)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Something went wrong while processing your request. Please try again."
                }));
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}
