using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using perenne.DTOs;
using perenne.Models;
using perenne.Data;
using perenne.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [CorsEnable] // Garante que os headers CORS são adicionados
    public class IdentityController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        
        public IdentityController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("ping")]
        public ActionResult<object> Ping()
        {
            try
            {
                // Verificar conexão com o banco de dados
                bool dbConnectionWorks = false;
                string? dbError = null;
                
                try {
                    // Tenta fazer uma consulta simples ao banco de dados
                    dbConnectionWorks = _context.Database.CanConnect();
                }
                catch (Exception ex) {
                    dbError = ex.Message;
                }
                
                return Ok(new {
                    message = "API is working",
                    timestamp = DateTime.UtcNow,
                    database = new {
                        connectionString = _configuration.GetConnectionString("DefaultConnection")?.Replace("Password=", "Password=***"),
                        connected = dbConnectionWorks,
                        error = dbError
                    },
                    jwt = new {
                        issuer = _configuration["JwtSettings:Issuer"],
                        audience = _configuration["JwtSettings:Audience"],
                        keyConfigured = !string.IsNullOrEmpty(_configuration["JwtSettings:Key"])
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during ping", error = ex.Message });
            }
        }
        
        [HttpPost("login")]
        [HttpOptions("login")]  // Add explicit OPTIONS handler for CORS preflight
        public ActionResult<AuthResponse> Login([FromBody] LoginRequest request)
        {
            // If it's an OPTIONS request, just return OK - CORS headers handled by attribute
            if (HttpContext.Request.Method == "OPTIONS")
                return Ok();
                
            // Em produção, verificaria credenciais no banco de dados
            // Aqui estamos simplificando para o exemplo
            var user = _context.Users.FirstOrDefault(u => 
                u.Email == request.Email);
                
            if (user == null)
            {
                Console.WriteLine($"Login falhou: Usuário não encontrado - {request.Email}");
                return Unauthorized(new { message = "Credenciais inválidas", error = "user_not_found" });
            }
            
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                Console.WriteLine($"Login falhou: Senha incorreta para {request.Email}");
                return Unauthorized(new { message = "Credenciais inválidas", error = "invalid_password" });
            }

            // Gerar tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();
            
            // Em produção: armazenar o refresh token no banco de dados
            // user.RefreshToken = refreshToken;
            // user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            // _context.SaveChanges();

            // Criar a resposta de autenticação
            var authResponse = new AuthResponse 
            { 
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
            
            // Registrar os dados da resposta para debugging
            Console.WriteLine($"Resposta de autenticação preparada:");
            Console.WriteLine($"  AccessToken: {(accessToken.Length > 20 ? accessToken.Substring(0, 20) + "..." : accessToken)}");
            Console.WriteLine($"  RefreshToken: {(refreshToken.Length > 10 ? refreshToken.Substring(0, 10) + "..." : refreshToken)}");
            Console.WriteLine($"  User: {authResponse.User.Name} ({authResponse.User.Email})");
            
            return Ok(authResponse);
        }

        [HttpPost("refresh")]
        public ActionResult<AuthResponse> Refresh([FromBody] RefreshRequest request)
        {
            // Em produção: validar o refresh token no banco de dados
            // var user = _context.Users.FirstOrDefault(u => 
            //    u.Id.ToString() == request.UserId && 
            //    u.RefreshToken == request.RefreshToken &&
            //    u.RefreshTokenExpiry > DateTime.UtcNow);
            
            // if (user == null)
            // {
            //    return Unauthorized(new { message = "Refresh token inválido ou expirado" });
            // }
            
            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == request.UserId);
            if (user == null)
            {
                return Unauthorized(new { message = "Usuário não encontrado" });
            }

            // Gerar novos tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();
            
            // Em produção: atualizar o refresh token no banco de dados
            // user.RefreshToken = refreshToken;
            // user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            // _context.SaveChanges();

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            });
        }

        private string GenerateAccessToken(User user)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtKey = _configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json");
                var key = Encoding.UTF8.GetBytes(jwtKey);
                
                Console.WriteLine($"Gerando token para usuário: {user.Email} (ID: {user.Id})");
                
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                
                // UserRole is an enum and can't be null, so we always add the role claim
                claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
                
                Console.WriteLine($"Claims adicionadas: {claims.Count}");
                
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Issuer = _configuration["JwtSettings:Issuer"],
                    Audience = _configuration["JwtSettings:Audience"],
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);
                
                Console.WriteLine($"Token gerado com sucesso. Tamanho: {tokenString.Length} caracteres");
                
                return tokenString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar token: {ex.Message}");
                throw;
            }
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
        
        private bool VerifyPassword(string password, string passwordHash)
        {
            // Em produção: implementar verificação real de senha com hash
            // Exemplo: BCrypt.Net.BCrypt.Verify(password, passwordHash)
            return true; // Apenas para exemplo
        }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class RefreshRequest
    {
        public required string RefreshToken { get; set; }
        public required string UserId { get; set; }
    }

    public class AuthResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required UserDto User { get; set; }
    }

    public class UserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}