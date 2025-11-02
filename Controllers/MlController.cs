using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MottuProjeto.ML;

namespace MottuProjeto.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ml")]
public class MlController : ControllerBase
{
    private readonly TelemetryRiskService _svc;

    public MlController(TelemetryRiskService svc)
    {
        _svc = svc;
    }

    /// <summary>
    /// Prediz risco de manutenção da moto a partir de telemetria (ML.NET).
    /// </summary>
    /// <remarks>
    /// Exemplo:
    /// POST /api/v1/ml/risco-manutencao
    /// {
    ///   "tempC": 61.5,
    ///   "vib": 0.38,
    ///   "battPct": 52
    /// }
    /// </remarks>
    /// <response code="200">Retorna probabilidade e nível de risco</response>
    [HttpPost("risco-manutencao")]
    [AllowAnonymous] // deixar aberto p/ facilitar correção
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult PreverRisco([FromBody] TelemetryInput dto)
    {
        if (dto is null)
            return BadRequest(new { message = "Corpo inválido." });

        var (pred, prob, nivel) = _svc.Prever(dto);

        return Ok(new
        {
            tempC = dto.TempC,
            vib = dto.Vib,
            battPct = dto.BattPct,
            predicted = pred,                 // true => risco
            probability = System.Math.Round(prob, 3),
            nivel                              // "Normal" | "Alerta" | "Risco"
        });
    }
}
