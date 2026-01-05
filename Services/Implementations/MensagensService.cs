using AutoMarket.Infrastructure.Data;
using AutoMarket.Models.Entities;
using AutoMarket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
    public class MensagensService : IMensagensService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MensagensService> _logger;

        public MensagensService(ApplicationDbContext context, ILogger<MensagensService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Mensagem> EnviarMensagemAsync(string remetenteId, string destinatarioId, string conteudo, int? veiculoId = null)
        {
            var mensagem = new Mensagem
            {
                RemetenteId = remetenteId,
                DestinatarioId = destinatarioId,
                Conteudo = conteudo,
                VeiculoId = veiculoId,
                DataEnvio = DateTime.UtcNow,
                Lida = false
            };

            _context.Mensagens.Add(mensagem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Mensagem enviada de {RemetenteId} para {DestinatarioId}", remetenteId, destinatarioId);
            return mensagem;
        }

        public async Task<List<Mensagem>> ListarConversaAsync(string userId1, string userId2, int page = 1, int pageSize = 50)
        {
            return await _context.Mensagens
                .Where(m => (m.RemetenteId == userId1 && m.DestinatarioId == userId2) ||
                           (m.RemetenteId == userId2 && m.DestinatarioId == userId1))
                .OrderByDescending(m => m.DataEnvio)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> MarcarComoLidaAsync(int mensagemId)
        {
            var mensagem = await _context.Mensagens.FindAsync(mensagemId);
            if (mensagem == null) return false;

            mensagem.Lida = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ContarNaolidasAsync(string userId)
        {
            return await _context.Mensagens
                .CountAsync(m => m.DestinatarioId == userId && !m.Lida);
        }
    }
}
