namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para entidades que suportam Soft Delete.
    /// Quando uma entidade implementa esta interface, o ApplicationDbContext
    /// intercepta tentativas de delete físico e converte-as automaticamente
    /// em soft delete (IsDeleted = true).
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Indica se a entidade foi marcada como eliminada (soft delete).
        /// </summary>
        bool IsDeleted { get; set; }
    }
}

