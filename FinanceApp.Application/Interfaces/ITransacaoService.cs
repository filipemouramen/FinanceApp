using FinanceApp.Application.DTOs.Transacoes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.Interfaces
{
    public interface ITransacaoService
    {
        Task<Resultado<TransacaoResponse>> CriarAsync(Guid usuarioId, CriarTransacaoRequest request);
        Task<Resultado<TransacaoResponse>> AtualizarAsync(Guid usuarioId, Guid transacaoId, AtualizarTransacaoRequest request);
        Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid transacaoId);
        Task<Resultado<TransacaoResponse>> ObterPorIdAsync(Guid usuarioId, Guid transacaoId);
        Task<Resultado<ListaPaginada<TransacaoResponse>>> ListarAsync(Guid usuarioId, FiltroTransacaoRequest filtro);
    }
}