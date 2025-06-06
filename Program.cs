using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using perenne.Data;
using perenne.Interfaces;
using perenne.Repositories;
using perenne.Services;
using perenne.Websockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

// Configuração das opções JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();


// Database Configuration with URI parsing
string? rawConn = builder.Configuration.GetConnectionString("DefaultConnection")?.Trim()
                  ?? Environment.GetEnvironmentVariable("DATABASE_URL")?.Trim();

if (string.IsNullOrEmpty(rawConn))
{
    Console.WriteLine("WARNING: Database connection string 'DefaultConnection' or 'DATABASE_URL' not found. Using local fallback if in Development.");
    if (builder.Environment.IsDevelopment())
    {
        rawConn = "Host=localhost;Port=5432;Database=perenne_dev_db;Username=postgres;Password=yourlocalpassword;";
    }
    else
    {
        Console.WriteLine("ERROR: Database connection string 'DefaultConnection' or 'DATABASE_URL' not found in Production. Throwing exception.");
        throw new InvalidOperationException("Database connection string not found and not in Development environment.");
    }
}

string npgsqlConn;
if (rawConn.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
 || rawConn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"DEBUG: Detected URI-style connection string: '{rawConn}'");
    try
    {
        var uri = new Uri(rawConn);
        var userInfo = uri.UserInfo.Split(':');
        var dbUser = userInfo.Length > 0 ? userInfo[0] : string.Empty;
        var dbPass = userInfo.Length > 1 ? userInfo[1] : string.Empty;
        var dbHost = uri.Host;
        var dbPort = uri.Port > 0 ? uri.Port : 5432;
        var dbName = uri.AbsolutePath.TrimStart('/');

        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = dbHost,
            Port = dbPort,
            Database = dbName,
            Username = dbUser,
            Password = dbPass,
            SslMode = SslMode.Require
        };
        npgsqlConn = csb.ToString();
        Console.WriteLine($"DEBUG: Converted URI to Npgsql connection string: '{npgsqlConn}'");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: Failed to parse URI connection string '{rawConn}'. Exception: {ex.Message}");
        throw;
    }
}
else
{
    npgsqlConn = rawConn;
    Console.WriteLine($"DEBUG: Using provided key/value connection string: '{npgsqlConn}'");
    if (!npgsqlConn.Contains("localhost", StringComparison.OrdinalIgnoreCase) &&
        !npgsqlConn.Contains("Ssl Mode", StringComparison.OrdinalIgnoreCase) &&
        !npgsqlConn.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("WARNING: Key/value connection string for non-localhost does not explicitly set SslMode. Appending SslMode=Require;TrustServerCertificate=true;");
        npgsqlConn = npgsqlConn.TrimEnd(';') + ";Ssl Mode=Require;Trust Server Certificate=true;";
    }
}

//Console.WriteLine($"DEBUG_FINAL_CONNECTION_STRING_TO_USE: '{npgsqlConn}'");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(npgsqlConn));

// Mais coisa de JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
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

// Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
    options.AddPolicy("ProductionPolicy", policy =>
    {
        var frontendUrl = builder.Configuration["FrontendUrl"];
        if (string.IsNullOrEmpty(frontendUrl))
        {
            Console.WriteLine("WARNING: FrontendUrl not configured for ProductionPolicy. CORS will be very restrictive or fail.");
        } else {
            var allowedOrigins = frontendUrl.Split(',')
                .Concat(["http://localhost:5000", "https://localhost:5000"])
                .ToArray();
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

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

// Message Cache
builder.Services.AddScoped<IMessageCacheService, MessageCacheService>();

// Feed
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IFeedService, FeedService>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Outros Serviços
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddOpenApi();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expiração da sessão
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseForwardedHeaders();

// Migrações
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Attempting to apply database migrations (all environments)...");
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("Applying database migrations...");
            dbContext.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations to apply.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database. Application might not start correctly.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAllOrigins");
} else
{
    app.UseCors("ProductionPolicy");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>(ChatHub.ChatHubPath);
app.MapHealthChecks("/healthz");
app.MapGet("/Error", () => Results.Problem("An unexpected error occurred. Please try again later.", statusCode: 500))
   .ExcludeFromDescription();

//app.UseHttpsRedirection();
//app.MapOpenApi();

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
