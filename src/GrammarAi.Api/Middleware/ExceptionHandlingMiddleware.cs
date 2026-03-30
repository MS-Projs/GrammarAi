using System.Net;
using System.Text.Json;

namespace GrammarAi.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            KeyNotFoundException  => (HttpStatusCode.NotFound,            "Resource not found"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,  "Unauthorized"),
            ArgumentException     => (HttpStatusCode.BadRequest,          "Bad request"),
            InvalidOperationException e when e.Message.Contains("not found") =>
                                     (HttpStatusCode.NotFound,            "Resource not found"),
            _                     => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title,
            status = (int)status,
            detail = ex.Message,
            traceId = context.TraceIdentifier
        };

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
