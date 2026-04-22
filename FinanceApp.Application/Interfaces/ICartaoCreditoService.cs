using FinanceApp.Application.DTOs.CartoesCredito;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.Interfaces
{
    public interface ICartaoCreditoService
    {
        Task<Resultado<List<CartaoCreditoResponse>>> ListarAsync(Guid usuarioId);
        Task<Resultado<CartaoCreditoResponse>> CriarAsync(Guid usuarioId, CriarCartaoCreditoRequest request);
        Task<Resultado<CartaoCreditoResponse>> AtualizarAsync(Guid usuarioId, Guid cartaoId, AtualizarCartaoCreditoRequest request);
        Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid cartaoId);
        Task<Resultado<FaturaCartaoResponse>> ObterFaturaAsync(Guid usuarioId, Guid cartaoId, int mes, int ano);
        Task<Resultado<List<FaturaCartaoResponse>>> ListarFaturasAsync(Guid usuarioId, Guid cartaoId);
        Task<Resultado<bool>> PagarFaturaAsync(Guid usuarioId, Guid faturaId, PagarFaturaRequest request);
    }
}