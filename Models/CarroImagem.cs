using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models
{
    public class CarroImagem
    {
        [Key] //Desnecessária esta data annotation?? 
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string CaminhoFicheiro { get; set; } // ex: "carro_123_guid.jpg"

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } // ex: "image/jpeg"

        [Display(Name = "Capa")]
        public bool IsCapa { get; set; } = false; // Define se é a foto de capa

        // --- Relação com Carro ---
        public int CarroId { get; set; }

        [ForeignKey("CarroId")]
        public Carro Carro { get; set; }
    }
}



