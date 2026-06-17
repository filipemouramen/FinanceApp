using FinanceApp.Application.DTOs.Notificacoes;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Interfaces;

public interface INotificacaoService
{
    Task<Resultado<ListaNotificacoesResponse>> ListarAsync(int usuarioId, bool? apenasNaoLidas = null, int pagina = 1, int tamanhoPagina = 20);
    Task<Resultado<ContadorNotificacoesResponse>> ContarNaoLidasAsync(int usuarioId);
    Task<Resultado<NotificacaoResponse>> MarcarComoLidaAsync(int usuarioId, int notificacaoId);
    Task<Resultado<bool>> MarcarTodasComoLidasAsync(int usuarioId);
    Task<Resultado<bool>> ExcluirAsync(int usuarioId, int notificacaoId);
    Task CriarAsync(int usuarioId, TipoNotificacao tipo, string titulo, string mensagem, int? entidadeId = null);
}
