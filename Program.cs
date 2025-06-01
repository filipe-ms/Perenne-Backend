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