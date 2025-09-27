using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuProjeto.Data;
using MottuProjeto.DTOs;
using MottuProjeto.Models;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsuariosController(AppDbContext context) => _context = context;

        /// <summary>Lista todos os usu�rios.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> Get()
            => Ok(await _context.Usuarios.AsNoTracking().ToListAsync());

        /// <summary>Obt�m um usu�rio pelo ID.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> GetById(int id)
        {
            var u = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return u is null ? NotFound(new { message = "Usu�rio n�o encontrado." }) : Ok(u);
        }

        /// <summary>Cadastra um novo usu�rio.</summary>
        [HttpPost]
        public async Task<ActionResult<Usuario>> Post([FromBody] UsuarioDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Regra simples: e-mail �nico
            var emailEmUso = await _context.Usuarios.AsNoTracking().AnyAsync(x => x.Email == dto.Email);
            if (emailEmUso) return Conflict(new { message = "J� existe um usu�rio com esse e-mail." });

            var u = new Usuario { Nome = dto.Nome, Email = dto.Email };
            _context.Usuarios.Add(u);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = u.Id }, u);
        }

        /// <summary>Atualiza um usu�rio existente.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] UsuarioDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound(new { message = "Usu�rio n�o encontrado." });

            var emailEmUso = await _context.Usuarios.AsNoTracking()
                .AnyAsync(x => x.Email == dto.Email && x.Id != id);
            if (emailEmUso) return Conflict(new { message = "J� existe outro usu�rio com esse e-mail." });

            u.Nome = dto.Nome;
            u.Email = dto.Email;
            await _context.SaveChangesAsync();
            return Ok(u);
        }

        /// <summary>Exclui um usu�rio.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u is null) return NotFound(new { message = "Usu�rio n�o encontrado." });

            _context.Usuarios.Remove(u);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
