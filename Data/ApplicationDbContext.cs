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

        /// <summary>
        /// DbSet para a entidade Veiculo.
        /// </summary>
        public DbSet<Veiculo> Veiculos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar StatusAprovacao para ser armazenado como string no banco de dados
            builder.Entity<Utilizador>()
                .Property(u => u.StatusAprovacao)
                .HasConversion<string>();

            // Configurar EstadoVeiculo como string
            builder.Entity<Veiculo>()
                .Property(v => v.Estado)
                .HasConversion<string>();

            // Configurar índice único para Matrícula
            builder.Entity<Veiculo>()
                .HasIndex(v => v.Matricula)
                .IsUnique()
                .HasDatabaseName("IX_Veiculo_Matricula_Unique");

            // Configurar relacionamento Utilizador -> Veiculo
            builder.Entity<Veiculo>()
                .HasOne(v => v.Vendedor)
                .WithMany()
                .HasForeignKey(v => v.VendedorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para performance
            builder.Entity<Veiculo>()
                .HasIndex(v => v.VendedorId)
                .HasDatabaseName("IX_Veiculo_VendedorId");

            builder.Entity<Veiculo>()
                .HasIndex(v => v.Estado)
                .HasDatabaseName("IX_Veiculo_Estado");

            builder.Entity<Veiculo>()
                .HasIndex(v => v.Marca)
                .HasDatabaseName("IX_Veiculo_Marca");
        }
    }
}






