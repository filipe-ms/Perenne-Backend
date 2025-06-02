using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using perenne.Data;
using perenne.Interfaces;
using perenne.Repositories;
using perenne.Services;
using perenne.Websockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key) || jwtSettings.Key == "DefaultKey_MustBeOverriddenInProduction")
{
    Console.WriteLine("WARNING: JWT Key is not configured or using default. THIS IS INSECURE FOR PRODUCTION.");
    if (jwtSettings == null) jwtSettings = new JwtSettings();
    if (string.IsNullOrEmpty(jwtSettings.Key)) jwtSettings.Key = "SuperSecretKeyThatIsAtLeast32BytesLongForHS256_ReplaceInConfig";
}
builder.Services.AddSingleton(jwtSettings); // Or Scoped/Transient as appropriate

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("WARNING: Database connection string 'DefaultConnection' not found. Using local fallback if in Development.");
    if (builder.Environment.IsDevelopment())
    {
        connectionString = "Host=localhost;Port=5432;Database=perenne_dev_db;Username=postgres;Password=yourlocalpassword;";
    }
    else
    {
        Console.WriteLine("ERROR: Database connection string 'DefaultConnection' not found in Production. Lançando exceção.");
        throw new InvalidOperationException("Database connection string 'DefaultConnection' not found and not in Development environment.");
    }
}

Console.WriteLine($"DEBUG_CONNECTION_STRING_FINAL: String de conexão a ser usada por UseNpgsql: '{connectionString}'");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

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
            Console.WriteLine("WARNING: FrontendUrl not configured for ProductionPolicy. CORS might be too open or fail.");
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials(); // Fallback, less secure
        }
        else
        {
            policy.WithOrigins(frontendUrl.Split(',')).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
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

// Chat Cache
builder.Services.AddScoped<IMessageCacheService, MessageCacheService>();

// Feed
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IFeedService, FeedService>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAllOrigins");

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>(); // Sua classe ApplicationDbContext
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying database migrations...");
            dbContext.Database.Migrate(); // Aplica as migrações pendentes
            Console.WriteLine("Database migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("No pending migrations to apply.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}
else // Production
{
    app.UseCors("ProductionPolicy");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Map your actual ChatHub from perenne.Websockets namespace using its defined path
app.MapHub<ChatHub>(ChatHub.ChatHubPath);
app.MapHealthChecks("/healthz");
app.MapGet("/Error", () => Results.Problem("An error occurred.", statusCode: 500));

app.Run();
