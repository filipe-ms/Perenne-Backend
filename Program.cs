using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using perenne.Data;
using perenne.Websockets;
using System.Text;

// Removed the placeholder User : IdentityUser class as you provided User.cs
// Removed the placeholder ApplicationDbContext as you provided ApplicationDbContext.cs

// Define placeholder JwtSettings class if not provided elsewhere in your models
// This is to make the example runnable. Replace with your actual class if you have one.
// Placeholder for ChatHub removed as you provided perenne.Websockets.ChatHub

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on the port provided by Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// --- JWT Configuration ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key) || jwtSettings.Key == "DefaultKey_MustBeOverriddenInProduction")
{
    Console.WriteLine("WARNING: JWT Key is not configured or using default. THIS IS INSECURE FOR PRODUCTION.");
    if (jwtSettings == null) jwtSettings = new JwtSettings();
    if (string.IsNullOrEmpty(jwtSettings.Key)) jwtSettings.Key = "SuperSecretKeyThatIsAtLeast32BytesLongForHS256_ReplaceInConfig";
}
builder.Services.AddSingleton(jwtSettings); // Or Scoped/Transient as appropriate

// --- Database Configuration (PostgreSQL) ---
// Uses your perenne.Data.ApplicationDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("WARNING: Database connection string 'DefaultConnection' not found. Using local fallback if in Development.");
    if (builder.Environment.IsDevelopment())
    {
        // IMPORTANT: Replace with your actual local development connection string
        connectionString = "Host=localhost;Port=5432;Database=perenne_dev_db;Username=postgres;Password=yourlocalpassword;";
    }
    else
    {
        throw new InvalidOperationException("Database connection string 'DefaultConnection' not found and not in Development environment.");
    }
}
builder.Services.AddDbContext<ApplicationDbContext>(options => // This is your perenne.Data.ApplicationDbContext
    options.UseNpgsql(connectionString));

// --- ASP.NET Core Identity NOT USED directly with perenne.Models.User ---
// The following lines for AddIdentity are removed/commented out because
// your perenne.Models.User does not inherit from IdentityUser and
// your ApplicationDbContext does not inherit from IdentityDbContext.
// You will need to implement custom user management (password hashing, user creation, role checks)
// in your service layer.

// builder.Services.AddIdentity<User, IdentityRole>() // User here would need to be an IdentityUser
//     .AddEntityFrameworkStores<ApplicationDbContext>() // ApplicationDbContext would need to be an IdentityDbContext
//     .AddDefaultTokenProviders();

// --- Authentication (JWT Bearer) ---
// This setup assumes you will manually create JWTs upon successful custom login
// and include necessary claims (like user ID and role from your UserRole enum).
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

// --- Forwarded Headers for Reverse Proxy (Render) ---
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// --- CORS Configuration ---
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

// --- Application Services (Repositories, Services, etc.) ---
// Register your application-specific services that will handle logic like
// user creation (with password hashing), login, group management, etc.
// Example:
// builder.Services.AddScoped<perenne.Interfaces.IUserService, perenne.Services.UserService>(); // Your custom UserService
// builder.Services.AddScoped<perenne.Interfaces.ITokenService, perenne.Services.TokenService>(); // Your custom service to generate JWTs
// ... register your other services from perenne.Services, perenne.Repositories ...
// Example for services used by ChatHub:
// builder.Services.AddScoped<perenne.Interfaces.IUserService, perenne.Services.UserService>();
// builder.Services.AddScoped<perenne.Interfaces.IGroupService, perenne.Services.GroupService>();
// builder.Services.AddSingleton<perenne.Interfaces.IMessageCacheService, perenne.Services.MessageCacheService>(); // Or Scoped/Transient


// --- SignalR Configuration ---
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
}
);
builder.Services.AddDistributedMemoryCache();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

// --- HTTP Request Pipeline ---
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAllOrigins");
    // Optional: Apply migrations on startup for dev convenience.
    // Ensure your ApplicationDbContext is correctly configured for migrations.
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // Your perenne.Data.ApplicationDbContext
        try
        {
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                Console.WriteLine("Applying database migrations (Development)...");
                dbContext.Database.Migrate();
                Console.WriteLine("Database migrations applied successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
        }
    }
}
else // Production
{
    app.UseCors("ProductionPolicy");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();

// Authentication middleware must come before Authorization.
app.UseAuthentication(); // This will use the JWT Bearer handler.
app.UseAuthorization();  // Authorization policies can be set up based on claims in the JWT.

app.MapControllers();
// Map your actual ChatHub from perenne.Websockets namespace using its defined path
app.MapHub<ChatHub>(ChatHub.ChatHubPath);
app.MapHealthChecks("/healthz");
app.MapGet("/Error", () => Results.Problem("An error occurred.", statusCode: 500));

app.Run();
