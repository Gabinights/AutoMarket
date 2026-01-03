using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Models.Enums
{
    /// <summary>
    /// Enum para o estado do veículo no sistema.
    /// </summary>
    public enum EstadoVeiculo
    {
        [Display(Name = "Ativo")]
        Ativo = 0,

        [Display(Name = "Reservado")]
        Reservado = 1,

        [Display(Name = "Vendido")]
        Vendido = 2,

        [Display(Name = "Pausado")]
        Pausado = 3
    }
}
