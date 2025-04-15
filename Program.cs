using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Repositories;
using perenne.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// DB Connection
builder.Services.AddDbContext<ApplicationDbContext>( options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")) );

// User
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) 
{ 
    app.MapOpenApi(); 
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
