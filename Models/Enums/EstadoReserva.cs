namespace AutoMarket.Models.Enums
{
    /// <summary>
    /// Estados possíveis de uma reserva de veículo.
    /// </summary>
    public enum EstadoReserva
    {
        /// <summary>Reserva acabou de ser criada, aguardando confirmação do comprador</summary>
        Pendente = 0,

        /// <summary>Reserva confirmada e válida até a data de expiração</summary>
        Confirmada = 1,

        /// <summary>Reserva expirou sem ser confirmada ou finalizada</summary>
        Expirada = 2,

        /// <summary>Reserva foi cancelada pelo comprador ou vendedor</summary>
        Cancelada = 3,

        /// <summary>Reserva foi convertida em transação (compra realizada)</summary>
        Concluida = 4
    }
}
