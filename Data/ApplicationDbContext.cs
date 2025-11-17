using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Models;
using AutoMarket.Models.Enums;

namespace AutoMarket.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar StatusAprovacao para ser armazenado como string no banco de dados
            builder.Entity<Utilizador>()
                .Property(u => u.StatusAprovacao)
                .HasConversion<string>();
        }
    }
}











