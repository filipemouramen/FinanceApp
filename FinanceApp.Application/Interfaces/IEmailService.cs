namespace FinanceApp.Application.Interfaces;

public interface IEmailService
{
    Task EnviarCodigoResetAsync(string email, string codigo);
}
