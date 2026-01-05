namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Background service que limpa reservas expiradas periodicamente.
    /// Executa a cada hora (configurável).
    /// </summary>
    public class LimparReservasHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LimparReservasHostedService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromHours(1); // Executar a cada hora

        public LimparReservasHostedService(
            IServiceProvider serviceProvider,
            ILogger<LimparReservasHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("?? Serviço de limpeza de reservas iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Executar limpeza
                    await LimparReservasAsync(stoppingToken);

                    // Aguardar próxima execução
                    await Task.Delay(_intervalo, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("?? Serviço de limpeza foi cancelado");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Erro no serviço de limpeza de reservas");
                    // Continuar executando mesmo após erro
                    await Task.Delay(_intervalo, stoppingToken);
                }
            }
        }

        private async Task LimparReservasAsync(CancellationToken cancellationToken)
        {
            // Usar um novo scope para cada execução
            using (var scope = _serviceProvider.CreateScope())
            {
                var reservaService = scope.ServiceProvider.GetRequiredService<IReservaService>();

                _logger.LogInformation("? Iniciando limpeza de reservas expiradas...");
                await reservaService.LimparReservasExpirasAsync();
                _logger.LogInformation("? Limpeza de reservas concluída");
            }
        }
    }
}
