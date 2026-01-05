using AutoMarket.Models.Attributes;
using AutoMarket.Models; // Para aceder ao atributo [NifPortugues]
using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Models.ViewModels
{
    public class DadosFiscaisViewModel
    {
        [Display(Name = "Número de Identificação Fiscal (NIF)")]
        [Required(ErrorMessage = "O NIF é obrigatório para emitir a fatura.")]
        [StringLength(9, ErrorMessage = "O NIF deve ter 9 dígitos.")]
        [NifPortugues(ErrorMessage = "O NIF introduzido não é válido.")]
        public string NIF { get; set; }
    }
}
