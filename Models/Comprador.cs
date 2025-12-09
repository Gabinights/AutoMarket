using System;

public class Comprador
{
	public int Id { get; set; }
	
	// Relação 1:1 com Utilizador
	[Required]
	public string UserId { get; set; }
	public Utilizador User { get; set; }

	//Dados específicos do Comprador
	public bool ReceberNotificacoes { get; set; }

    // Histórico de compras do Comprador

    // Lista de carros favoritos do Comprador (Many-to-Many com Car)
	// public ICollection<Car> Favoritos { get; set; }
}
