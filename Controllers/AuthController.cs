using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuProjeto.Data;
using MottuProjeto.DTOs;
using MottuProjeto.Models;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public AuthController(AppDbContext ctx) => _ctx = ctx;

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO req)
        {
            var user = await _ctx.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == req.Username);

            if (user is null)
                return Unauthorized(new { message = "Usuário ou senha inválidos." });

            // SIMPLIFICADO: comparando campo PasswordHash como se fosse senha em texto.
            // Troque para BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash) quando salvar hash.
            var ok = user.PasswordHash == req.Password;
            if (!ok)
                return Unauthorized(new { message = "Usuário ou senha inválidos." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                });

            return Ok(new { message = "Login ok", username = user.Username, role = user.Role });
        }

        // POST /api/auth/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logout ok" });
        }

        // GET /api/auth/me
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                user = User.Identity?.Name,
                role = User.FindFirstValue(ClaimTypes.Role)
            });
        }

        [HttpGet("denied")]
        public IActionResult Denied() => Forbid();
    }
}
