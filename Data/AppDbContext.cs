using Microsoft.EntityFrameworkCore;
using MottuProjeto.Models;

namespace MottuProjeto.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Moto> Motos { get; set; }
        public DbSet<Area> Areas { get; set; }
    }
}
