using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JP_APIService.Service
{
    public class TokenService(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;

        public string GenerateToken(string ClientId)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKeyValue = jwtSettings["SecretKey"];
                var tokenExpirationInMinutes = jwtSettings["TokenExpirationInMinutes"];

                if (string.IsNullOrEmpty(secretKeyValue))
                {
                    throw new InvalidOperationException("SecretKey is not configured in JwtSettings.");
                }

                if (!int.TryParse(tokenExpirationInMinutes, out int tokenExpiration))
                {
                    throw new InvalidOperationException("TokenExpirationInMinutes is not a valid integer.");
                }

                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyValue));

                var claims = new[]
                {
                        new Claim(JwtRegisteredClaimNames.Sub, ClientId),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(tokenExpiration),
                    signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
