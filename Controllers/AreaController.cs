using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuProjeto.Data;
using MottuProjeto.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class AreaController : ControllerBase
    {
        private readonly AppDbContext _ctx;

        public AreaController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        // GET: api/area  |  api/v1/area
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Area>>> GetAll()
        {
            var list = await _ctx.Areas.AsNoTracking().ToListAsync();
            return Ok(list);
        }

        // GET: api/area/5  |  api/v1/area/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Area>> GetById(int id)
        {
            var area = await _ctx.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (area == null)
                return NotFound();
            return Ok(area);
        }

        // POST: api/area  |  api/v1/area
        [HttpPost]
        public async Task<ActionResult<Area>> Create([FromBody] Area area)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _ctx.Areas.Add(area);
            await _ctx.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = area.Id }, area);
        }

        // PUT: api/area/5  |  api/v1/area/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Area area)
        {
            if (id != area.Id)
                return BadRequest("Id da rota difere do corpo");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _ctx.Entry(area).State = EntityState.Modified;

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _ctx.Areas.AnyAsync(a => a.Id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // DELETE: api/area/5  |  api/v1/area/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var area = await _ctx.Areas.FindAsync(id);
            if (area == null)
                return NotFound();

            _ctx.Areas.Remove(area);
            await _ctx.SaveChangesAsync();

            return NoContent();
        }
    }
}
