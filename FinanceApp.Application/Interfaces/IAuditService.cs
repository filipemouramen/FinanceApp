namespace FinanceApp.Application.Interfaces;

public interface IAuditService
{
    Task RegistrarAsync(int usuarioId, string entidade, int entidadeId, string operacao, object? valorAnterior, object? valorNovo);
}
