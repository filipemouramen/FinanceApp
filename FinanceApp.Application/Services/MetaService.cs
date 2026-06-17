using FinanceApp.Application.DTOs.Metas;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;

public class MetaService : IMetaService
{
    private readonly FinanceDbContext _context;
    private readonly INotificacaoService _notificacaoService;

    public MetaService(FinanceDbContext context, INotificacaoService notificacaoService)
    {
        _context = context;
        _notificacaoService = notificacaoService;
    }

    public async Task<Resultado<List<MetaResponse>>> ListarAsync(int usuarioId)
    {
        var metas = await _context.MetasEconomia
            .Include(m => m.Lancamentos)
            .Where(m => m.UsuarioId == usuarioId)
            .OrderBy(m => m.Concluida)
            .ThenByDescending(m => m.CriadoEm)
            .Select(m => new MetaResponse
            {
                Id = m.Id,
                Titulo = m.Titulo,
                ValorAlvo = m.ValorAlvo,
                ValorAtual = m.ValorAtual,
                DataLimite = m.DataLimite,
                Icone = m.Icone,
                Cor = m.Cor,
                Concluida = m.Concluida,
                CriadoEm = m.CriadoEm,
                UltimosLancamentos = m.Lancamentos
                    .OrderByDescending(l => l.CriadoEm)
                    .Take(5)
                    .Select(l => new LancamentoMetaResponse
                    {
                        Id = l.Id,
                        Valor = l.Valor,
                        Observacoes = l.Observacoes,
                        CriadoEm = l.CriadoEm
                    })
                    .ToList()
            })
            .ToListAsync();

        return Resultado<List<MetaResponse>>.Ok(metas);
    }

    public async Task<Resultado<MetaResponse>> ObterPorIdAsync(int usuarioId, int metaId)
    {
        var meta = await _context.MetasEconomia
            .Include(m => m.Lancamentos)
            .Where(m => m.Id == metaId && m.UsuarioId == usuarioId)
            .Select(m => new MetaResponse
            {
                Id = m.Id,
                Titulo = m.Titulo,
                ValorAlvo = m.ValorAlvo,
                ValorAtual = m.ValorAtual,
                DataLimite = m.DataLimite,
                Icone = m.Icone,
                Cor = m.Cor,
                Concluida = m.Concluida,
                CriadoEm = m.CriadoEm,
                UltimosLancamentos = m.Lancamentos
                    .OrderByDescending(l => l.CriadoEm)
                    .Take(10)
                    .Select(l => new LancamentoMetaResponse
                    {
                        Id = l.Id,
                        Valor = l.Valor,
                        Observacoes = l.Observacoes,
                        CriadoEm = l.CriadoEm
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (meta == null)
            return Resultado<MetaResponse>.NaoEncontrado("Meta não encontrada.");

        return Resultado<MetaResponse>.Ok(meta);
    }

    public async Task<Resultado<MetaResponse>> CriarAsync(int usuarioId, CriarMetaRequest request)
    {
        if (request.DataLimite.HasValue && request.DataLimite.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
            return Resultado<MetaResponse>.Falha("A data limite deve ser uma data futura.");

        var meta = new MetaEconomia
        {
            UsuarioId = usuarioId,
            Titulo = request.Titulo,
            ValorAlvo = request.ValorAlvo,
            DataLimite = request.DataLimite,
            Icone = request.Icone,
            Cor = request.Cor
        };

        _context.MetasEconomia.Add(meta);
        await _context.SaveChangesAsync();

        return Resultado<MetaResponse>.Criado(new MetaResponse
        {
            Id = meta.Id,
            Titulo = meta.Titulo,
            ValorAlvo = meta.ValorAlvo,
            ValorAtual = 0,
            DataLimite = meta.DataLimite,
            Icone = meta.Icone,
            Cor = meta.Cor,
            Concluida = false,
            CriadoEm = meta.CriadoEm,
            UltimosLancamentos = new()
        }, "Meta criada com sucesso!");
    }

    public async Task<Resultado<MetaResponse>> AtualizarAsync(int usuarioId, int metaId, AtualizarMetaRequest request)
    {
        var meta = await _context.MetasEconomia
            .Include(m => m.Lancamentos)
            .FirstOrDefaultAsync(m => m.Id == metaId && m.UsuarioId == usuarioId);

        if (meta == null)
            return Resultado<MetaResponse>.NaoEncontrado("Meta não encontrada.");

        if (meta.Concluida)
            return Resultado<MetaResponse>.Falha("Não é possível editar uma meta já concluída.");

        if (!string.IsNullOrWhiteSpace(request.Titulo))
            meta.Titulo = request.Titulo;

        if (request.ValorAlvo.HasValue)
        {
            if (request.ValorAlvo.Value < meta.ValorAtual)
                return Resultado<MetaResponse>.Falha(
                    $"O valor alvo não pode ser menor que o valor já guardado (R${meta.ValorAtual:F2}).");
            meta.ValorAlvo = request.ValorAlvo.Value;
        }

        if (request.DataLimite.HasValue)
            meta.DataLimite = request.DataLimite;

        if (request.Icone != null)
            meta.Icone = request.Icone;

        if (request.Cor != null)
            meta.Cor = request.Cor;

        meta.AtualizadoEm = DateTime.UtcNow;

        if (meta.ValorAtual >= meta.ValorAlvo && !meta.Concluida)
        {
            meta.Concluida = true;
            meta.ConcluidaEm = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Resultado<MetaResponse>.Ok(new MetaResponse
        {
            Id = meta.Id,
            Titulo = meta.Titulo,
            ValorAlvo = meta.ValorAlvo,
            ValorAtual = meta.ValorAtual,
            DataLimite = meta.DataLimite,
            Icone = meta.Icone,
            Cor = meta.Cor,
            Concluida = meta.Concluida,
            CriadoEm = meta.CriadoEm,
            UltimosLancamentos = meta.Lancamentos
                .OrderByDescending(l => l.CriadoEm)
                .Take(5)
                .Select(l => new LancamentoMetaResponse
                {
                    Id = l.Id,
                    Valor = l.Valor,
                    Observacoes = l.Observacoes,
                    CriadoEm = l.CriadoEm
                })
                .ToList()
        }, "Meta atualizada!");
    }

    public async Task<Resultado<bool>> ExcluirAsync(int usuarioId, int metaId)
    {
        var meta = await _context.MetasEconomia
            .FirstOrDefaultAsync(m => m.Id == metaId && m.UsuarioId == usuarioId);

        if (meta == null)
            return Resultado<bool>.NaoEncontrado("Meta não encontrada.");

        // soft delete via AuditInterceptor (MetaEconomia implements ISoftDeletable)
        _context.MetasEconomia.Remove(meta);
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Meta excluída!");
    }

    public async Task<Resultado<MetaResponse>> AdicionarLancamentoAsync(int usuarioId, int metaId, LancamentoMetaRequest request)
    {
        var meta = await _context.MetasEconomia
            .Include(m => m.Lancamentos)
            .FirstOrDefaultAsync(m => m.Id == metaId && m.UsuarioId == usuarioId);

        if (meta == null)
            return Resultado<MetaResponse>.NaoEncontrado("Meta não encontrada.");

        if (meta.Concluida)
            return Resultado<MetaResponse>.Falha("Esta meta já foi concluída.");

        var valorRestante = meta.ValorAlvo - meta.ValorAtual;
        if (request.Valor > valorRestante)
            return Resultado<MetaResponse>.Falha(
                $"Valor excede o necessário. Faltam R${valorRestante:F2} para completar a meta.");

        var lancamento = new LancamentoMeta
        {
            MetaEconomiaId = metaId,
            Valor = request.Valor,
            Observacoes = request.Observacoes
        };

        _context.LancamentosMeta.Add(lancamento);

        meta.ValorAtual += request.Valor;
        meta.AtualizadoEm = DateTime.UtcNow;

        if (meta.ValorAtual >= meta.ValorAlvo)
        {
            meta.Concluida = true;
            meta.ConcluidaEm = DateTime.UtcNow;

            await _notificacaoService.CriarAsync(
                usuarioId,
                TipoNotificacao.META_ATINGIDA,
                "Meta atingida!",
                $"Parabéns! Você completou a meta '{meta.Titulo}' de R${meta.ValorAlvo:F2}!",
                meta.Id);
        }

        await _context.SaveChangesAsync();

        var mensagem = meta.Concluida
            ? $"Parabéns! Meta '{meta.Titulo}' concluída!"
            : $"R${request.Valor:F2} adicionados. Faltam R${(meta.ValorAlvo - meta.ValorAtual):F2}.";

        return Resultado<MetaResponse>.Ok(new MetaResponse
        {
            Id = meta.Id,
            Titulo = meta.Titulo,
            ValorAlvo = meta.ValorAlvo,
            ValorAtual = meta.ValorAtual,
            DataLimite = meta.DataLimite,
            Icone = meta.Icone,
            Cor = meta.Cor,
            Concluida = meta.Concluida,
            CriadoEm = meta.CriadoEm,
            UltimosLancamentos = meta.Lancamentos
                .OrderByDescending(l => l.CriadoEm)
                .Take(5)
                .Select(l => new LancamentoMetaResponse
                {
                    Id = l.Id,
                    Valor = l.Valor,
                    Observacoes = l.Observacoes,
                    CriadoEm = l.CriadoEm
                })
                .ToList()
        }, mensagem);
    }
}
