using FinanceApp.Application.DTOs.CartoesCredito;

namespace FinanceApp.Application.Interfaces;

public interface ICartaoCreditoService
{
    Task<Resultado<List<CartaoCreditoResponse>>> ListarAsync(int usuarioId);
    Task<Resultado<CartaoCreditoResponse>> ObterPorIdAsync(int usuarioId, int cartaoId);
    Task<Resultado<CartaoCreditoResponse>> CriarAsync(int usuarioId, CriarCartaoCreditoRequest request);
    Task<Resultado<CartaoCreditoResponse>> AtualizarAsync(int usuarioId, int cartaoId, AtualizarCartaoCreditoRequest request);
    Task<Resultado<bool>> ExcluirAsync(int usuarioId, int cartaoId);
    Task<Resultado<FaturaCartaoResponse>> ObterFaturaAsync(int usuarioId, int cartaoId, int mes, int ano);
    Task<Resultado<FaturaCartaoResponse>> ObterFaturaPorIdAsync(int usuarioId, int cartaoId, int faturaId);
    Task<Resultado<List<FaturaCartaoResponse>>> ListarFaturasAsync(int usuarioId, int cartaoId);
    Task<Resultado<bool>> PagarFaturaAsync(int usuarioId, int faturaId, PagarFaturaRequest request);
}
