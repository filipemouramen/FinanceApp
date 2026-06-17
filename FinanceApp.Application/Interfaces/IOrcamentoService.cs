using FinanceApp.Application.DTOs.Orcamentos;

namespace FinanceApp.Application.Interfaces;

public interface IOrcamentoService
{
    Task<Resultado<List<OrcamentoResponse>>> ListarPorMesAsync(int usuarioId, int mes, int ano);
    Task<Resultado<OrcamentoResponse>> CriarAsync(int usuarioId, CriarOrcamentoRequest request);
    Task<Resultado<OrcamentoResponse>> AtualizarAsync(int usuarioId, int orcamentoId, AtualizarOrcamentoRequest request);
    Task<Resultado<bool>> ExcluirAsync(int usuarioId, int orcamentoId);
    Task VerificarAlertasAsync(int usuarioId, int categoriaId);
}
