using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AutoMarket.Models;
using AutoMarket.Services;

namespace AutoMarket.Data
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

            // Se apagar Vendedor -> Apaga os seus Ve�culos
            builder.Entity<Veiculo>()
                .HasOne(v => v.Vendedor)
                .WithMany()
                .HasForeignKey(v => v.VendedorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Se apagar Ve�culo -> Apaga as suas Imagens
            builder.Entity<VeiculoImagem>()
                .HasOne(i => i.Veiculo)
                .WithMany(v => v.Imagens)
                .HasForeignKey(i => i.VeiculoId)
                .OnDelete(DeleteBehavior.Cascade);

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
    }
}
