using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using perenne.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace perenne.Controllers
{
    public class IdentityController : ControllerBase
    {
        private const string TokenSecret = "andersongabrielvalencamarquesdesa";
        private static readonly TimeSpan TokenExpiration = TimeSpan.FromHours(2);

        [HttpPost("token")]
        public IActionResult GenerateToken(
            [FromBody] TokenGenerationRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TokenSecret);
            var claims = new List<Claim>
                      {
                          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                          new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
                          new Claim(JwtRegisteredClaimNames.Email, request.Email)
                      };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenExpiration),
                Issuer = "Perenne",
                Audience = "Perenne",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(tokenString);
        }
    }


}
