using FinanceApp.Application.DTOs.Categorias;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly FinanceDbContext _context;

    public CategoriaService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Resultado<List<CategoriaResponse>>> ListarAsync(int usuarioId, string? tipo = null)
    {
        var query = _context.Categorias
            .Where(c => c.Ativo && (c.UsuarioId == null || c.UsuarioId == usuarioId))
            .AsQueryable();

        if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoTransacao>(tipo, out var tipoEnum))
            query = query.Where(c => c.Tipo == tipoEnum);

        var categorias = await query
            .OrderBy(c => c.Tipo)
            .ThenBy(c => c.Padrao ? 0 : 1)
            .ThenBy(c => c.Nome)
            .Select(c => new CategoriaResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Icone = c.Icone,
                Cor = c.Cor,
                Tipo = c.Tipo.ToString(),
                Padrao = c.Padrao,
                TotalTransacoes = c.Transacoes.Count(t => t.UsuarioId == usuarioId)
            })
            .ToListAsync();

        return Resultado<List<CategoriaResponse>>.Ok(categorias);
    }

    public async Task<Resultado<CategoriaResponse>> CriarAsync(int usuarioId, CriarCategoriaRequest request)
    {
        if (!Enum.TryParse<TipoTransacao>(request.Tipo, out var tipo))
            return Resultado<CategoriaResponse>.Falha("Tipo inválido. Use as categorias: DESPESA ou RECEITA.");

        var existe = await _context.Categorias
            .AnyAsync(c => c.Nome == request.Nome
                        && (c.UsuarioId == null || c.UsuarioId == usuarioId)
                        && c.Ativo);
        if (existe)
            return Resultado<CategoriaResponse>.Falha("Já existe uma categoria com esse nome.");

        var categoria = new Categoria
        {
            Nome = request.Nome,
            Icone = request.Icone,
            Cor = request.Cor,
            Tipo = tipo,
            Padrao = false,
            UsuarioId = usuarioId
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return Resultado<CategoriaResponse>.Criado(new CategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Icone = categoria.Icone,
            Cor = categoria.Cor,
            Tipo = categoria.Tipo.ToString(),
            Padrao = false,
            TotalTransacoes = 0
        }, "Categoria criada com sucesso!");
    }

    public async Task<Resultado<CategoriaResponse>> AtualizarAsync(int usuarioId, int categoriaId, AtualizarCategoriaRequest request)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == categoriaId && c.Ativo);

        if (categoria == null)
            return Resultado<CategoriaResponse>.NaoEncontrado("Categoria não encontrada.");

        if (categoria.Padrao)
            return Resultado<CategoriaResponse>.Falha("Não é possível editar categorias padrão do sistema.");

        if (categoria.UsuarioId != usuarioId)
            return Resultado<CategoriaResponse>.Falha("Você não tem permissão para editar esta categoria.", 403);

        if (!string.IsNullOrWhiteSpace(request.Nome))
        {
            var existe = await _context.Categorias
                .AnyAsync(c => c.Nome == request.Nome
                            && c.Id != categoriaId
                            && (c.UsuarioId == null || c.UsuarioId == usuarioId)
                            && c.Ativo);
            if (existe)
                return Resultado<CategoriaResponse>.Falha("Já existe uma categoria com esse nome.");

            categoria.Nome = request.Nome;
        }

        if (!string.IsNullOrWhiteSpace(request.Icone))
            categoria.Icone = request.Icone;

        if (!string.IsNullOrWhiteSpace(request.Cor))
            categoria.Cor = request.Cor;

        await _context.SaveChangesAsync();

        var totalTransacoes = await _context.Transacoes
            .CountAsync(t => t.CategoriaId == categoriaId && t.UsuarioId == usuarioId);

        return Resultado<CategoriaResponse>.Ok(new CategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Icone = categoria.Icone,
            Cor = categoria.Cor,
            Tipo = categoria.Tipo.ToString(),
            Padrao = categoria.Padrao,
            TotalTransacoes = totalTransacoes
        }, "Categoria atualizada!");
    }

    public async Task<Resultado<bool>> ExcluirAsync(int usuarioId, int categoriaId)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == categoriaId && c.Ativo);

        if (categoria == null)
            return Resultado<bool>.NaoEncontrado("Categoria não encontrada.");

        if (categoria.Padrao)
            return Resultado<bool>.Falha("Não é possível excluir categorias padrão do sistema.");

        if (categoria.UsuarioId != usuarioId)
            return Resultado<bool>.Falha("Você não tem permissão para excluir esta categoria.", 403);

        var temTransacoes = await _context.Transacoes
            .AnyAsync(t => t.CategoriaId == categoriaId && t.UsuarioId == usuarioId);

        if (temTransacoes)
        {
            categoria.Ativo = false;
            await _context.SaveChangesAsync();
            return Resultado<bool>.Ok(true, "Categoria desativada. Possui transações vinculadas.");
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Categoria excluída!");
    }
}
