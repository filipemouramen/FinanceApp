using FinanceApp.Application.DTOs.Categorias;

namespace FinanceApp.Application.Interfaces;

public interface ICategoriaService
{
    Task<Resultado<List<CategoriaResponse>>> ListarAsync(int usuarioId, string? tipo = null);
    Task<Resultado<CategoriaResponse>> CriarAsync(int usuarioId, CriarCategoriaRequest request);
    Task<Resultado<CategoriaResponse>> AtualizarAsync(int usuarioId, int categoriaId, AtualizarCategoriaRequest request);
    Task<Resultado<bool>> ExcluirAsync(int usuarioId, int categoriaId);
}
