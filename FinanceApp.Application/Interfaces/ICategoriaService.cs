using FinanceApp.Application.DTOs.Categorias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<Resultado<List<CategoriaResponse>>> ListarAsync(Guid usuarioId, string? tipo = null);
        Task<Resultado<CategoriaResponse>> CriarAsync(Guid usuarioId, CriarCategoriaRequest request);
        Task<Resultado<CategoriaResponse>> AtualizarAsync(Guid usuarioId, int categoriaId, AtualizarCategoriaRequest request);
        Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, int categoriaId);
    }
}