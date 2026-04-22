using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Application.DTOs.Dashboard;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly FinanceDbContext _context;

    public DashboardService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Resultado<DashboardResponse>> ObterAsync(Guid usuarioId, int mes, int ano)
    {
        var dashboard = new DashboardResponse
        {
            Resumo = await ObterResumoAsync(usuarioId, mes, ano),
            GastosPorCategoria = await ObterGastosPorCategoriaAsync(usuarioId, mes, ano),
            BalancoUltimos6Meses = await ObterBalancoMensalAsync(usuarioId, mes, ano),
            UltimasTransacoes = await ObterUltimasTransacoesAsync(usuarioId),
            Orcamentos = await ObterOrcamentosAsync(usuarioId, mes, ano),
            Metas = await ObterMetasAsync(usuarioId),
            ProximasFaturas = await ObterProximasFaturasAsync(usuarioId),
            Contas = await ObterContasAsync(usuarioId)
        };

        return Resultado<DashboardResponse>.Ok(dashboard);
    }

    // RESUMO FINANCEIRO DO MÊS

    private async Task<ResumoFinanceiroResponse> ObterResumoAsync(Guid usuarioId, int mes, int ano)
    {
        var transacoesMes = _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId
                     && t.Status == StatusTransacao.EFETIVADA
                     && t.DataTransacao.Month == mes
                     && t.DataTransacao.Year == ano);

        var totalReceitas = await transacoesMes
            .Where(t => t.Tipo == TipoTransacao.RECEITA)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        var totalDespesas = await transacoesMes
            .Where(t => t.Tipo == TipoTransacao.DESPESA)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        var totalTransacoes = await transacoesMes.CountAsync();

        var maiorDespesa = await transacoesMes
            .Where(t => t.Tipo == TipoTransacao.DESPESA)
            .MaxAsync(t => (decimal?)t.Valor) ?? 0;

        var categoriaMaisGasta = await transacoesMes
            .Where(t => t.Tipo == TipoTransacao.DESPESA)
            .GroupBy(t => t.Categoria.Nome)
            .OrderByDescending(g => g.Sum(t => t.Valor))
            .Select(g => g.Key)
            .FirstOrDefaultAsync();

        var diasNoMes = DateTime.DaysInMonth(ano, mes);
        var hoje = DateTime.UtcNow;
        var diasPassados = (hoje.Year == ano && hoje.Month == mes)
            ? hoje.Day
            : diasNoMes;
        var mediaDiaria = diasPassados > 0 ? Math.Round(totalDespesas / diasPassados, 2) : 0;

        var saldoTotalContas = await _context.Contas
            .Where(c => c.UsuarioId == usuarioId && c.Ativo)
            .SumAsync(c => (decimal?)c.SaldoAtual) ?? 0;

        return new ResumoFinanceiroResponse
        {
            TotalReceitas = totalReceitas,
            TotalDespesas = totalDespesas,
            SaldoTotalContas = saldoTotalContas,
            TotalTransacoesMes = totalTransacoes,
            MediaDiariaDespesas = mediaDiaria,
            MaiorDespesaMes = maiorDespesa,
            CategoriaMaisGasta = categoriaMaisGasta
        };
    }

    // GASTOS POR CATEGORIA (gráfico pizza)

    private async Task<List<GastoPorCategoriaResponse>> ObterGastosPorCategoriaAsync(Guid usuarioId, int mes, int ano)
    {
        var gastos = await _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.UsuarioId == usuarioId
                     && t.Tipo == TipoTransacao.DESPESA
                     && t.Status == StatusTransacao.EFETIVADA
                     && t.DataTransacao.Month == mes
                     && t.DataTransacao.Year == ano)
            .GroupBy(t => new
            {
                t.CategoriaId,
                t.Categoria.Nome,
                t.Categoria.Icone,
                t.Categoria.Cor
            })
            .Select(g => new GastoPorCategoriaResponse
            {
                CategoriaId = g.Key.CategoriaId,
                NomeCategoria = g.Key.Nome,
                IconeCategoria = g.Key.Icone,
                CorCategoria = g.Key.Cor,
                ValorTotal = g.Sum(t => t.Valor),
                QuantidadeTransacoes = g.Count()
            })
            .OrderByDescending(g => g.ValorTotal)
            .ToListAsync();

        var totalGeral = gastos.Sum(g => g.ValorTotal);
        foreach (var gasto in gastos)
        {
            gasto.Percentual = totalGeral > 0
                ? Math.Round(gasto.ValorTotal / totalGeral * 100, 1)
                : 0;
        }

        return gastos;
    }

    // BALANÇO ÚLTIMOS 6 MESES (gráfico barras)

    private async Task<List<BalancoMensalResponse>> ObterBalancoMensalAsync(Guid usuarioId, int mes, int ano)
    {
        var dataFim = new DateOnly(ano, mes, DateTime.DaysInMonth(ano, mes));
        var dataInicio = dataFim.AddMonths(-5);
        dataInicio = new DateOnly(dataInicio.Year, dataInicio.Month, 1);

        var nomesMeses = new[] { "", "Jan", "Fev", "Mar", "Abr", "Mai", "Jun",
                                      "Jul", "Ago", "Set", "Out", "Nov", "Dez" };

        var transacoes = await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId
                     && t.Status == StatusTransacao.EFETIVADA
                     && t.DataTransacao >= dataInicio
                     && t.DataTransacao <= dataFim)
            .GroupBy(t => new { Ano = t.DataTransacao.Year, Mes = t.DataTransacao.Month })
            .Select(g => new
            {
                g.Key.Ano,
                g.Key.Mes,
                Receitas = g.Where(t => t.Tipo == TipoTransacao.RECEITA).Sum(t => t.Valor),
                Despesas = g.Where(t => t.Tipo == TipoTransacao.DESPESA).Sum(t => t.Valor)
            })
            .OrderBy(b => b.Ano)
            .ThenBy(b => b.Mes)
            .ToListAsync();

        // Preencher meses sem movimentação
        var resultado = new List<BalancoMensalResponse>();
        var dataAtual = dataInicio;

        for (int i = 0; i < 6; i++)
        {
            var mesAtual = dataAtual.Month;
            var anoAtual = dataAtual.Year;

            var dados = transacoes.FirstOrDefault(t => t.Ano == anoAtual && t.Mes == mesAtual);

            resultado.Add(new BalancoMensalResponse
            {
                Ano = anoAtual,
                Mes = mesAtual,
                NomeMes = nomesMeses[mesAtual],
                TotalReceitas = dados?.Receitas ?? 0,
                TotalDespesas = dados?.Despesas ?? 0
            });

            dataAtual = dataAtual.AddMonths(1);
        }

        return resultado;
    }

    // ÚLTIMAS TRANSAÇÕES

    private async Task<List<TransacaoRecenteResponse>> ObterUltimasTransacoesAsync(Guid usuarioId)
    {
        return await _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.UsuarioId == usuarioId && t.Status == StatusTransacao.EFETIVADA)
            .OrderByDescending(t => t.DataTransacao)
            .ThenByDescending(t => t.CriadoEm)
            .Take(15)
            .Select(t => new TransacaoRecenteResponse
            {
                Id = t.Id,
                Descricao = t.Descricao,
                NomeCategoria = t.Categoria.Nome,
                IconeCategoria = t.Categoria.Icone,
                CorCategoria = t.Categoria.Cor,
                Valor = t.Valor,
                Tipo = t.Tipo.ToString(),
                Origem = t.Origem.ToString(),
                DataTransacao = t.DataTransacao
            })
            .ToListAsync();
    }

    // ORÇAMENTOS DO MÊS

    private async Task<List<OrcamentoResumoResponse>> ObterOrcamentosAsync(Guid usuarioId, int mes, int ano)
    {
        var orcamentos = await _context.Orcamentos
            .Include(o => o.Categoria)
            .Where(o => o.UsuarioId == usuarioId && o.Mes == mes && o.Ano == ano)
            .ToListAsync();

        var resultado = new List<OrcamentoResumoResponse>();

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

            resultado.Add(new OrcamentoResumoResponse
            {
                NomeCategoria = orc.Categoria.Nome,
                IconeCategoria = orc.Categoria.Icone,
                CorCategoria = orc.Categoria.Cor,
                ValorLimite = orc.ValorLimite,
                TotalGasto = totalGasto
            });
        }

        return resultado
            .OrderByDescending(r => r.Estourado)
            .ThenByDescending(r => r.PercentualUsado)
            .ToList();
    }

    // METAS ATIVAS

    private async Task<List<MetaResumoResponse>> ObterMetasAsync(Guid usuarioId)
    {
        return await _context.MetasEconomia
            .Where(m => m.UsuarioId == usuarioId && !m.Concluida)
            .OrderByDescending(m => m.CriadoEm)
            .Take(5)
            .Select(m => new MetaResumoResponse
            {
                Id = m.Id,
                Titulo = m.Titulo,
                ValorAlvo = m.ValorAlvo,
                ValorAtual = m.ValorAtual,
                Cor = m.Cor,
                Icone = m.Icone
            })
            .ToListAsync();
    }

    // PRÓXIMAS FATURAS A VENCER

    private async Task<List<FaturaResumoResponse>> ObterProximasFaturasAsync(Guid usuarioId)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _context.FaturasCartao
            .Include(f => f.CartaoCredito)
            .Where(f => f.UsuarioId == usuarioId
                     && f.Status != StatusFatura.PAGA
                     && f.DataVencimento >= hoje)
            .OrderBy(f => f.DataVencimento)
            .Take(5)
            .Select(f => new FaturaResumoResponse
            {
                Id = f.Id,
                NomeCartao = f.CartaoCredito.Nome,
                CorCartao = f.CartaoCredito.Cor,
                ValorTotal = f.ValorTotal,
                ValorPago = f.ValorPago,
                DataVencimento = f.DataVencimento,
                Status = f.Status.ToString()
            })
            .ToListAsync();
    }

    // SALDO DAS CONTAS

    private async Task<List<ContaSaldoResponse>> ObterContasAsync(Guid usuarioId)
    {
        return await _context.Contas
            .Where(c => c.UsuarioId == usuarioId && c.Ativo)
            .OrderByDescending(c => c.Principal)
            .ThenBy(c => c.Nome)
            .Select(c => new ContaSaldoResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Banco = c.Banco,
                Cor = c.Cor,
                Icone = c.Icone,
                SaldoAtual = c.SaldoAtual
            })
            .ToListAsync();
    }
}