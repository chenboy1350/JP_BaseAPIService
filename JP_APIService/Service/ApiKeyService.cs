using JP_APIService.Models;
using Microsoft.Extensions.Options;

namespace JP_APIService.Service
{
    public class ApiKeyService(RequestDelegate next, IOptions<ApiKeySettingsModel> options)
    {
        private readonly RequestDelegate _next = next;
        private const string API_KEY_HEADER_NAME = "X-API-KEY";
        private readonly string _apiKey = options.Value.Key;

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key is missing");
                return;
            }

            if (!_apiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            await _next(context);
        }
    }
}
