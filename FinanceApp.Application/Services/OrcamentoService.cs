using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public OrcamentoService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Resultado<List<OrcamentoResponse>>> ListarPorMesAsync(Guid usuarioId, int mes, int ano)
    {
        var orcamentos = await _context.Orcamentos
            .Include(o => o.Categoria)
            .Where(o => o.UsuarioId == usuarioId && o.Mes == mes && o.Ano == ano)
            .ToListAsync();

        var responses = new List<OrcamentoResponse>();

        foreach (var orc in orcamentos)
        {
            //quanto foi gasto em cada categoria naquele mês
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

    public async Task<Resultado<OrcamentoResponse>> CriarAsync(Guid usuarioId, CriarOrcamentoRequest request)
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

        // Calcular gasto atual
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

    public async Task<Resultado<OrcamentoResponse>> AtualizarAsync(Guid usuarioId, Guid orcamentoId, AtualizarOrcamentoRequest request)
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

    public async Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid orcamentoId)
    {
        var orcamento = await _context.Orcamentos
            .FirstOrDefaultAsync(o => o.Id == orcamentoId && o.UsuarioId == usuarioId);

        if (orcamento == null)
            return Resultado<bool>.NaoEncontrado("Orçamento não encontrado.");

        _context.Orcamentos.Remove(orcamento);
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Orçamento excluído!");
    }
}