using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuProjeto.Data;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [ApiVersionNeutral]
    [Route("")]
    public class HealthController : ControllerBase
    {
        // GET /healthz
        [HttpGet("healthz")]
        [AllowAnonymous]
        public IActionResult Live() => Ok(new
        {
            status = "Healthy",
            service = "MottuProjeto API",
            version = "v1",
            timestamp = DateTime.UtcNow
        });

        // GET /healthz/ready
        [HttpGet("healthz/ready")]
        [AllowAnonymous]
        public async Task<IActionResult> Ready([FromServices] AppDbContext db)
        {
            try
            {
                var ok = await db.Database.CanConnectAsync();
                return ok
                    ? Ok(new { status = "Ready", db = "Connected", timestamp = DateTime.UtcNow })
                    : Problem(statusCode: 503, title: "Not Ready", detail: "Database unreachable");
            }
            catch (Exception ex)
            {
                return Problem(statusCode: 503, title: "Not Ready", detail: ex.Message);
            }
        }
    }
}
