using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuProjeto.Models;
using MottuProjeto.Data;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AreasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AreasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CriarArea([FromBody] Area area)
        {
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CriarArea), new { id = area.Id }, area);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Area>>> ListarAreas()
        {
            return await _context.Areas.ToListAsync();
        }
    }
}
