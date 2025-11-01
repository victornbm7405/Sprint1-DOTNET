using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuProjeto.Data;
using MottuProjeto.Models;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsuariosController(AppDbContext context) => _context = context;

        // GET: api/usuarios  |  api/v1/usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> Listar()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/usuarios/5  |  api/v1/usuarios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> Obter(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound();
            return Ok(usuario);
        }

        // POST: api/usuarios  |  api/v1/usuarios
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Obter), new { id = usuario.Id }, usuario);
        }

        // PUT: api/usuarios/5  |  api/v1/usuarios/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Usuario usuario)
        {
            if (id != usuario.Id)
                return BadRequest("Id do recurso diverge do corpo.");

            var existe = await _context.Usuarios.AnyAsync(u => u.Id == id);
            if (!existe) return NotFound();

            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(usuario);
        }

        // DELETE: api/usuarios/5  |  api/v1/usuarios/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
