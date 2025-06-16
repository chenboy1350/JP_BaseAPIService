using JP_APIService.Service;
using Microsoft.AspNetCore.Mvc;
using JP_APIService.Model;
using JP_APIService.Models;

namespace JP_APIService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(AccessTokenService tokenService, IConfiguration configuration) : Controller
    {
        private readonly AccessTokenService _tokenService = tokenService;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("TryToGetToken")]
        public IActionResult TryToGetToken([FromBody] AuthRequestModel model)
        {
            if (string.IsNullOrEmpty(model.ClientId) || string.IsNullOrEmpty(model.ClientSecret))
            {
                return BadRequest("ClientId & ClientSecret is required.");
            }

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var ClientId = jwtSettings["ClientId"];
            var ClientSecret = jwtSettings["ClientSecret"];

            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret))
            {
                return StatusCode(500, "JWT settings are not properly configured.");
            }

            var expected = $"{ClientId}:{ClientSecret}";
            var decodedHeader = $"{model.ClientId}:{model.ClientSecret}";

            if (decodedHeader == expected)
            {
                var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "30");
                var response = new AuthResponseModel
                {
                    AccessToken = _tokenService.GenerateToken(ClientId),
                    ExpiresIn = expirationMinutes * 60,
                };
                return Ok(response);
            }
            else
            {
                return Unauthorized(new { message = "Client authentication failed." });
            }
        }

        [HttpPost("AccessToken")]
        public IActionResult AccessToken([FromBody] AuthRequestModel model)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var ClientId = jwtSettings["ClientId"];
                var ClientSecret = jwtSettings["ClientSecret"];

                if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret))
                {
                    return StatusCode(500, "JWT settings are not properly configured.");
                }

                if (string.IsNullOrEmpty(model.Header))
                {
                    return BadRequest("Missing header.");
                }

                string decodedHeader;
                try
                {
                    var bytes = Convert.FromBase64String(model.Header);
                    decodedHeader = System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return BadRequest("Invalid base64 header.");
                }

                var expected = $"{ClientId}:{ClientSecret}";

                if (decodedHeader == expected)
                {
                    var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "30");
                    var response = new AuthResponseModel
                    {
                        AccessToken = _tokenService.GenerateToken(ClientId),
                        ExpiresIn = expirationMinutes * 60,
                    };
                    return Ok(response);
                }
                else
                {
                    return Unauthorized(new { message = "Client authentication failed." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
