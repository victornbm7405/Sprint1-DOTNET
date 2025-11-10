using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MottuProjeto.Swagger;

/// <summary>
/// Preenche Summary/Description e respostas padrão automaticamente
/// para deixar o Swagger “bem explícito” sem editar seus controllers.
/// </summary>
public class ExplicitDocsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var http = (context.ApiDescription.HttpMethod ?? "GET").ToUpperInvariant();
        var action = context.ApiDescription.ActionDescriptor.RouteValues.TryGetValue("action", out var a) ? a : string.Empty;
        var route = context.ApiDescription.RelativePath ?? string.Empty;
        var res = ExtractResourceFromRoute(route);

        operation.Summary ??= BuildSummary(http, res, action);
        operation.Description ??= BuildDescription(http, res, action);

        Ensure(operation, "400", "Requisição inválida.");
        if (http != "GET") Ensure(operation, "401", "Não autorizado.");

        if (IsGetById(action, route))
        {
            Ensure(operation, "200", $"Detalhes de {Singular(res)} retornados.");
            Ensure(operation, "404", $"{Singular(res)} não encontrado.");
        }
        else if (http == "GET")
        {
            Ensure(operation, "200", $"Lista de {res} retornada.");
        }
        else if (http == "POST")
        {
            Ensure(operation, "201", $"{Singular(res)} criado.");
        }
        else if (http is "PUT" or "PATCH")
        {
            Ensure(operation, "200", $"{Singular(res)} atualizado.");
            Ensure(operation, "404", $"{Singular(res)} não encontrado.");
        }
        else if (http == "DELETE")
        {
            Ensure(operation, "204", $"{Singular(res)} removido.");
            Ensure(operation, "404", $"{Singular(res)} não encontrado.");
        }
    }

    private static string ExtractResourceFromRoute(string? route)
    {
        var parts = (route ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (p.Equals("api", StringComparison.OrdinalIgnoreCase)) continue;
            if (p.StartsWith("v", StringComparison.OrdinalIgnoreCase)) continue;
            if (p.StartsWith("{")) continue;
            return p;
        }
        return "recursos";
    }

    private static bool IsGetById(string? action, string? route)
    {
        var a = action ?? string.Empty;
        var r = route ?? string.Empty;
        return r.Contains("{id", StringComparison.OrdinalIgnoreCase)
            || a.Contains("ById", StringComparison.OrdinalIgnoreCase);
    }

    private static string Singular(string s) =>
        (!string.IsNullOrEmpty(s) && s.EndsWith("s", StringComparison.OrdinalIgnoreCase)) ? s[..^1] : s;

    private static string BuildSummary(string method, string res, string? action) => method switch
    {
        "GET" when (action ?? string.Empty).Contains("ById", StringComparison.OrdinalIgnoreCase) => $"Detalhar {Singular(res)}",
        "GET" => $"Listar {res}",
        "POST" => $"Criar {Singular(res)}",
        "PUT" => $"Atualizar {Singular(res)}",
        "PATCH" => $"Atualizar {Singular(res)}",
        "DELETE" => $"Excluir {Singular(res)}",
        _ => $"{method} {res}"
    };

    private static string BuildDescription(string method, string res, string? action) => method switch
    {
        "GET" when (action ?? string.Empty).Contains("ById", StringComparison.OrdinalIgnoreCase)
            => $"**O que faz:** retorna os dados de um(a) {Singular(res)} pelo Id.",
        "GET" => $"**O que faz:** retorna a lista de {res}.",
        "POST" => $"**O que faz:** cria um(a) {Singular(res)} e retorna o recurso.",
        "PUT" => $"**O que faz:** atualiza um(a) {Singular(res)} existente.",
        "PATCH" => $"**O que faz:** atualiza parcialmente um(a) {Singular(res)}.",
        "DELETE" => $"**O que faz:** remove um(a) {Singular(res)} pelo Id.",
        _ => $"Endpoint {method} para {res}."
    };

    private static void Ensure(OpenApiOperation op, string code, string desc)
    {
        if (op.Responses.TryGetValue(code, out var resp))
            resp.Description = desc;
        else
            op.Responses[code] = new OpenApiResponse { Description = desc };
    }
}
