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

            // Convers�o de Enums para String (Legibilidade na BD)
            builder.Entity<Vendedor>().Property(v => v.Status).HasConversion<string>();
            builder.Entity<Carro>().Property(c => c.Estado).HasConversion<string>();
            builder.Entity<Transacao>().Property(t => t.Estado).HasConversion<string>();
            builder.Entity<Transacao>().Property(t => t.Metodo).HasConversion<string>();
            builder.Entity<Denuncia>().Property(d => d.Estado).HasConversion<string>();

            // Tipos SQL Espec�ficos (Money)
            builder.Entity<Carro>().Property(c => c.Preco).HasColumnType("decimal(18,2)");
            builder.Entity<Transacao>().Property(t => t.ValorPago).HasColumnType("decimal(18,2)");

            // #endregion

            // #region 2. �ndices e Restri��es (Unicidade)

            // Garante rela��o 1:1 estrita (Um User s� pode ter 1 perfil de cada tipo)
            builder.Entity<Comprador>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            builder.Entity<Vendedor>()
                .HasIndex(v => v.UserId)
                .IsUnique();

            // #endregion

            // #region 3. Rela��es: Delete CASCADE (Pai morre -> Filhos morrem)

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

            // #region 4. Rela��es: Delete RESTRICT (Seguran�a & Hist�rico)

            // --- Transa��es (Financeiro) ---
            // Impedir apagar Comprador ou Carro se houver hist�rico financeiro
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

            // --- Den�ncias (Auditoria) ---
            // Impedir apagar User se tiver feito den�ncias (Opcional, mas boa pr�tica)
            builder.Entity<Denuncia>()
                .HasOne(d => d.Denunciante)
                .WithMany()
                .HasForeignKey(d => d.DenuncianteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Impedir apagar Admin se ele resolveu den�ncias (Preservar auditoria)
            builder.Entity<Denuncia>()
                .HasOne(d => d.AnalisadoPorAdmin)
                .WithMany()
                .HasForeignKey(d => d.AnalisadoPorAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Vendedores (Aprova��es) ---
            // Preservar hist�rico de quem aprovou o vendedor
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