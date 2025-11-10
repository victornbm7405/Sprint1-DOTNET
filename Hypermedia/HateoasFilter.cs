using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace MottuProjeto.Hypermedia;

/// <summary>
/// Filtro global que injeta links HATEOAS em respostas 2xx contendo objetos/coleções.
/// Ajuste os nomes de controllers/actions se os seus forem diferentes.
/// </summary>
public class HateoasFilter : IAsyncActionFilter
{
    private readonly LinkGenerator _linkGen;

    public HateoasFilter(LinkGenerator linkGen) => _linkGen = linkGen;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        if (executed.Result is ObjectResult obj &&
            obj.Value is not null &&
            (obj.StatusCode is null || (obj.StatusCode >= 200 && obj.StatusCode < 300)))
        {
            obj.Value = Wrap(obj.Value, context);
        }
    }

    private object Wrap(object value, ActionExecutingContext ctx)
    {
        if (value is IEnumerable<object> list)
        {
            var wrapped = new List<object>();
            foreach (var item in list)
                wrapped.Add(WrapSingle(item, ctx));
            return wrapped;
        }
        return WrapSingle(value, ctx);
    }

    private object WrapSingle(object entity, ActionExecutingContext ctx)
    {
        var type = entity.GetType();

        // Não re-empacota se já for Resource<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Resource<>))
            return entity;

        var resourceGeneric = typeof(Resource<>).MakeGenericType(type);
        var resource = Activator.CreateInstance(resourceGeneric, entity)!;

        var linksProp = resourceGeneric.GetProperty("Links")!;
        var links = (List<Link>)linksProp.GetValue(resource)!;

        var id = type.GetProperty("Id")?.GetValue(entity);

        string? self = null, put = null, del = null;

        // 🔧 Ajuste se o nome dos seus controllers for diferente
        if (type.Name.Contains("Moto", StringComparison.OrdinalIgnoreCase))
        {
            self = _linkGen.GetPathByAction(ctx.HttpContext, "GetById", "Motos", new { version = "1", id });
            put = _linkGen.GetPathByAction(ctx.HttpContext, "Update", "Motos", new { version = "1", id });
            del = _linkGen.GetPathByAction(ctx.HttpContext, "Delete", "Motos", new { version = "1", id });
        }
        else if (type.Name.Contains("Area", StringComparison.OrdinalIgnoreCase))
        {
            self = _linkGen.GetPathByAction(ctx.HttpContext, "GetById", "Area", new { version = "1", id });
            put = _linkGen.GetPathByAction(ctx.HttpContext, "Update", "Area", new { version = "1", id });
            del = _linkGen.GetPathByAction(ctx.HttpContext, "Delete", "Area", new { version = "1", id });
        }
        else if (type.Name.Contains("Usuario", StringComparison.OrdinalIgnoreCase))
        {
            self = _linkGen.GetPathByAction(ctx.HttpContext, "GetById", "Usuarios", new { version = "1", id });
            put = _linkGen.GetPathByAction(ctx.HttpContext, "Update", "Usuarios", new { version = "1", id });
            del = _linkGen.GetPathByAction(ctx.HttpContext, "Delete", "Usuarios", new { version = "1", id });
        }

        void add(string rel, string? href, string method)
        {
            if (!string.IsNullOrWhiteSpace(href))
                links.Add(new Link { Rel = rel, Href = href!, Method = method });
        }

        add("self", self, "GET");
        add("update", put, "PUT");
        add("delete", del, "DELETE");

        return resource;
    }
}
