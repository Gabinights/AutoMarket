using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Models;

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
        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Comprador> Compradores { get; set; }
        public DbSet<Carro> Carros { get; set; }
        public DbSet<CarroImagem> CarroImagens { get; set; }
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

            builder.Entity<Carro>().Property(c => c.Estado).HasConversion<string>().HasMaxLength(50);

            builder.Entity<Transacao>().Property(t => t.Estado).HasConversion<string>().HasMaxLength(50);
            builder.Entity<Transacao>().Property(t => t.Metodo).HasConversion<string>().HasMaxLength(50);

            builder.Entity<Denuncia>().Property(d => d.Estado).HasConversion<string>().HasMaxLength(50);

            // Tipos SQL Especificos (Money)
            builder.Entity<Carro>().Property(c => c.Preco).HasColumnType("decimal(18,2)");
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

            // NIF unico quando preenchido (NULLs permitidos mas sem duplicados)
            builder.Entity<Utilizador>()
                .HasIndex(u => u.NIF)
                .IsUnique()
                .HasFilter("[NIF] IS NOT NULL");

            // Soft Delete Global Query Filter
            builder.Entity<Utilizador>().HasQueryFilter(u => !u.IsDeleted);
            // #endregion

            // #region 3. Relacoes: Delete CASCADE (Pai morre -> Filhos morrem)

            // Se apagar Vendedor -> Apaga os seus Carros
            builder.Entity<Carro>()
                .HasOne(c => c.Vendedor)
                .WithMany(v => v.CarrosAVenda)
                .HasForeignKey(c => c.VendedorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Se apagar Carro -> Apaga as suas Imagens
            builder.Entity<CarroImagem>()
                .HasOne(i => i.Carro)
                .WithMany(c => c.Imagens)
                .HasForeignKey(i => i.CarroId)
                .OnDelete(DeleteBehavior.Cascade);

            // #endregion

            // #region 4. Relacoes: Delete RESTRICT (Seguranca & Historico)

            // --- Transacoes (Financeiro) ---
            // Impedir apagar Comprador ou Carro se houver historico financeiro
            builder.Entity<Transacao>()
                .HasOne(t => t.Comprador)
                .WithMany()
                .HasForeignKey(t => t.CompradorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transacao>()
                .HasOne(t => t.Carro)
                .WithMany()
                .HasForeignKey(t => t.CarroId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Denuncias (Auditoria) ---
            // Impedir apagar User se tiver feito denuncias (Opcional, mas boa pratica)
            builder.Entity<Denuncia>()
                .HasOne(d => d.Denunciante)
                .WithMany()
                .HasForeignKey(d => d.DenuncianteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Impedir apagar Admin se ele resolveu denuncias (Preservar auditoria)
            builder.Entity<Denuncia>()
                .HasOne(d => d.AnalisadoPorAdmin)
                .WithMany()
                .HasForeignKey(d => d.AnalisadoPorAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Vendedores (Aprovacoes) ---
            // Preservar historico de quem aprovou o vendedor
            builder.Entity<Vendedor>()
                .HasOne(v => v.ApprovedByAdmin)
                .WithMany()
                .HasForeignKey(v => v.ApprovedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Mensagens (Chat) ---
            // Impedir apagar User se tiver mensagens (Evita o ciclo de delete do SQL Server)
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

            // --- Impede apagar uma categoria se existirem carros associados a ela
            builder.Entity<Carro>()
                .HasOne(c => c.Categoria)
                .WithMany(cat => cat.Carros) // Assumindo que tens: public ICollection<Carro> Carros { get; set; } na Categoria
                .HasForeignKey(c => c.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // #endregion
        }
    }
}
