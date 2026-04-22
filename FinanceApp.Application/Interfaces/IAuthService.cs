using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Domain.Entities;
using FinanceApp.Application.DTOs.Auth;


namespace FinanceApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Resultado<AuthResponse>> RegistrarAsync(RegistroRequest request);
        Task<Resultado<AuthResponse>> LoginAsync(LoginRequest request);
        Task<Resultado<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<Resultado<UsuarioResponse>> ObterPerfilAsync(Guid usuarioId);
        Task<Resultado<UsuarioResponse>> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilRequest request);
        Task<Resultado<bool>> AlterarSenhaAsync(Guid usuarioId, AlterarSenhaRequest request);
        Task<Resultado<bool>> SolicitarResetSenhaAsync(SolicitarResetSenhaRequest request);
        Task<Resultado<bool>> ResetarSenhaAsync(ResetarSenhaRequest request);
        Task<Resultado<bool>> RevogarTokenAsync(Guid usuarioId);
    }
}