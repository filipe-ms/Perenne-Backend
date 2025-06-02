// ADICIONADO: Usings para Redis (necessário instalar os pacotes NuGet)
// --- Fim dos usings adicionados para Redis ---

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

// --- Início da Configuração de Pacotes NuGet Necessários ---
// Certifique-se de ter os seguintes pacotes NuGet instalados no seu projeto (.csproj):
// <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="SEU_NET_VERSION_COMPATIVEL" />
// <PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="SEU_NET_VERSION_COMPATIVEL" />
// <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="SEU_NET_VERSION_COMPATIVEL" />
// Substitua SEU_NET_VERSION_COMPATIVEL pela versão apropriada para .NET 9.0 (ex: 8.0.x ou 9.0.x previews)
// --- Fim da Configuração de Pacotes NuGet Necessários ---

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
            // Substitua "https://seu-frontend.onrender.com" pelo domínio real do seu frontend
            // Considere adicionar também o domínio da sua API se o frontend fizer chamadas para ele mesmo de forma diferente
            var frontendUrl = builder.Configuration["FrontendUrl"]; // Ex: https://meufrontend.onrender.com
            if (string.IsNullOrEmpty(frontendUrl))
            {
                Console.WriteLine("AVISO: FrontendUrl não configurado para a política CORS de produção. Permissões podem ser restritas.");
                // Defina um fallback ou lance um erro se for crítico
            }
            else
            {
                policy.WithOrigins(frontendUrl)
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials(); // ADICIONADO: Necessário para SignalR com cookies/autenticação de frontend para backend
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

// Configuração do SignalR e Cache Distribuído com Redis (para produção)
var signalRBuilder = builder.Services.AddSignalR();
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");

if (!string.IsNullOrEmpty(redisConnectionString) && builder.Environment.IsProduction())
{
    Console.WriteLine($"Configurando Redis para SignalR e Cache Distribuído em: {redisConnectionString}");
    try
    {
        // Para SignalR Backplane
        signalRBuilder.AddStackExchangeRedis(redisConnectionString, options =>
        {
            options.Configuration.ChannelPrefix = "PerenneSignalR_"; // Prefixo customizável
        });
        Console.WriteLine("SignalR configurado com backplane Redis.");

        // Para Cache Distribuído (usado por IMessageCacheService ou IDistributedCache)
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "PerenneCache_"; // Prefixo customizável
        });
        Console.WriteLine("Cache distribuído configurado com Redis.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"ERRO CRÍTICO ao configurar Redis: {ex.Message}. A aplicação pode não funcionar como esperado em produção.");
        // Em um cenário real, você pode querer impedir o início da aplicação ou ter um fallback mais robusto.
    }
}
else
{
    Console.WriteLine("SignalR e Cache Distribuído rodando sem Redis (modo de desenvolvimento ou RedisConnection não configurada).");
    builder.Services.AddDistributedMemoryCache(); // Fallback para cache em memória
}

builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer(); // REMOVIDO: Parte do Swagger
// builder.Services.AddSwaggerGen(...); // REMOVIDO: Swagger não utilizado

var app = builder.Build();

app.UseForwardedHeaders(); // Importante para o Render

// REMOVIDO: app.UseHttpsRedirection(); // O Render lida com HTTPS

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // REMOVIDO: Configurações do Swagger UI
    // app.UseSwagger();
    // app.UseSwaggerUI(...);

    // REMOVIDO: Migrações automáticas em desenvolvimento. Faça via 'dotnet ef database update'
    // using (var scope = app.Services.CreateScope())
    // {
    //     var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //     dbContext.Database.Migrate();
    // }
    app.UseCors("AllowAllOrigins");
}
else // Produção
{
    app.UseCors("ProductionPolicy");
    app.UseExceptionHandler("/Error"); // Crie um endpoint/página de erro genérico
    app.UseHsts();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapGet("/healthz", () => Results.Ok("Healthy")); // Health check para o Render

// Configuração da porta para o Render
var portVar = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portVar) && int.TryParse(portVar, out int portNumber))
{
    // Kestrel escutará em http://*:{portNumber} por padrão se PORT for definido.
    // Não é necessário app.Urls.Add() explicitamente na maioria dos casos com .NET 6+
    Console.WriteLine($"Aplicação configurada para escutar na porta: {portNumber} (via variável de ambiente PORT).");
}
else
{
    Console.WriteLine("Variável de ambiente PORT não definida ou inválida. Kestrel usará suas configurações padrão (ex: http://localhost:5000).");
}

app.Run();
