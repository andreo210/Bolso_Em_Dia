using BolsoEmDia.Application.Services.RecorrenciaServices;

namespace BolsoEmDia.Api.Jobs
{
    /// <summary>
    /// UC21 — Gerar ocorrência de recorrência. Hosted service (singleton) que dispara todo dia à
    /// meia-noite; abre um escopo de DI a cada execução para resolver o serviço/DbContext scoped
    /// (ver arquitetura-api/aplicacao-api.md sobre injetar scoped dentro de singleton).
    /// </summary>
    public class RecorrenciaJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecorrenciaJob> _logger;

        public RecorrenciaJob(IServiceScopeFactory scopeFactory, ILogger<RecorrenciaJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var agora = DateTime.UtcNow;
                var proximaMeiaNoite = agora.Date.AddDays(1);

                try
                {
                    await Task.Delay(proximaMeiaNoite - agora, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                await ExecutarAsync(stoppingToken);
            }
        }

        private async Task ExecutarAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var jobService = scope.ServiceProvider.GetRequiredService<IRecorrenciaJobService>();

            try
            {
                await jobService.ProcessarGeracoesAsync(DateTime.UtcNow.Date, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Falha de infraestrutura (ex.: banco fora do ar): registra e tenta de novo na
                // próxima meia-noite, em vez de derrubar o host inteiro por causa de uma execução.
                _logger.LogError(ex, "Falha ao executar RecorrenciaJob (UC21)");
            }
        }
    }
}
