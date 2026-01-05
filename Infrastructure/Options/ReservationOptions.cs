namespace AutoMarket.Infrastructure.Options
{
    public class ReservationOptions
    {
        /// <summary>Horas até expirar uma reserva (ex.: 24).</summary>
        public int TempoExpiracaoHoras { get; set; } = 24;

        /// <summary>Máximo de reservas ativas por comprador.</summary>
        public int MaxReservasAtivasPorComprador { get; set; } = 3;
    }
}