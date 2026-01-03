namespace AutoMarket.Models.DTOs
{
    /// <summary>
    /// DTO para encapsular parâmetros de pesquisa/filtro de veículos.
    /// Reutilizável entre diferentes métodos do controller.
    /// </summary>
    public class VeiculoSearchFiltersDto
    {
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Combustivel { get; set; }
        public int? Ano { get; set; }
        public string? Categoria { get; set; }
        public decimal? PrecoMin { get; set; }
        public decimal? PrecoMax { get; set; }
        public int? KmMin { get; set; }
        public int? KmMax { get; set; }
        public string Ordenacao { get; set; } = "recente";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12; // 12 veículos por página

        /// <summary>
        /// Valida se pelo menos um filtro foi aplicado.
        /// </summary>
        public bool HasFilters()
        {
            return !string.IsNullOrEmpty(Marca)
                || !string.IsNullOrEmpty(Modelo)
                || !string.IsNullOrEmpty(Combustivel)
                || Ano.HasValue
                || !string.IsNullOrEmpty(Categoria)
                || PrecoMin.HasValue
                || PrecoMax.HasValue
                || KmMin.HasValue
                || KmMax.HasValue;
        }
    }
}
