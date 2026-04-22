using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Application.DTOs.Notificacoes;

namespace FinanceApp.Application.Interfaces;

public interface INotificacaoService
{
    Task<Resultado<List<NotificacaoResponse>>> ListarAsync(Guid usuarioId, bool? apenasNaoLidas = null);
    Task<Resultado<ContadorNotificacoesResponse>> ContarAsync(Guid usuarioId);
    Task<Resultado<bool>> MarcarComoLidaAsync(Guid usuarioId, Guid notificacaoId);
    Task<Resultado<bool>> MarcarTodasComoLidasAsync(Guid usuarioId);
    Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid notificacaoId);
}