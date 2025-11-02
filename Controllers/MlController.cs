using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MottuProjeto.ML;
using System.ComponentModel.DataAnnotations;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/ml")]
    public class MlController : ControllerBase
    {
        private readonly TelemetryRiskService _service;

        public MlController(TelemetryRiskService service)
        {
            _service = service;
        }

        /// <summary>
        /// Prediz o risco de manutenção com base em temperatura (°C), vibração e bateria (%).
        /// </summary>
        [HttpPost("risco-manutencao")] // 🔄 endpoint alterado
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(TelemetryResponse), StatusCodes.Status200OK)]
        public ActionResult<TelemetryResponse> RiscoManutencao([FromBody, Required] TelemetryRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var result = _service.Predict(request);
            return Ok(result);
        }
    }
}
