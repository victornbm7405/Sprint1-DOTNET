namespace MottuProjeto.Infrastructure;

/// <summary>
/// Regras simples para normalizar paginação (AAA-friendly para testes).
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Normaliza página e tamanho: page>=1, size entre 1 e maxSize (default 100), default size=10.
    /// </summary>
    public static (int page, int size) Clamp(int page, int size, int maxSize = 100)
    {
        var p = page < 1 ? 1 : page;
        var s = size < 1 ? 10 : size;
        if (s > maxSize) s = maxSize;
        return (p, s);
    }
}
