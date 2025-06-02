using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using perenne.Data;
using perenne.Interfaces;
using perenne.Models;
using perenne.Repositories;
using perenne.Services;
using perenne.Websockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Configuração do JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("Configurações JWT (JwtSettings) não encontradas ou inválidas no appsettings.json ou variáveis de ambiente.");
}
if (string.IsNullOrEmpty(jwtSettings.Key))
{
    throw new InvalidOperationException("A chave JWT (JwtSettings:Key) não pode ser nula ou vazia.");
}
builder.Services.AddSingleton(jwtSettings);

// Configuração do Entity Framework Core com PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("String de conexão 'DefaultConnection' para o PostgreSQL não encontrada.");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); // ALTERADO: Para usar PostgreSQL

// Configuração do Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configuração da Autenticação JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

// Configuração de Forwarded Headers para o Render
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", // Política para desenvolvimento
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });

    options.AddPolicy("ProductionPolicy", // Política para produção no Render
        policy =>
        {
            var frontendUrl = builder.Configuration["FrontendUrl"]; // Ex: https://meufrontend.onrender.com
            if (string.IsNullOrEmpty(frontendUrl))
            {
                Console.WriteLine("AVISO: FrontendUrl não configurado para a política CORS de produção. Usando fallback para permitir qualquer origem com credenciais (RESTRinja ISSO!).");
                policy.AllowAnyOrigin() // ATENÇÃO: Fallback perigoso para produção sem FrontendUrl definido
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials();
            }
            else
            {
                policy.WithOrigins(frontendUrl.Split(',')) // Permite múltiplas URLs separadas por vírgula
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials(); // Necessário para SignalR com cookies/autenticação
            }
        });
});

// Configuração dos Serviços da Aplicação
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddSingleton<IMessageCacheService, MessageCacheService>();

// Configuração do SignalR
var signalRBuilder = builder.Services.AddSignalR();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddControllers();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAllOrigins");
}
else // Produção
{
    app.UseCors("ProductionPolicy");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapGet("/healthz", () => Results.Ok("Healthy"));

app.Run();
