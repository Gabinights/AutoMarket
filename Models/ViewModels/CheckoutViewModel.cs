using System.ComponentModel.DataAnnotations;
using AutoMarket.Utils;

namespace AutoMarket.Models.ViewModels
{
    public class CheckoutViewModel : IValidatableObject
    {
        // Dados de Envio / Contacto
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        public string NomeCompleto { get; set; }

        [Required(ErrorMessage = "A morada é obrigatória.")]
        public string Morada { get; set; }

        [Required]
        public string CodigoPostal { get; set; }

        // --- DADOS FISCAIS ---

        [Display(Name = "Desejo fatura com número de contribuinte")]
        public bool QueroFaturaComNif { get; set; }

        [Display(Name = "NIF para Fatura")]
        public string? NifFaturacao { get; set; }

        [Display(Name = "Nome na Fatura (Opcional)")]
        public string? NomeFaturacao { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 1. Se pediu fatura, o NIF passa a ser OBRIGATÓRIO
            if (QueroFaturaComNif)
            {
                if (string.IsNullOrWhiteSpace(NifFaturacao))
                {
                    yield return new ValidationResult(
                        "Para emitir fatura, é necessário introduzir o NIF.",
                        new[] { nameof(NifFaturacao) });
                }
                else
                {
                    // 2. Se preenchido, tem de ser matematicamente válido
                    if (!NifValidator.IsValid(NifFaturacao))
                    {
                        yield return new ValidationResult(
                            "O NIF de faturação não é válido.",
                            new[] { nameof(NifFaturacao) });
                    }

                    // Nota: No checkout, normalmente NÃO validamos se começa por 1 ou 5.
                    // Eu posso ser um particular (conta 1) a comprar carro para a minha empresa (nif 5),
                    // ou vice-versa. Validamos apenas a existência fiscal (IsValid).
                }
            }
            // 3. Caso Raro: Não pediu fatura, mas escreveu NIF na caixa (User Error)
            else if (!string.IsNullOrWhiteSpace(NifFaturacao))
            {
                if (!NifValidator.IsValid(NifFaturacao))
                {
                    yield return new ValidationResult(
                        "O NIF introduzido não é válido.",
                        new[] { nameof(NifFaturacao) });
                }
            }
        }
    }
}