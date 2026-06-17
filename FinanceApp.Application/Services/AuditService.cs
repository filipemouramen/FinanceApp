using System.Text.Json;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.Data;

namespace FinanceApp.Application.Services;

public class AuditService : IAuditService
{
    private readonly FinanceDbContext _context;

    public AuditService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(int usuarioId, string entidade, int entidadeId, string operacao, object? valorAnterior, object? valorNovo)
    {
        var log = new LogAuditoria
        {
            UsuarioId = usuarioId,
            Acao = operacao,
            TipoEntidade = entidade,
            EntidadeId = entidadeId.ToString(),
            ValorAnterior = valorAnterior != null ? JsonSerializer.Serialize(valorAnterior) : null,
            ValorNovo = valorNovo != null ? JsonSerializer.Serialize(valorNovo) : null,
            CriadoEm = DateTime.UtcNow
        };

        _context.LogsAuditoria.Add(log);
        await _context.SaveChangesAsync();
    }
}
