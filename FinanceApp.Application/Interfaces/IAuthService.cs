using FinanceApp.Application.DTOs.Auth;

namespace FinanceApp.Application.Interfaces;

public interface IAuthService
{
    Task<Resultado<AuthResponse>> RegistrarAsync(RegistroRequest request);
    Task<Resultado<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Resultado<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Resultado<UsuarioResponse>> ObterPerfilAsync(int usuarioId);
    Task<Resultado<UsuarioResponse>> AtualizarPerfilAsync(int usuarioId, AtualizarPerfilRequest request);
    Task<Resultado<bool>> AlterarSenhaAsync(int usuarioId, AlterarSenhaRequest request);
    Task<Resultado<bool>> SolicitarResetSenhaAsync(SolicitarResetSenhaRequest request);
    Task<Resultado<VerificarCodigoResponse>> VerificarCodigoAsync(VerificarCodigoRequest request);
    Task<Resultado<bool>> ResetarSenhaAsync(ResetarSenhaRequest request);
    Task<Resultado<bool>> RevogarTokenAsync(int usuarioId);
    Task<Resultado<bool>> ExcluirContaAsync(int usuarioId);
}
