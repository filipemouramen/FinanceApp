using FinanceApp.Application.DTOs.Contas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.Interfaces
{
    public interface IContaService
    {
        Task<Resultado<List<ContaResponse>>> ListarAsync(Guid usuarioId);
        Task<Resultado<ContaResponse>> ObterPorIdAsync(Guid usuarioId, Guid contaId);
        Task<Resultado<ContaResponse>> CriarAsync(Guid usuarioId, CriarContaRequest request);
        Task<Resultado<ContaResponse>> AtualizarAsync(Guid usuarioId, Guid contaId, AtualizarContaRequest request);
        Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid contaId);
        Task<Resultado<TransferenciaResponse>> TransferirAsync(Guid usuarioId, TransferenciaRequest request);
    }
}