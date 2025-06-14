using System.Diagnostics;

namespace JP_APIService.Service
{
    public class RequestLoggingService(RequestDelegate next, Serilog.ILogger logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly Serilog.ILogger _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.Information("[SYS] Starting request {Method} {Url} at {Timestamp}",
                context.Request.Method,
                context.Request.Path,
                DateTime.UtcNow);

            await _next(context);

            stopwatch.Stop();

            _logger.Information("[SYS] Completed request {Method} {Url} with status {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
