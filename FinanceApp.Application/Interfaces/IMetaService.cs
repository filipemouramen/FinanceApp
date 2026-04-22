using FinanceApp.Application.DTOs.Metas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.Interfaces
{
    public interface IMetaService
    {
        Task<Resultado<List<MetaResponse>>> ListarAsync(Guid usuarioId);
        Task<Resultado<MetaResponse>> ObterPorIdAsync(Guid usuarioId, Guid metaId);
        Task<Resultado<MetaResponse>> CriarAsync(Guid usuarioId, CriarMetaRequest request);
        Task<Resultado<MetaResponse>> AtualizarAsync(Guid usuarioId, Guid metaId, AtualizarMetaRequest request);
        Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid metaId);
        Task<Resultado<MetaResponse>> AdicionarLancamentoAsync(Guid usuarioId, Guid metaId, LancamentoMetaRequest request);
    }

}