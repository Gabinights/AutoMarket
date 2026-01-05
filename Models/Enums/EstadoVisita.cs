namespace AutoMarket.Models.Enums
{
    /// <summary>
    /// Estados possíveis de uma visita agendada.
    /// </summary>
    public enum EstadoVisita
    {
        /// <summary>Visita agendada, aguardando confirmação</summary>
        Pendente = 0,

        /// <summary>Visita confirmada pelo vendedor</summary>
        Confirmada = 1,

        /// <summary>Visita foi realizada</summary>
        Realizada = 2,

        /// <summary>Visita foi cancelada</summary>
        Cancelada = 3,

        /// <summary>Visita passou da data agendada sem ser realizada</summary>
        NaoRealizada = 4
    }
}
