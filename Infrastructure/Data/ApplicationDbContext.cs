using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq;
using AutoMarket.Models.Entities;
using AutoMarket.Infrastructure.Security;

namespace AutoMarket.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =========================================================
        // DbSets (Tabelas)
        // =========================================================
        public DbSet<Veiculo> Veiculos { get; set; } = null!;
        public DbSet<VeiculoImagem> VeiculoImagens { get; set; } = null!;
        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Comprador> Compradores { get; set; }
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Reserva> Reservas { get; set; } = null!;
        public DbSet<Visita> Visitas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // #region 1. Configuracao de Propriedades (Enums & Tipos SQL)

            // Conversao de Enums para String (Legibilidade na BD)
            builder.Entity<Vendedor>().Property(v => v.Status).HasConversion<string>().HasMaxLength(50);
            builder.Entity<Vendedor>().Property(v => v.TipoConta).HasConversion<string>().HasMaxLength(50);
            builder.Entity<Veiculo>().Property(v => v.Estado).HasConversion<string>().HasMaxLength(50);
            builder.Entity<Transacao>().Property(t => t.Estado).HasConversion<string>().HasMaxLength(50);
            builder.Entity<Transacao>().Property(t => t.Metodo).HasConversion<string>().HasMaxLength(50);
            builder.Entity<Denuncia>().Property(d => d.Estado).HasConversion<string>().HasMaxLength(50);
            builder.Entity<Reserva>().Property(r => r.Estado).HasConversion<string>().HasMaxLength(50);
            builder.Entity<Visita>().Property(v => v.Estado).HasConversion<string>().HasMaxLength(50);

            // Tipos SQL Especificos (Money)
            builder.Entity<Veiculo>().Property(v => v.Preco).HasColumnType("decimal(18,2)");
            builder.Entity<Transacao>().Property(t => t.ValorPago).HasColumnType("decimal(18,2)");

            // #endregion

            // #region 2. Indices e Restricoes (Unicidade)

            // Garante relacao 1:1 estrita (Um User so pode ter 1 perfil de cada tipo)
            builder.Entity<Comprador>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            builder.Entity<Vendedor>()
                .HasIndex(v => v.UserId)
                .IsUnique();

            // Indices para performance
            builder.Entity<Veiculo>()
                .HasIndex(v => v.VendedorId)
                .HasDatabaseName("IX_Veiculo_VendedorId");

            builder.Entity<Veiculo>()
                .HasIndex(v => v.Estado)
                .HasDatabaseName("IX_Veiculo_Estado");

            builder.Entity<Veiculo>()
                .HasIndex(v => v.Marca)
                .HasDatabaseName("IX_Veiculo_Marca");

            // Índices para Reservas
            builder.Entity<Reserva>()
                .HasIndex(r => r.VeiculoId)
                .HasDatabaseName("IX_Reserva_VeiculoId");

            builder.Entity<Reserva>()
                .HasIndex(r => r.CompradorId)
                .HasDatabaseName("IX_Reserva_CompradorId");

            builder.Entity<Reserva>()
                .HasIndex(r => r.DataExpiracao)
                .HasDatabaseName("IX_Reserva_DataExpiracao");

            // Índices para Visitas
            builder.Entity<Visita>()
                .HasIndex(v => v.VeiculoId)
                .HasDatabaseName("IX_Visita_VeiculoId");

            builder.Entity<Visita>()
                .HasIndex(v => v.CompradorId)
                .HasDatabaseName("IX_Visita_CompradorId");

            builder.Entity<Visita>()
                .HasIndex(v => v.VendedorId)
                .HasDatabaseName("IX_Visita_VendedorId");

            builder.Entity<Visita>()
                .HasIndex(v => v.DataHora)
                .HasDatabaseName("IX_Visita_DataHora");

            // Soft Delete Global Query Filter
            builder.Entity<Utilizador>().HasQueryFilter(u => !u.IsDeleted);

            // Encriptação do NIF (RGPD compliance) usando helper estático
            var nifConverter = new ValueConverter<string?, string?>(
                v => string.IsNullOrEmpty(v) ? v : NifEncryptionHelper.EncryptNif(v),
                v => string.IsNullOrEmpty(v) ? v : NifEncryptionHelper.DecryptNif(v),
                convertsNulls: true);

            builder.Entity<Utilizador>()
                .Property(u => u.NIF)
                .HasConversion(nifConverter);

            // #endregion

            // #region 3. Relacoes: Delete CASCADE (Pai morre -> Filhos morrem)

            // Se apagar Vendedor -> Apaga os seus Veículos
            builder.Entity<Veiculo>()
                .HasOne(v => v.Vendedor)
                .WithMany()
                .HasForeignKey(v => v.VendedorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Se apagar Veículo -> Apaga as suas Imagens
            builder.Entity<VeiculoImagem>()
                .HasOne(i => i.Veiculo)
                .WithMany(v => v.Imagens)
                .HasForeignKey(i => i.VeiculoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Se apagar Veículo -> Apaga as suas Reservas (NoAction para evitar ciclos)
            builder.Entity<Reserva>()
                .HasOne(r => r.Veiculo)
                .WithMany()
                .HasForeignKey(r => r.VeiculoId)
                .OnDelete(DeleteBehavior.NoAction);

            // Se apagar Vendedor -> Apaga as suas Visitas (NoAction para evitar ciclos)
            builder.Entity<Visita>()
                .HasOne(v => v.Vendedor)
                .WithMany()
                .HasForeignKey(v => v.VendedorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Se apagar Veículo -> Apaga as suas Visitas (NoAction para evitar ciclos)
            builder.Entity<Visita>()
                .HasOne(v => v.Veiculo)
                .WithMany()
                .HasForeignKey(v => v.VeiculoId)
                .OnDelete(DeleteBehavior.NoAction);

            // #endregion

            // #region 4. Relacoes: Delete RESTRICT (Seguranca & Historico)

            // --- Transacoes (Financeiro) ---
            builder.Entity<Transacao>()
                .HasOne(t => t.Comprador)
                .WithMany()
                .HasForeignKey(t => t.CompradorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transacao>()
                .HasOne(t => t.Veiculo)
                .WithMany()
                .HasForeignKey(t => t.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Denuncias (Auditoria) ---
            builder.Entity<Denuncia>()
                .HasOne(d => d.Denunciante)
                .WithMany()
                .HasForeignKey(d => d.DenuncianteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Denuncia>()
                .HasOne(d => d.AnalisadoPorAdmin)
                .WithMany()
                .HasForeignKey(d => d.AnalisadoPorAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Vendedores (Aprovacoes) ---
            builder.Entity<Vendedor>()
                .HasOne(v => v.ApprovedByAdmin)
                .WithMany()
                .HasForeignKey(v => v.ApprovedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Mensagens (Chat) ---
            builder.Entity<Mensagem>()
                .HasOne(m => m.Remetente)
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Mensagem>()
                .HasOne(m => m.Destinatario)
                .WithMany()
                .HasForeignKey(m => m.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Categoria ---
            builder.Entity<Veiculo>()
                .HasOne(v => v.Categoria)
                .WithMany()
                .HasForeignKey(v => v.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // #endregion
        }

        /// <summary>
        /// Intercepta tentativas de delete físico em entidades ISoftDelete
        /// e converte-as automaticamente em soft delete (IsDeleted = true).
        /// </summary>
        /// <remarks>
        /// Mesmo que um developer use _context.Utilizadores.Remove(user),
        /// o sistema converte automaticamente para soft delete.
        /// </remarks>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            InterceptSoftDeletes();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// Intercepta tentativas de delete físico em entidades ISoftDelete
        /// e converte-as automaticamente em soft delete (IsDeleted = true).
        /// </summary>
        /// <remarks>
        /// Mesmo que um developer use _context.Utilizadores.Remove(user),
        /// o sistema converte automaticamente para soft delete.
        /// </remarks>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            InterceptSoftDeletes();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// Intercepta entidades marcadas para delete físico e converte-as em soft delete.
        /// </summary>
        private void InterceptSoftDeletes()
        {
            // Intercetar entradas marcadas para Deleted que implementam ISoftDelete
            var entries = ChangeTracker.Entries<ISoftDelete>()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                // Cancelar o delete físico
                entry.State = EntityState.Modified;

                // Aplicar o soft delete
                entry.Entity.IsDeleted = true;

                // Opcional: Se a entidade tiver DataRemocao, pode ser adicionado aqui
                // if (entry.Entity is Utilizador utilizador && utilizador.GetType().GetProperty("DataRemocao") != null)
                // {
                //     utilizador.DataRemocao = DateTime.UtcNow;
                // }
            }
        }
    }
}
