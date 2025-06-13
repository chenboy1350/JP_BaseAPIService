using JP_APIService.Models;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Collections.Concurrent;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var apiSettings = new ApiSettingsModel();
builder.Configuration.GetSection("ApiSettings").Bind(apiSettings);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = apiSettings.Title,
        Version = apiSettings.Version,
        Description = apiSettings.Description,
    });
});

builder.Services.Configure<RateLimitingOptionsModel>(
    builder.Configuration.GetSection("RateLimiting")
);

builder.Services.AddRateLimiter(options =>
{
    var limiterCache = new ConcurrentDictionary<string, RateLimiter>();

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var monitor = context.RequestServices.GetRequiredService<IOptionsMonitor<RateLimitingOptionsModel>>();
        var config = monitor.CurrentValue;

        return RateLimitPartition.Get(ip, _ =>
        {
            if (limiterCache.TryRemove(ip, out var oldLimiter))
            {
                oldLimiter.Dispose();
            }

            var newLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                PermitLimit = config.PermitLimit,
                Window = TimeSpan.FromSeconds(config.WindowSeconds),
                QueueLimit = config.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });

            limiterCache[ip] = newLimiter;
            return newLimiter;
        });
    });

    options.RejectionStatusCode = 429;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\": \"Too many requests. Please try again later.\"}", token);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{apiSettings.Title} {apiSettings.Version}");
        c.RoutePrefix = string.Empty;
    });
}

app.UseRateLimiter();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
