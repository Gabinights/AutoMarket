using AutoMarket.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoMarket.Infrastructure.Data.Configurations
{
    public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
    {
        public void Configure(EntityTypeBuilder<Reserva> builder)
        {
            // Única reserva ativa por veículo
            builder.HasIndex(r => r.VeiculoId)
                   .IsUnique()
                   .HasFilter("[Estado] = 'Ativa'");

            builder.Property(r => r.Estado)
                   .HasMaxLength(50);

            builder.Property(r => r.DataReserva)
                   .HasColumnType("datetime2");

            builder.Property(r => r.DataExpiracao)
                   .HasColumnType("datetime2");

            builder.HasOne(r => r.Veiculo)
                   .WithMany()
                   .HasForeignKey(r => r.VeiculoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Comprador)
                   .WithMany()
                   .HasForeignKey(r => r.CompradorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}