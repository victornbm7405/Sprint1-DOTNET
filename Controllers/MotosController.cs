using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuProjeto.Data;
using MottuProjeto.Models;
using MottuProjeto.DTOs;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MotosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Moto>>> GetMotos()
        {
            return await _context.Motos.ToListAsync();
        }

        [HttpGet("byarea")]
        public async Task<ActionResult<IEnumerable<Moto>>> GetMotosPorArea([FromQuery] int idArea)
        {
            return await _context.Motos.Where(m => m.IdArea == idArea).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Moto>> PostMoto([FromBody] MotoDTO dto)
        {
            var novaMoto = new Moto
            {
                Placa = dto.Placa,
                Modelo = dto.Modelo,
                IdArea = dto.IdArea
            };

            _context.Motos.Add(novaMoto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMotos), new { id = novaMoto.Id }, novaMoto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMoto(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarMoto(int id, [FromBody] MotoDTO dto)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();

            moto.Placa = dto.Placa;
            moto.Modelo = dto.Modelo;
            moto.IdArea = dto.IdArea;

            await _context.SaveChangesAsync();

            return Ok(moto);
        }
    }
}
