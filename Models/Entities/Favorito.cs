using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models.Entities
{
    /// <summary>
    /// Bookmarks de anúncios favoritos para compradores.
    /// Um comprador pode ter múltiplos favoritos.
    /// </summary>
    public class Favorito
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID do comprador que fez o bookmark.
        /// </summary>
        public int CompradorId { get; set; }

        [ForeignKey("CompradorId")]
        public Comprador? Comprador { get; set; }

        /// <summary>
        /// ID do veículo favoritado.
        /// </summary>
        public int VeiculoId { get; set; }

        [ForeignKey("VeiculoId")]
        public Veiculo? Veiculo { get; set; }

        /// <summary>
        /// Data de quando foi adicionado aos favoritos.
        /// </summary>
        public DateTime DataAdicao { get; set; } = DateTime.UtcNow;
    }
}
