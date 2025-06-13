using JP_APIService.Models;
using JP_APIService.Service.Helper;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var AppInfo = new SwaggerInfoModel();
builder.Configuration.GetSection("AppInfo").Bind(AppInfo);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = AppInfo.Title,
        Version = AppInfo.Version,
        Description = AppInfo.Description,
    });
});

var rateLimitConfig = new RateLimitingOptionsModel();
builder.Configuration.GetSection("RateLimiting").Bind(rateLimitConfig);

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimitConfig.PermitLimit,
            Window = TimeSpan.FromSeconds(rateLimitConfig.WindowSeconds),
            QueueLimit = rateLimitConfig.QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    options.RejectionStatusCode = 429;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\": \"Too many requests. Please try again later.\"}", token);
    };
});

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{AppInfo.Title} {AppInfo.Version}");
        c.RoutePrefix = string.Empty;
    });
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : httpContext.Response.StatusCode > 499
            ? LogEventLevel.Error
            : LogEventLevel.Information;
});

app.UseRateLimiter();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseMiddleware<RequestLoggingMiddleware>();

app.Run();
