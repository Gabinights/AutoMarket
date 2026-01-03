using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models
{
    /// <summary>
    /// Modelo que representa uma imagem associada a um veículo.
    /// Um veículo pode ter múltiplas imagens.
    /// </summary>
    public class VeiculoImagem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string CaminhoFicheiro { get; set; } // ex: "veiculo_123_guid.jpg"

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } // ex: "image/jpeg"

        [Display(Name = "Capa")]
        public bool IsCapa { get; set; } = false; // Define se é a foto de capa

        // --- Relação com Veículo ---
        public int VeiculoId { get; set; }

        [ForeignKey("VeiculoId")]
        public Veiculo Veiculo { get; set; }
    }
}
