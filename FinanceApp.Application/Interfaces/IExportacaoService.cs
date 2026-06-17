using FinanceApp.Application.DTOs.Exportacao;

namespace FinanceApp.Application.Interfaces;

public interface IExportacaoService
{
    Task<Resultado<ExportacaoPdfResponseDTO>> GerarExtratoPdfAsync(int usuarioId, DateTime dataInicio, DateTime dataFim);
}
