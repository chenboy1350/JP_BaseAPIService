using JP_APIService.Service;
using Microsoft.AspNetCore.Mvc;
using SimpleJWT.Model;

namespace JP_APIService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(TokenService tokenService, IConfiguration configuration) : Controller
    {
        private readonly TokenService _tokenService = tokenService;
        private readonly IConfiguration _configuration = configuration;


        [HttpPost("TryToGetToken")]
        public IActionResult TryToGetToken([FromBody] TryAuthModel model)
        {
            if (string.IsNullOrEmpty(model.ClientId))
            {
                return BadRequest("ClientId is required.");
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
                var token = _tokenService.GenerateToken(model.ClientId);

                return Ok(new { Token = token });
            }
            else
            {
                return Unauthorized(new { message = "Client authentication failed." });
            }
        }

        [HttpPost("GetToken")]
        public IActionResult GetToken([FromBody] AuthModel model)
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
                    var token = _tokenService.GenerateToken(ClientId);
                    return Ok(new { Token = token });
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
