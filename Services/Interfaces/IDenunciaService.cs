using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para serviço de denúncias.
    /// Gerencia workflow: Aberta ? Em Análise ? Encerrada (Procedente/Não Procedente).
    /// </summary>
    public interface IDenunciaService
    {
        /// <summary>
        /// Criar uma nova denúncia.
        /// </summary>
        Task<Denuncia> CriarDenunciaAsync(
            string denuncianteId,
            int? veiculoId = null,
            string? targetUserId = null,
            string motivo = "");

        /// <summary>
        /// Obter denúncia por ID.
        /// </summary>
        Task<Denuncia?> ObterDenunciaAsync(int denunciaId);

        /// <summary>
        /// Listar denúncias com filtro por estado.
        /// </summary>
        Task<List<Denuncia>> ListarDenunciasAsync(
            string? estado = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Iniciar análise de denúncia (muda para Em Análise).
        /// </summary>
        Task<bool> IniciarAnaliseAsync(int denunciaId, string adminId);

        /// <summary>
        /// Encerrar denúncia com decisão.
        /// </summary>
        Task<bool> EncerrarDenunciaAsync(
            int denunciaId,
            string adminId,
            bool procedente,
            string decisao);

        /// <summary>
        /// Contar denúncias por estado.
        /// </summary>
        Task<Dictionary<string, int>> ContarPorEstadoAsync();
    }
}
