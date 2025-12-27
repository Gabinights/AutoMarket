using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMarket.Models
{

	public class Comprador
	{
		[Key]
		public int Id { get; set; }

		// Relação 1:1 com Utilizador
		[Required]
		public string UserId { get; set; } //Foreign Key: coluna na tabela Compradores. Isto liga as tabelas

		[ForeignKey("UserId")]
		public Utilizador User { get; set; } //Navigation Property: não existe coluna na BD. Serve para poder fazer comprador.User.Nome sem ter de fazer uma query manual SQL

		//Dados específicos do Comprador
		public bool ReceberNotificacoes { get; set; }

        // TODO: Histórico de compras do Comprador

        // Lista de carros favoritos do Comprador (Many-to-Many com Car)
        // public ICollection<Car> Favoritos { get; set; }
        // TODO: Implementar a tabela de junção para favoritos?
    }
}