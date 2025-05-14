using Microsoft.AspNetCore.Mvc;
using perenne.Models;
using perenne.Data;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        
        public TestController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("create-test-user")]
        public ActionResult<object> CreateTestUser()
        {
            try {
                // Verifica se o usuário já existe
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == "teste123@gmail.com");
                if (existingUser != null)
                {
                    return Ok(new { message = "Usuário de teste já existe", userId = existingUser.Id });
                }

                // Cria um usuário de teste
                var testUser = new User
                {
                    Email = "teste123@gmail.com",
                    Password = "senha123",
                    PasswordHash = "senha123", // Em produção, isso seria um hash real
                    FirstName = "Usuário",
                    LastName = "Teste",
                    CPF = "12345678901",
                    Role = UserRole.Admin,
                    IsValidated = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Users.Add(testUser);
                _context.SaveChanges();
                
                return Ok(new { 
                    message = "Usuário de teste criado com sucesso", 
                    userId = testUser.Id,
                    email = testUser.Email,
                    password = "senha123"
                });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }

        [HttpGet("diagnostics")]
        public ActionResult<object> GetDiagnostics()
        {
            try {
                // Verificar conexão com o banco de dados
                bool dbConnectionWorks = false;
                string dbError = null;
                int userCount = 0;
                var users = new List<object>();
                
                try {
                    // Tenta fazer uma consulta simples ao banco de dados
                    dbConnectionWorks = _context.Database.CanConnect();
                    if (dbConnectionWorks) {
                        userCount = _context.Users.Count();
                        users = _context.Users
                            .Select(u => new {
                                id = u.Id,
                                email = u.Email,
                                name = $"{u.FirstName} {u.LastName}",
                                role = u.Role.ToString(),
                                createdAt = u.CreatedAt
                            })
                            .Take(5)
                            .ToList<object>();
                    }
                }
                catch (Exception ex) {
                    dbError = ex.Message;
                }
                
                return Ok(new {
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    database = new {
                        connectionString = _configuration.GetConnectionString("DefaultConnection")?.Replace("Password=", "Password=***"),
                        connected = dbConnectionWorks,
                        userCount = userCount,
                        users = users,
                        error = dbError
                    },
                    jwt = new {
                        issuer = _configuration["JwtSettings:Issuer"],
                        audience = _configuration["JwtSettings:Audience"],
                        keyConfigured = !string.IsNullOrEmpty(_configuration["JwtSettings:Key"])
                    }
                });
            }
            catch (Exception ex) {
                return StatusCode(500, new { message = "Erro durante diagnóstico", error = ex.Message });
            }
        }

        [HttpGet("protected-data")]
        [Authorize]
        public ActionResult<object> GetProtectedData()
        {
            try {
                // Obtém o ID do usuário do token JWT
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                // Retorna dados protegidos, incluindo informações do usuário autenticado
                return Ok(new {
                    message = "Você acessou com sucesso um endpoint protegido!",
                    timestamp = DateTime.UtcNow,
                    user = new {
                        id = userId,
                        email = userEmail,
                        name = userName,
                        role = userRole
                    },
                    serverInfo = new {
                        version = "1.0.0",
                        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                        currentDate = DateTime.Now
                    }
                });
            }
            catch (Exception ex) {
                return StatusCode(500, new { message = "Erro ao obter dados protegidos", error = ex.Message });
            }
        }
    }
}
