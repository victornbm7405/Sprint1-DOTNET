using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MottuProjeto.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger + JWT (botão Authorize → Bearer <token>)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MottuProjeto API",
        Version = "v1",
        Description = "API de gestão de usuários, motos e áreas com autenticação JWT."
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Informe: Bearer {seu_token_jwt}",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// DbContext (Oracle) — via appsettings.json ou env ORACLE_CONN
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var conn = builder.Configuration.GetConnectionString("Default")
               ?? Environment.GetEnvironmentVariable("ORACLE_CONN");

    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("Connection string Oracle não configurada.");

    opt.UseOracle(conn);
});

// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSection["Key"] ?? "CHAVE_SECRETA_DEV");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,   // simplificado p/ dev
        ValidateAudience = false, // simplificado p/ dev
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// Liveness: API está viva e respondendo
app.MapGet("/healthz", () => Results.Ok(new
{
    status = "Healthy",
    service = "MottuProjeto API",
    version = "v1",
    timestamp = DateTime.UtcNow
}))
.AllowAnonymous()
.WithName("Healthz");

// Readiness: API pronta (checa conexão com o banco)
app.MapGet("/healthz/ready", async (AppDbContext db) =>
{
    try
    {
        var ok = await db.Database.CanConnectAsync();
        return ok
            ? Results.Ok(new
            {
                status = "Ready",
                db = "Connected",
                timestamp = DateTime.UtcNow
            })
            : Results.Problem(
                statusCode: 503,
                title: "Not Ready",
                detail: "Database unreachable"
            );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 503,
            title: "Not Ready",
            detail: ex.Message
        );
    }
})
.AllowAnonymous()
.WithName("Readiness");

app.MapControllers();

// SEED simples (admin/admin123)
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
                PasswordHash = "admin123",
                Role = "Admin"
            });
            ctx.SaveChanges();
        }
    }
    catch { }
}

app.Run();

// Habilita WebApplicationFactory<Program> em testes de integração (xUnit)
public partial class Program { }
