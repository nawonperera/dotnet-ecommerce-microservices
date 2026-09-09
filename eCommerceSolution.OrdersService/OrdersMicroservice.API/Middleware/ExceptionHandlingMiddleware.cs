namespace OrdersMicroservice.API.Middleware;

public class ExceptionHandleMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandleMiddleware> _logger;

    public ExceptionHandleMiddleware(RequestDelegate next, ILogger<ExceptionHandleMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            //Log the exception type and message
            _logger.LogError($"{ex.GetType().ToString()}: {ex.Message}");

            if (ex.InnerException is not null)
            {
                //Log the inner exception type and message
                _logger.LogError($"{ex.InnerException.GetType().ToString()}: {ex.InnerException.Message}");
            }

            httpContext.Response.StatusCode = 500;
            await httpContext.Response.WriteAsJsonAsync(new { Message = ex.Message, Type = ex.GetType().ToString() });
        }

    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class ExceptionHandleMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandleMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandleMiddleware>();
    }
}
