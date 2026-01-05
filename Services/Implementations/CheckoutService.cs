using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Utils;
using AutoMarket.Models.DTOs;
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
        private readonly ICarrinhoService _carrinhoService;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            ICarrinhoService carrinhoService,
            ILogger<CheckoutService> logger)
        {
            _context = context;
            _userManager = userManager;
            _carrinhoService = carrinhoService;
            _logger = logger;
        }

        public async Task<CheckoutInitDto?> GetCheckoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var carrinhoItens = _carrinhoService.GetItens();
            if (!carrinhoItens.Any()) return null;

            return new CheckoutInitDto(
                user.Nome,
                user.Morada ?? string.Empty,
                user.NIF,
                _carrinhoService.GetTotal());
        }

        public async Task<CheckoutProcessResultDto> ProcessAsync(string userId, CheckoutInputDto input)
        {
            var errors = new List<string>();
            var itensCarrinho = _carrinhoService.GetItens();
            if (!itensCarrinho.Any())
            {
                errors.Add("O carrinho está vazio.");
                return new CheckoutProcessResultDto(false, null, errors);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                errors.Add("Utilizador não encontrado.");
                return new CheckoutProcessResultDto(false, null, errors);
            }

            if (input.QueroFaturaComNif && string.IsNullOrEmpty(user.NIF) && !string.IsNullOrEmpty(input.NifFaturacao))
            {
                if (!NifValidator.IsValid(input.NifFaturacao))
                {
                    errors.Add("NIF inválido. Por favor, verifique o número introduzido.");
                    return new CheckoutProcessResultDto(false, null, errors);
                }

                user.NIF = input.NifFaturacao;
                await _userManager.UpdateAsync(user);
            }

            await using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (comprador == null)
                {
                    comprador = new Comprador { UserId = user.Id };
                    _context.Add(comprador);
                    await _context.SaveChangesAsync();
                }

                var transacoesIds = new List<int>();

                foreach (var item in itensCarrinho)
                {
                    var veiculoDb = await _context.Veiculos.FindAsync(item.VeiculoId);
                    if (veiculoDb == null || veiculoDb.Estado != EstadoVeiculo.Ativo)
                    {
                        throw new InvalidOperationException($"O veículo {item.Marca} {item.Modelo} já não está disponível.");
                    }

                    var transacao = new Transacao
                    {
                        DataTransacao = DateTime.UtcNow,
                        ValorPago = item.Preco,
                        Metodo = input.MetodoPagamento,
                        Estado = EstadoTransacao.Pendente,
                        CompradorId = comprador.Id,
                        VeiculoId = item.VeiculoId,
                        NifFaturacaoSnapshot = input.QueroFaturaComNif ? input.NifFaturacao : null,
                        MoradaEnvioSnapshot = $"{input.Morada}, {input.CodigoPostal}"
                    };

                    _context.Transacoes.Add(transacao);
                    veiculoDb.Estado = EstadoVeiculo.Reservado;
                    transacoesIds.Add(transacao.Id);
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                _carrinhoService.LimparCarrinho();

                return new CheckoutProcessResultDto(true, transacoesIds.FirstOrDefault(), Enumerable.Empty<string>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar compra para o utilizador {UserId}", user.Id);
                errors.Add(ex.Message);
                return new CheckoutProcessResultDto(false, null, errors);
            }
        }

        public async Task<TransacaoDto?> GetTransacaoAsync(string userId, int transacaoId)
        {
            var transacao = await _context.Transacoes
                .Include(t => t.Comprador)
                .FirstOrDefaultAsync(t => t.Id == transacaoId);

            if (transacao == null || transacao.Comprador.UserId != userId)
            {
                return null;
            }

            return new TransacaoDto(
                transacao.Id,
                transacao.ValorPago,
                transacao.Estado,
                transacao.DataTransacao,
                string.IsNullOrWhiteSpace(transacao.NifFaturacaoSnapshot) ? null : transacao.NifFaturacaoSnapshot,
                transacao.MoradaEnvioSnapshot);
        }
    }
}