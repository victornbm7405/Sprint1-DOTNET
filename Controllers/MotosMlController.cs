using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MottuProjeto.ML;

namespace MottuProjeto.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ml/motos")]
public class MotosMlController : ControllerBase
{
    private readonly MotoRiskModelService _svc;

    public MotosMlController(MotoRiskModelService svc)
    {
        _svc = svc;
    }

    /// <summary>
    /// Prediz risco de manutenção da moto com base na telemetria (ML.NET treinado via JSON).
    /// </summary>
    /// <remarks>
    /// Exemplo:
    /// POST /api/v1/ml/motos/risco
    /// {
    ///   "tempC": 61.5,
    ///   "vib": 0.38,
    ///   "battPct": 52
    /// }
    /// </remarks>
    /// <response code="200">Retorna probabilidade e nível de risco</response>
    [HttpPost("risco")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult PreverRisco([FromBody] MotoInput dto)
    {
        if (dto is null) return BadRequest(new { message = "Corpo inválido." });

        var (pred, prob, nivel) = _svc.Prever(dto);

        return Ok(new
        {
            tempC = dto.TempC,
            vib = dto.Vib,
            battPct = dto.BattPct,
            predicted = pred,
            probability = System.Math.Round(prob, 3),
            nivel
        });
    }
}
