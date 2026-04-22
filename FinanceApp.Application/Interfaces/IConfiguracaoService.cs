using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Application.DTOs.Configuracoes;

namespace FinanceApp.Application.Interfaces;

public interface IConfiguracaoService
{
    Task<Resultado<ConfiguracaoResponse>> ObterAsync(Guid usuarioId);
    Task<Resultado<ConfiguracaoResponse>> AtualizarAsync(Guid usuarioId, AtualizarConfiguracaoRequest request);
}