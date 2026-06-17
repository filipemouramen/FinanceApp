using FinanceApp.Application.DTOs.Notificacoes;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;

public class NotificacaoService : INotificacaoService
{
    private readonly FinanceDbContext _context;

    public NotificacaoService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Resultado<ListaNotificacoesResponse>> ListarAsync(int usuarioId, bool? apenasNaoLidas = null, int pagina = 1, int tamanhoPagina = 20)
    {
        var query = _context.Notificacoes
            .Where(n => n.UsuarioId == usuarioId)
            .AsQueryable();

        if (apenasNaoLidas == true)
            query = query.Where(n => !n.Lida);

        var total = await query.CountAsync();

        var notificacoes = await query
            .OrderByDescending(n => n.CriadoEm)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(n => new NotificacaoResponse
            {
                Id = n.Id,
                Titulo = n.Titulo,
                Mensagem = n.Mensagem,
                Tipo = n.Tipo.ToString(),
                Lida = n.Lida,
                EntidadeRelacionadaId = n.EntidadeRelacionadaId,
                CriadoEm = n.CriadoEm
            })
            .ToListAsync();

        var agora = DateTime.UtcNow;
        foreach (var notif in notificacoes)
            notif.TempoAtras = CalcularTempoAtras(notif.CriadoEm, agora);

        return Resultado<ListaNotificacoesResponse>.Ok(new ListaNotificacoesResponse
        {
            Itens = notificacoes,
            TotalItens = total,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        });
    }

    public async Task<Resultado<ContadorNotificacoesResponse>> ContarNaoLidasAsync(int usuarioId)
    {
        var count = await _context.Notificacoes
            .CountAsync(n => n.UsuarioId == usuarioId && !n.Lida);

        return Resultado<ContadorNotificacoesResponse>.Ok(new ContadorNotificacoesResponse
        {
            Count = count
        });
    }

    public async Task<Resultado<NotificacaoResponse>> MarcarComoLidaAsync(int usuarioId, int notificacaoId)
    {
        var notificacao = await _context.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == notificacaoId && n.UsuarioId == usuarioId);

        if (notificacao == null)
            return Resultado<NotificacaoResponse>.NaoEncontrado("Notificação não encontrada.");

        notificacao.Lida = true;
        await _context.SaveChangesAsync();

        return Resultado<NotificacaoResponse>.Ok(new NotificacaoResponse
        {
            Id = notificacao.Id,
            Titulo = notificacao.Titulo,
            Mensagem = notificacao.Mensagem,
            Tipo = notificacao.Tipo.ToString(),
            Lida = notificacao.Lida,
            EntidadeRelacionadaId = notificacao.EntidadeRelacionadaId,
            CriadoEm = notificacao.CriadoEm,
            TempoAtras = CalcularTempoAtras(notificacao.CriadoEm, DateTime.UtcNow)
        });
    }

    public async Task<Resultado<bool>> MarcarTodasComoLidasAsync(int usuarioId)
    {
        var naoLidas = await _context.Notificacoes
            .Where(n => n.UsuarioId == usuarioId && !n.Lida)
            .ToListAsync();

        foreach (var n in naoLidas)
            n.Lida = true;

        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, $"{naoLidas.Count} notificações marcadas como lidas.");
    }

    public async Task<Resultado<bool>> ExcluirAsync(int usuarioId, int notificacaoId)
    {
        var notificacao = await _context.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == notificacaoId && n.UsuarioId == usuarioId);

        if (notificacao == null)
            return Resultado<bool>.NaoEncontrado("Notificação não encontrada.");

        _context.Notificacoes.Remove(notificacao);
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Notificação excluída!");
    }

    public async Task CriarAsync(int usuarioId, TipoNotificacao tipo, string titulo, string mensagem, int? entidadeId = null)
    {
        _context.Notificacoes.Add(new Notificacao
        {
            UsuarioId = usuarioId,
            Tipo = tipo,
            Titulo = titulo,
            Mensagem = mensagem,
            EntidadeRelacionadaId = entidadeId
        });
        await _context.SaveChangesAsync();
    }

    private static string CalcularTempoAtras(DateTime data, DateTime agora)
    {
        var diferenca = agora - data;

        if (diferenca.TotalMinutes < 1)
            return "agora";
        if (diferenca.TotalMinutes < 60)
            return $"há {(int)diferenca.TotalMinutes}min";
        if (diferenca.TotalHours < 24)
            return $"há {(int)diferenca.TotalHours}h";
        if (diferenca.TotalDays < 7)
            return $"há {(int)diferenca.TotalDays}d";
        if (diferenca.TotalDays < 30)
            return $"há {(int)(diferenca.TotalDays / 7)} sem";

        return data.ToString("dd/MM/yyyy");
    }
}
