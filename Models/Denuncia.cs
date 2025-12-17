using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models
{
    public enum EstadoDenuncia
    {
        Aberta = 0,
        EmAnalise = 1,
        Encerrada = 2
    }

    public class Denuncia : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DenuncianteId { get; set; } = string.Empty;

        [ForeignKey("DenuncianteId")]
        public Utilizador? Denunciante { get; set; }

        // Alvos (Opcionais na BD, Obrigatórios na Lógica)
        public int? TargetCarroId { get; set; }
        public Carro? TargetCarro { get; set; }

        public string? TargetUserId { get; set; }
        public Utilizador? TargetUser { get; set; }

        [Required(ErrorMessage = "O motivo é obrigatório.")]
        [StringLength(500)]
        public string Motivo { get; set; } = string.Empty;

        public EstadoDenuncia Estado { get; set; } = EstadoDenuncia.Aberta;
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Gestão Admin
        public string? DecisaoAdmin { get; set; }

        [StringLength(450)]
        public string? AnalisadoPorAdminId { get; set; }

        [ForeignKey("AnalisadoPorAdminId")]
        public Utilizador? AnalisadoPorAdmin { get; set; }

        // Validação: Garante que escolheu pelo menos um alvo
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TargetCarroId == null && string.IsNullOrEmpty(TargetUserId))
            {
                yield return new ValidationResult(
                    "Tem de denunciar um Veículo ou um Utilizador.",
                    new[] { nameof(TargetCarroId), nameof(TargetUserId) }
                );
            }
        }
    }
}