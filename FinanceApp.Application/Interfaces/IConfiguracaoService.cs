using FinanceApp.Application.DTOs.Configuracoes;

namespace FinanceApp.Application.Interfaces;

public interface IConfiguracaoService
{
    Task<Resultado<ConfiguracaoResponse>> ObterAsync(int usuarioId);
    Task<Resultado<ConfiguracaoResponse>> AtualizarAsync(int usuarioId, AtualizarConfiguracaoRequest request);
}
