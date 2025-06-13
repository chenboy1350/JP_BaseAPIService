using System.Diagnostics;

namespace JP_APIService.Service.Helper
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("[SYS] Starting request {Method} {Url} at {Timestamp}",
                context.Request.Method,
                context.Request.Path,
                DateTime.UtcNow);

            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation("[SYS] Completed request {Method} {Url} with status {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
