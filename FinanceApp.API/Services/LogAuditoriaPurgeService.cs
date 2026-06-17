using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceApp.API.Services;

public class LogAuditoriaPurgeService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LogAuditoriaPurgeService> _logger;
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(24);
    private const int DiasRetencao = 90;

    public LogAuditoriaPurgeService(IServiceScopeFactory scopeFactory, ILogger<LogAuditoriaPurgeService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PurgarLogsAntigosAsync(stoppingToken);
            await Task.Delay(Intervalo, stoppingToken);
        }
    }

    private async Task PurgarLogsAntigosAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

            var corte = DateTime.UtcNow.AddDays(-DiasRetencao);
            var deletados = await context.LogsAuditoria
                .Where(l => l.CriadoEm < corte)
                .ExecuteDeleteAsync(ct);

            if (deletados > 0)
                _logger.LogInformation("Purge de LogsAuditoria: {Count} registros removidos (> {Dias} dias).", deletados, DiasRetencao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao purgar LogsAuditoria.");
        }
    }
}
