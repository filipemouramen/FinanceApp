using FinanceApp.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<Resultado<DashboardResponse>> ObterAsync(Guid usuarioId, int mes, int ano);
    }
}