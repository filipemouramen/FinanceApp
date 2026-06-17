using FinanceApp.Application.DTOs.Transferencias;
using FinanceApp.Application.DTOs.Transacoes;

namespace FinanceApp.Application.Interfaces;

public interface ITransferenciaService
{
    Task<Resultado<TransferenciaResponseDTO>> CriarAsync(int usuarioId, CriarTransferenciaDTO dto);
    Task<Resultado<ListaPaginada<TransferenciaResponseDTO>>> ListarAsync(int usuarioId, FiltroTransferenciaDTO filtro);
    Task<Resultado<CancelamentoTransferenciaResponse>> CancelarAsync(int id, int usuarioId);
}
