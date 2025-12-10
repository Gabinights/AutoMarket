using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Models;

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
        public DbSet<Carro> Carros { get; set; }
        public DbSet<CarroImagem> CarroImagens { get; set; }
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
       



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =========================================================
            // 1. Configurações de Enums (Para guardar como Texto)
            // =========================================================

            builder.Entity<Vendedor>()
            .Property(v => v.Status)
            .HasConversion<string>();

            builder.Entity<Carro>()
                .Property(c => c.Estado)
                .HasConversion<string>();

            builder.Entity<Transacao>()
                .Property(t => t.Estado)
                .HasConversion<string>();

            builder.Entity<Transacao>()
                .Property(t => t.Metodo)
                .HasConversion<string>();

            builder.Entity<Denuncia>()
                .Property(d => d.Estado)
                .HasConversion<string>();

            // =========================================================
            // 2. Tipos de Dados Especiais (SQL)
            // =========================================================

            builder.Entity<Carro>().Property(c => c.Preco).HasColumnType("money");
            builder.Entity<Transacao>().Property(t => t.ValorPago).HasColumnType("money");


            // =========================================================
            // 3. Relações e Cascades (Explícito é melhor que Implícito)
            // =========================================================

            // Configurar Delete em Cascata: Apagar Vendedor -> Apaga Carros
            builder.Entity<Carro>()
                .HasOne(c => c.Vendedor)
                .WithMany(v => v.CarrosAVenda)
                .HasForeignKey(c => c.VendedorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar Delete em Cascata: Apagar Carro -> Apaga Imagens
            builder.Entity<CarroImagem>()
                .HasOne(i => i.Carro)
                .WithMany(c => c.Imagens)
                .HasForeignKey(i => i.CarroId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict: Apagar User NÃO pode apagar Mensagens (Evitar Ciclos)
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

            // Restrict: Apagar User NÃO deve apagar Denúncias feitas por ele (Histórico)
            builder.Entity<Denuncia>()
                .HasOne(d => d.Denunciante)
                .WithMany()
                .HasForeignKey(d => d.DenuncianteId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // CORREÇÃO DO ERRO DE CICLO (Transações)
            // =========================================================

            // Se apagares um Comprador, NÃO apagues o histórico de compras dele.
            // O SQL vai lançar erro se tentares apagar um comprador com compras.
            builder.Entity<Transacao>()
                .HasOne(t => t.Comprador)
                .WithMany() // (Ou .WithMany(c => c.Transacoes) se tiveres a lista no Comprador)
                .HasForeignKey(t => t.CompradorId)
                .OnDelete(DeleteBehavior.Restrict); // <--- O SEGREDO ESTÁ AQUI

            // Se apagares um Carro, também não deves apagar o registo financeiro da venda.
            builder.Entity<Transacao>()
                .HasOne(t => t.Carro)
                .WithMany()
                .HasForeignKey(t => t.CarroId)
                .OnDelete(DeleteBehavior.Restrict); // <--- AQUI TAMBÉM




        }
    }
}






