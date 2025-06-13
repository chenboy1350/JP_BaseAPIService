using JP_APIService.Models;
using Microsoft.OpenApi.Models;
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
