using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Models.Enums;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            ILogger<CheckoutService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Transacao> ProcessCheckoutAsync(string userId, Transacao checkout)
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new InvalidOperationException("Utilizador não encontrado.");

                var veiculo = await _context.Veiculos.FindAsync(checkout.VeiculoId);
                if (veiculo == null || veiculo.Estado != EstadoVeiculo.Ativo)
                    throw new InvalidOperationException("Veículo não está disponível.");

                checkout.DataTransacao = DateTime.UtcNow;
                checkout.Estado = EstadoTransacao.Pendente;

                _context.Transacoes.Add(checkout);
                veiculo.Estado = EstadoVeiculo.Vendido;

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                _logger.LogInformation("Transação {TransacaoId} processada para utilizador {UserId}", checkout.Id, userId);
                return checkout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar checkout para utilizador {UserId}", userId);
                throw;
            }
        }

        public async Task<Transacao?> GetTransacaoAsync(string userId, int transacaoId)
        {
            var transacao = await _context.Transacoes
                .Include(t => t.Comprador)
                .Include(t => t.Veiculo)
                .FirstOrDefaultAsync(t => t.Id == transacaoId);

            if (transacao == null || transacao.Comprador.UserId != userId)
                return null;

            return transacao;
        }
    }
}