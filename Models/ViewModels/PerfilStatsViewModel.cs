namespace AutoMarket.Models.ViewModels
{
    /// <summary>
    /// ViewModel para exibir estatísticas do utilizador no perfil.
    /// Usado pelo ProfileService.
    /// </summary>
    public class PerfilStatsViewModel
    {
        public int TotalCompras { get; set; }
        public int TotalVendas { get; set; }
        public int TotalVeiculos { get; set; }
        public string StatusVendedor { get; set; } = string.Empty;
        public bool IsComprador { get; set; }
        public bool IsVendedor { get; set; }
    }
}