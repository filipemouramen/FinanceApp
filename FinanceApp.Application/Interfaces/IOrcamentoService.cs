using FinanceApp.Application.DTOs.Orcamentos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.Interfaces
{
    public interface IOrcamentoService
    {
        Task<Resultado<List<OrcamentoResponse>>> ListarPorMesAsync(Guid usuarioId, int mes, int ano);
        Task<Resultado<OrcamentoResponse>> CriarAsync(Guid usuarioId, CriarOrcamentoRequest request);
        Task<Resultado<OrcamentoResponse>> AtualizarAsync(Guid usuarioId, Guid orcamentoId, AtualizarOrcamentoRequest request);
        Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid orcamentoId);
    }
}