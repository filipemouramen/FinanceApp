using FinanceApp.Application.DTOs.Orcamentos;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;

public class OrcamentoService : IOrcamentoService
{
    private readonly FinanceDbContext _context;
    private readonly INotificacaoService _notificacaoService;

    public OrcamentoService(FinanceDbContext context, INotificacaoService notificacaoService)
    {
        _context = context;
        _notificacaoService = notificacaoService;
    }

    public async Task<Resultado<List<OrcamentoResponse>>> ListarPorMesAsync(int usuarioId, int mes, int ano)
    {
        var orcamentos = await _context.Orcamentos
            .Include(o => o.Categoria)
            .Where(o => o.UsuarioId == usuarioId && o.Mes == mes && o.Ano == ano)
            .ToListAsync();

        var responses = new List<OrcamentoResponse>();

        foreach (var orc in orcamentos)
        {
            var totalGasto = await _context.Transacoes
                .Where(t => t.UsuarioId == usuarioId
                         && t.CategoriaId == orc.CategoriaId
                         && t.Tipo == TipoTransacao.DESPESA
                         && t.Status == StatusTransacao.EFETIVADA
                         && t.DataTransacao.Month == mes
                         && t.DataTransacao.Year == ano)
                .SumAsync(t => (decimal?)t.Valor) ?? 0;

            responses.Add(new OrcamentoResponse
            {
                Id = orc.Id,
                CategoriaId = orc.CategoriaId,
                NomeCategoria = orc.Categoria.Nome,
                IconeCategoria = orc.Categoria.Icone,
                CorCategoria = orc.Categoria.Cor,
                ValorLimite = orc.ValorLimite,
                TotalGasto = totalGasto,
                PercentualAlerta = orc.PercentualAlerta,
                Mes = orc.Mes,
                Ano = orc.Ano
            });
        }

        responses = responses
            .OrderByDescending(r => r.Estourado)
            .ThenByDescending(r => r.PercentualUsado)
            .ToList();

        return Resultado<List<OrcamentoResponse>>.Ok(responses);
    }

    public async Task<Resultado<OrcamentoResponse>> CriarAsync(int usuarioId, CriarOrcamentoRequest request)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == request.CategoriaId && c.Ativo);
        if (categoria == null)
            return Resultado<OrcamentoResponse>.Falha("Categoria não encontrada.");

        if (categoria.Tipo != TipoTransacao.DESPESA)
            return Resultado<OrcamentoResponse>.Falha("Orçamentos só podem ser criados para categorias de despesa.");

        var mes = request.Mes ?? DateTime.UtcNow.Month;
        var ano = request.Ano ?? DateTime.UtcNow.Year;

        var existe = await _context.Orcamentos
            .AnyAsync(o => o.UsuarioId == usuarioId
                        && o.CategoriaId == request.CategoriaId
                        && o.Mes == mes
                        && o.Ano == ano);
        if (existe)
            return Resultado<OrcamentoResponse>.Falha(
                $"Já existe um orçamento para '{categoria.Nome}' em {mes:D2}/{ano}.");

        var orcamento = new Orcamento
        {
            UsuarioId = usuarioId,
            CategoriaId = request.CategoriaId,
            ValorLimite = request.ValorLimite,
            Mes = mes,
            Ano = ano,
            PercentualAlerta = request.PercentualAlerta
        };

        _context.Orcamentos.Add(orcamento);
        await _context.SaveChangesAsync();

        var totalGasto = await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId
                     && t.CategoriaId == request.CategoriaId
                     && t.Tipo == TipoTransacao.DESPESA
                     && t.Status == StatusTransacao.EFETIVADA
                     && t.DataTransacao.Month == mes
                     && t.DataTransacao.Year == ano)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        return Resultado<OrcamentoResponse>.Criado(new OrcamentoResponse
        {
            Id = orcamento.Id,
            CategoriaId = orcamento.CategoriaId,
            NomeCategoria = categoria.Nome,
            IconeCategoria = categoria.Icone,
            CorCategoria = categoria.Cor,
            ValorLimite = orcamento.ValorLimite,
            TotalGasto = totalGasto,
            PercentualAlerta = orcamento.PercentualAlerta,
            Mes = mes,
            Ano = ano
        }, "Orçamento criado com sucesso!");
    }

    public async Task<Resultado<OrcamentoResponse>> AtualizarAsync(int usuarioId, int orcamentoId, AtualizarOrcamentoRequest request)
    {
        var orcamento = await _context.Orcamentos
            .Include(o => o.Categoria)
            .FirstOrDefaultAsync(o => o.Id == orcamentoId && o.UsuarioId == usuarioId);

        if (orcamento == null)
            return Resultado<OrcamentoResponse>.NaoEncontrado("Orçamento não encontrado.");

        if (request.ValorLimite.HasValue)
            orcamento.ValorLimite = request.ValorLimite.Value;

        if (request.PercentualAlerta.HasValue)
            orcamento.PercentualAlerta = request.PercentualAlerta.Value;

        await _context.SaveChangesAsync();

        var totalGasto = await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId
                     && t.CategoriaId == orcamento.CategoriaId
                     && t.Tipo == TipoTransacao.DESPESA
                     && t.Status == StatusTransacao.EFETIVADA
                     && t.DataTransacao.Month == orcamento.Mes
                     && t.DataTransacao.Year == orcamento.Ano)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        return Resultado<OrcamentoResponse>.Ok(new OrcamentoResponse
        {
            Id = orcamento.Id,
            CategoriaId = orcamento.CategoriaId,
            NomeCategoria = orcamento.Categoria.Nome,
            IconeCategoria = orcamento.Categoria.Icone,
            CorCategoria = orcamento.Categoria.Cor,
            ValorLimite = orcamento.ValorLimite,
            TotalGasto = totalGasto,
            PercentualAlerta = orcamento.PercentualAlerta,
            Mes = orcamento.Mes,
            Ano = orcamento.Ano
        }, "Orçamento atualizado!");
    }

    public async Task<Resultado<bool>> ExcluirAsync(int usuarioId, int orcamentoId)
    {
        var orcamento = await _context.Orcamentos
            .FirstOrDefaultAsync(o => o.Id == orcamentoId && o.UsuarioId == usuarioId);

        if (orcamento == null)
            return Resultado<bool>.NaoEncontrado("Orçamento não encontrado.");

        _context.Orcamentos.Remove(orcamento);
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Orçamento excluído!");
    }

    public async Task VerificarAlertasAsync(int usuarioId, int categoriaId)
    {
        var agora = DateTime.UtcNow;
        var mes = agora.Month;
        var ano = agora.Year;

        var orcamento = await _context.Orcamentos
            .Include(o => o.Categoria)
            .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId
                                   && o.CategoriaId == categoriaId
                                   && o.Mes == mes
                                   && o.Ano == ano);

        if (orcamento == null)
            return;

        var totalGasto = await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId
                     && t.CategoriaId == categoriaId
                     && t.Tipo == TipoTransacao.DESPESA
                     && t.Status == StatusTransacao.EFETIVADA
                     && t.DataTransacao.Month == mes
                     && t.DataTransacao.Year == ano)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        if (orcamento.ValorLimite <= 0)
            return;

        var percentual = totalGasto / orcamento.ValorLimite * 100;

        // Alert at 80%
        if (percentual >= 80 && percentual < 100)
        {
            var jaNotificou80 = await _context.Notificacoes
                .AnyAsync(n => n.UsuarioId == usuarioId
                            && n.Tipo == TipoNotificacao.ALERTA_ORCAMENTO_80
                            && n.EntidadeRelacionadaId == orcamento.Id
                            && n.CriadoEm.Month == mes
                            && n.CriadoEm.Year == ano);

            if (!jaNotificou80)
            {
                await _notificacaoService.CriarAsync(
                    usuarioId,
                    TipoNotificacao.ALERTA_ORCAMENTO_80,
                    $"Alerta de orçamento: {orcamento.Categoria.Nome}",
                    $"Você usou {percentual:F0}% do seu orçamento de '{orcamento.Categoria.Nome}' (R${totalGasto:F2} de R${orcamento.ValorLimite:F2}).",
                    orcamento.Id);
            }
        }

        // Alert at 100%
        if (percentual >= 100)
        {
            var jaNotificou100 = await _context.Notificacoes
                .AnyAsync(n => n.UsuarioId == usuarioId
                            && n.Tipo == TipoNotificacao.ALERTA_ORCAMENTO_100
                            && n.EntidadeRelacionadaId == orcamento.Id
                            && n.CriadoEm.Month == mes
                            && n.CriadoEm.Year == ano);

            if (!jaNotificou100)
            {
                await _notificacaoService.CriarAsync(
                    usuarioId,
                    TipoNotificacao.ALERTA_ORCAMENTO_100,
                    $"Orçamento estourado: {orcamento.Categoria.Nome}",
                    $"Você atingiu 100% do orçamento de '{orcamento.Categoria.Nome}'. Gasto: R${totalGasto:F2} / Limite: R${orcamento.ValorLimite:F2}.",
                    orcamento.Id);
            }
        }
    }
}
