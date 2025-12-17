using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; } // Ex: "SUV", "Sedan"

        // Relação: Uma categoria tem muitos carros
        public ICollection<Carro> Carros { get; set; } = new List<Carro>();
    }
}