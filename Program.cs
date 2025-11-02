using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MottuProjeto.Data;

// ▼ Imports necessários para VERSIONAMENTO
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();



builder.Services.AddSingleton<MottuProjeto.ML.MotoRiskModelService>();
builder.Services.AddSingleton<MottuProjeto.ML.TelemetryRiskService>();
// Swagger + JWT (botão Authorize → Bearer <token>)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // ⚠️ Mantemos apenas a configuração de segurança aqui.
    // A definição dos "SwaggerDoc" por versão será feita via ConfigureSwaggerOptions (abaixo).
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

// ▼ VERSIONAMENTO DE API
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;

    // Permite 3 formas de enviar versão:
    // - Segmento na URL (/api/v{version}/...)
    // - Header: x-api-version
    // - QueryString: ?api-version=1.0
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new QueryStringApiVersionReader("api-version")
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";        // v1, v1.1, ...
    options.SubstituteApiVersionInUrl = true;  // substitui {version} quando presente na rota
});

// Gera Swagger por versão descoberta pelo ApiExplorer
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

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

// Health Checks (registrado; os endpoints estão no HealthController)
builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger
app.UseSwagger();

// UI do Swagger com suporte a múltiplas versões
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwaggerUI(options =>
{
    foreach (var desc in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
            $"MottuProjeto API {desc.GroupName.ToUpper()}");
    }
});

app.UseAuthentication();
app.UseAuthorization();

// ⛳ Endpoints de health **removidos** daqui para aparecerem no Swagger via HealthController
// (mantém as rotas /healthz e /healthz/ready no controller com [ApiVersionNeutral])

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

// ▼ Classe para configurar Swagger dinamicamente por versão
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var desc in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo
            {
                Title = "MottuProjeto API",
                Version = desc.ApiVersion.ToString(),
                Description = "API de gestão de usuários, motos e áreas com autenticação JWT, versionamento e health checks."
            });
        }
    }
}


