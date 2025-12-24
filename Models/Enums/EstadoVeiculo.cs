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

        [Display(Name = "Vendido")]
        Vendido = 1,

        [Display(Name = "Arquivado")]
        Arquivado = 2,

        [Display(Name = "Removido")]
        Removido = 3
    }
}
