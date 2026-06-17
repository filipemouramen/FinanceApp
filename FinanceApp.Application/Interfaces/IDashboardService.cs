using FinanceApp.Application.DTOs.Dashboard;

namespace FinanceApp.Application.Interfaces;

public interface IDashboardService
{
    Task<Resultado<DashboardResponse>> ObterAsync(int usuarioId, int mes, int ano);
}
