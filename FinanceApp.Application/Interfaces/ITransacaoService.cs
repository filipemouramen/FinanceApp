using FinanceApp.Application.DTOs.Transacoes;

namespace FinanceApp.Application.Interfaces;

public interface ITransacaoService
{
    Task<Resultado<TransacaoResponse>> CriarAsync(int usuarioId, CriarTransacaoRequest request);
    Task<Resultado<TransacaoResponse>> AtualizarAsync(int usuarioId, int transacaoId, AtualizarTransacaoRequest request);
    Task<Resultado<TransacaoResponse>> AtualizarStatusAsync(int usuarioId, int transacaoId, AtualizarStatusRequest request);
    Task<Resultado<bool>> ExcluirAsync(int usuarioId, int transacaoId);
    Task<Resultado<TransacaoResponse>> ObterPorIdAsync(int usuarioId, int transacaoId);
    Task<Resultado<ListaPaginada<TransacaoResponse>>> ListarAsync(int usuarioId, FiltroTransacaoRequest filtro);
    Task<Resultado<bool>> CancelarParcelamentoAsync(int usuarioId, int parcelamentoId);
}
