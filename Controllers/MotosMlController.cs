using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MottuProjeto.ML;
using System.ComponentModel.DataAnnotations;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/ml/motos")]
    public class MotosMlController : ControllerBase
    {
        private readonly MotoRiskModelService _svc;
        public MotosMlController(MotoRiskModelService svc) => _svc = svc;

        /// <summary>Prediz risco de manutenção da moto com base na telemetria.</summary>
        /// <remarks>
        /// Exemplo:
        /// POST /api/v1/ml/motos/risco
        /// { "tempC": 61.5, "vib": 0.38, "battPct": 52 }
        /// </remarks>
        [HttpPost("risco")]
        [Authorize] // mantenha protegido p/ fechar requisito de segurança
        [ProducesResponseType(typeof(TelemetryResponse), StatusCodes.Status200OK)]
        public IActionResult PreverRisco([FromBody, Required] TelemetryRequest req)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var res = _svc.Predict(req.TempC, req.Vib, req.BattPct);
            return Ok(res);
        }
    }
}
