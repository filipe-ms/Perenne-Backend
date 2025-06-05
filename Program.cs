using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using perenne.Data;
using perenne.Interfaces;
using perenne.Repositories;
using perenne.Services;
using perenne.Websockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração das opções JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();    // Para permitir injetar HttpContext
builder.Services.AddDistributedMemoryCache(); // Cache para armazenar sessões
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expiração da sessão
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true) // Permite qualquer origem
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Importante para cookies e autenticação
    });
});

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments(ChatHub.ChatHubPath))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Guest
builder.Services.AddScoped<IGuestService, GuestService>();

// User
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Group
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupService, GroupService>();

// Chat
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

// Chat Cache
builder.Services.AddScoped<IMessageCacheService, MessageCacheService>();

// Feed
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IFeedService, FeedService>();

// Email Service (Exemplo - você precisará criar esta interface e implementação)
// builder.Services.AddTransient<IEmailService, EmailService>(); // Descomente e implemente

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Middleware pipeline
app.UseSession();
app.UseCors("AllowAll");
// app.UseHttpsRedirection(); // Comentado para evitar problemas com CORS

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>(ChatHub.ChatHubPath);
app.MapControllers();

app.Urls.Add("http://0.0.0.0:5000");


// Iniciando o cache de mensagens

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var messageCacheService = services.GetRequiredService<IMessageCacheService>();
        await messageCacheService.InitMessageCacheServiceAsync();
        Console.WriteLine("Cache de mensagens inicializado com sucesso!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao tentar inicializar o cache de mensagens.");
    }
}

app.Run();

// Recuperação de senha

// Exemplo de Interface e Serviço de Email (Simplificado)
// Crie arquivos IEmailService.cs e EmailService.cs
/*
// IEmailService.cs
namespace perenne.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string token, string resetLink);
    }
}

// EmailService.cs
using System.Net;
using System.Net.Mail;
using perenne.Interfaces; // Se estiver em outro namespace

namespace perenne.Services
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        private readonly IConfiguration _configuration = configuration;

        public async Task SendPasswordResetEmailAsync(string toEmail, string token, string resetLink)
        {
            // Obtenha as configurações do appsettings.json
            var smtpHost = _configuration["SmtpSettings:Host"];
            var smtpPort = int.Parse(_configuration["SmtpSettings:Port"]);
            var smtpUser = _configuration["SmtpSettings:Username"];
            var smtpPass = _configuration["SmtpSettings:Password"];
            var fromEmail = _configuration["SmtpSettings:FromEmail"];


            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass) || string.IsNullOrEmpty(fromEmail))
            {
                Console.WriteLine("Configurações de SMTP não encontradas. O e-mail não será enviado.");
                // Em um cenário real, você poderia lançar uma exceção ou logar isso de forma mais robusta.
                return;
            }

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true; // Ou false, dependendo do seu servidor SMTP

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = "Redefinição de Senha",
                    Body = $"<p>Olá,</p><p>Você solicitou a redefinição da sua senha. Clique no link abaixo para prosseguir:</p><p><a href='{resetLink}'>{resetLink}</a></p><p>Este link é válido por 1 hora.</p><p>Se você não solicitou esta alteração, ignore este e-mail.</p>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                try
                {
                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine($"E-mail de redefinição enviado para {toEmail}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    // Trate o erro apropriadamente
                }
            }
        }
    }
}
*/