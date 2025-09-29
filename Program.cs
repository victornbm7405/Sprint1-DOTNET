// Program.cs
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Authorization; // AllowAnonymousFilter
using MottuProjeto.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ===== Controllers (força todo mundo como [AllowAnonymous]) =====
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AllowAnonymousFilter());
});

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MottuProjeto API (Sem Auth)",
        Version = "v1",
        Description = "Somente Motos e Áreas. Rotas de Usuário/Auth desativadas."
    });

    var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// ===== DB =====
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Usa a sua chave "Default" do appsettings.json
    var cs = builder.Configuration.GetConnectionString("Default");
    if (!string.IsNullOrWhiteSpace(cs))
        options.UseOracle(cs);
});

// ===== CORS (qualquer origem, sem cookies) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenAll", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

// ===== Pipeline =====
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("OpenAll");

// Bloqueia rotas de usuário e auth (404)
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api/usuarios") ||
        ctx.Request.Path.StartsWithSegments("/api/auth"))
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        await ctx.Response.WriteAsync("Not Found");
        return;
    }
    await next();
});

// IMPORTANTE: não usamos UseAuthentication/UseAuthorization
app.MapControllers();

// (Opcional) Seed silencioso — não é usado sem auth, pode remover se quiser
using (var scope = app.Services.CreateScope())
{
    try
    {
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!ctx.Usuarios.Any())
        {
            ctx.Usuarios.Add(new MottuProjeto.Models.Usuario
            {
                Nome = "Administrador do Sistema",
                Email = "admin@example.com",
                Username = "admin",
                PasswordHash = "admin123",
                Role = "Admin"
            });
            ctx.SaveChanges();
        }
    }
    catch { /* ignora erros de seed */ }
}

app.Run();
