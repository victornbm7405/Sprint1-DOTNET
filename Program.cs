// Program.cs
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;                 // SameSiteMode / CookieSecurePolicy
using Microsoft.Extensions.Hosting;              // IHostEnvironment
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;             // UseOracle
using MottuProjeto.Data;                         // AppDbContext

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (Oracle)
// Connection string lida de ConnectionStrings:Default (appsettings*.json) ou variáveis de ambiente
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var conn = builder.Configuration.GetConnectionString("Default")
               ?? Environment.GetEnvironmentVariable("ORACLE_CONN");
    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException(
            "Defina ConnectionStrings:Default no appsettings ou a variável de ambiente ORACLE_CONN.");

    opt.UseOracle(conn);
});

// Autenticação via Cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthCookie";
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;            // ok para HTTP local
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // em produção (HTTPS), use Always
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SEED opcional de admin (apenas se a base estiver vazia)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!ctx.Usuarios.Any())
        {
            ctx.Usuarios.Add(new MottuProjeto.Models.Usuario
            {
                Nome = "Administrador",
                Email = "admin@example.com",
                Username = "admin",
                PasswordHash = "admin123", // ⚠️ TROQUE para hash (BCrypt) e ajuste a verificação no login
                Role = "Admin"
            });
            ctx.SaveChanges();
        }
    }
    catch
    {
        // Se o DbContext não estiver configurado, ignore o seed.
    }
}

app.Run();
