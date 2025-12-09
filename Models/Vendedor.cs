using System;

public class Vendedor
{
	public int Id { get; set; }

    //Relação 1:1 com Utilizador
    [Required]
    public string UserId { get; set; }

    [Required]
    public Utilizador User { get; set; }

    //Dados específicos do Vendedor
    public bool IsEmpresa { get; set; } //Indica se o vendedor é uma empresa (=true) ou um particular (=false)

    //Gestão de Aprovação
    public bool IsApproved { get; set; } = false;
    public string? ApprovedByAdminId { get; set; } //ID do admin que aprovou o vendedor

    //Lista de carros que este vendedor tem à venda
    public ICollection<Car> CarrosAVenda { get; set; }
}
