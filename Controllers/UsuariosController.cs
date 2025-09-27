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

        /// <summary>Lista todos os usuários.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> Get()
            => Ok(await _context.Usuarios.AsNoTracking().ToListAsync());

        /// <summary>Obtém um usuário pelo ID.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> GetById(int id)
        {
            var u = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return u is null ? NotFound(new { message = "Usuário não encontrado." }) : Ok(u);
        }

        /// <summary>Cadastra um novo usuário.</summary>
        [HttpPost]
        public async Task<ActionResult<Usuario>> Post([FromBody] UsuarioDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Regra simples: e-mail único
            var emailEmUso = await _context.Usuarios.AsNoTracking().AnyAsync(x => x.Email == dto.Email);
            if (emailEmUso) return Conflict(new { message = "Já existe um usuário com esse e-mail." });

            var u = new Usuario { Nome = dto.Nome, Email = dto.Email };
            _context.Usuarios.Add(u);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = u.Id }, u);
        }

        /// <summary>Atualiza um usuário existente.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] UsuarioDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound(new { message = "Usuário não encontrado." });

            var emailEmUso = await _context.Usuarios.AsNoTracking()
                .AnyAsync(x => x.Email == dto.Email && x.Id != id);
            if (emailEmUso) return Conflict(new { message = "Já existe outro usuário com esse e-mail." });

            u.Nome = dto.Nome;
            u.Email = dto.Email;
            await _context.SaveChangesAsync();
            return Ok(u);
        }

        /// <summary>Exclui um usuário.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u is null) return NotFound(new { message = "Usuário não encontrado." });

            _context.Usuarios.Remove(u);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
