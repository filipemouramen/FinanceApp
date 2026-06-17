using FinanceApp.Application.DTOs.Metas;

namespace FinanceApp.Application.Interfaces;

public interface IMetaService
{
    Task<Resultado<List<MetaResponse>>> ListarAsync(int usuarioId);
    Task<Resultado<MetaResponse>> ObterPorIdAsync(int usuarioId, int metaId);
    Task<Resultado<MetaResponse>> CriarAsync(int usuarioId, CriarMetaRequest request);
    Task<Resultado<MetaResponse>> AtualizarAsync(int usuarioId, int metaId, AtualizarMetaRequest request);
    Task<Resultado<bool>> ExcluirAsync(int usuarioId, int metaId);
    Task<Resultado<MetaResponse>> AdicionarLancamentoAsync(int usuarioId, int metaId, LancamentoMetaRequest request);
}
