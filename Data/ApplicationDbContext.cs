using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Models;
using AutoMarket.Models.Enums;

namespace AutoMarket.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationDbContext"/> using the provided options.
        /// </summary>
        /// <param name="options">The options used to configure the database context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Comprador> Compradores { get; set; }

        //Futuramente, quando criar carro:
        //public DbSet<Carro> Carros { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar StatusAprovacao para ser armazenado como string no banco de dados
            builder.Entity<Utilizador>()
                .Property(u => u.StatusAprovacao) // o meu antigo status de aprovação
                .HasConversion<string>();
        }
    }
}






