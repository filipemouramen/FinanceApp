using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Application.DTOs.Notificacoes;
using FinanceApp.Application.Interfaces;
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

    public async Task<Resultado<List<NotificacaoResponse>>> ListarAsync(Guid usuarioId, bool? apenasNaoLidas = null)
    {
        var query = _context.Notificacoes
            .Where(n => n.UsuarioId == usuarioId)
            .AsQueryable();

        if (apenasNaoLidas == true)
            query = query.Where(n => !n.Lida);

        var notificacoes = await query
            .OrderByDescending(n => n.CriadoEm)
            .Take(50)
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
        {
            notif.TempoAtras = CalcularTempoAtras(notif.CriadoEm, agora);
        }

        return Resultado<List<NotificacaoResponse>>.Ok(notificacoes);
    }

    public async Task<Resultado<ContadorNotificacoesResponse>> ContarAsync(Guid usuarioId)
    {
        var total = await _context.Notificacoes
            .CountAsync(n => n.UsuarioId == usuarioId);

        var naoLidas = await _context.Notificacoes
            .CountAsync(n => n.UsuarioId == usuarioId && !n.Lida);

        return Resultado<ContadorNotificacoesResponse>.Ok(new ContadorNotificacoesResponse
        {
            Total = total,
            NaoLidas = naoLidas
        });
    }

    public async Task<Resultado<bool>> MarcarComoLidaAsync(Guid usuarioId, Guid notificacaoId)
    {
        var notificacao = await _context.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == notificacaoId && n.UsuarioId == usuarioId);

        if (notificacao == null)
            return Resultado<bool>.NaoEncontrado("Notificação não encontrada.");

        notificacao.Lida = true;
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true);
    }

    public async Task<Resultado<bool>> MarcarTodasComoLidasAsync(Guid usuarioId)
    {
        var naoLidas = await _context.Notificacoes
            .Where(n => n.UsuarioId == usuarioId && !n.Lida)
            .ToListAsync();

        foreach (var n in naoLidas)
            n.Lida = true;

        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, $"{naoLidas.Count} notificações marcadas como lidas.");
    }

    public async Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid notificacaoId)
    {
        var notificacao = await _context.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == notificacaoId && n.UsuarioId == usuarioId);

        if (notificacao == null)
            return Resultado<bool>.NaoEncontrado("Notificação não encontrada.");

        _context.Notificacoes.Remove(notificacao);
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Notificação excluída!");
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