using FinanceApp.Application.DTOs.Contas;

namespace FinanceApp.Application.Interfaces;

public interface IContaService
{
    Task<Resultado<List<ContaResponse>>> ListarAsync(int usuarioId);
    Task<Resultado<ContaResponse>> ObterPorIdAsync(int usuarioId, int contaId);
    Task<Resultado<ContaResponse>> CriarAsync(int usuarioId, CriarContaRequest request);
    Task<Resultado<ContaResponse>> AtualizarAsync(int usuarioId, int contaId, AtualizarContaRequest request);
    Task<Resultado<bool>> ExcluirAsync(int usuarioId, int contaId);
    Task<Resultado<TransferenciaResponse>> TransferirAsync(int usuarioId, TransferenciaRequest request);
    Task<Resultado<decimal>> RecalcularSaldoAsync(int usuarioId, int contaId);
}
