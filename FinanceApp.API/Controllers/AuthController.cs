using FinanceApp.Application.DTOs.Auth;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("registrar")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Registrar([FromBody] RegistroRequest request)
        => RespostaDe(await _authService.RegistrarAsync(request));

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => RespostaDe(await _authService.LoginAsync(request));

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        => RespostaDe(await _authService.RefreshTokenAsync(request));

    [HttpGet("perfil")]
    [Authorize]
    public async Task<IActionResult> ObterPerfil()
        => RespostaDe(await _authService.ObterPerfilAsync(UsuarioIdAtual));

    [HttpPut("perfil")]
    [Authorize]
    public async Task<IActionResult> AtualizarPerfil([FromBody] AtualizarPerfilRequest request)
        => RespostaDe(await _authService.AtualizarPerfilAsync(UsuarioIdAtual, request));

    [HttpPost("alterar-senha")]
    [Authorize]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request)
        => RespostaDe(await _authService.AlterarSenhaAsync(UsuarioIdAtual, request));

    [HttpPost("solicitar-reset-senha")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> SolicitarResetSenha([FromBody] SolicitarResetSenhaRequest request)
        => RespostaDe(await _authService.SolicitarResetSenhaAsync(request));

    [HttpPost("verificar-codigo")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerificarCodigo([FromBody] VerificarCodigoRequest request)
        => RespostaDe(await _authService.VerificarCodigoAsync(request));

    [HttpPost("resetar-senha")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetarSenha([FromBody] ResetarSenhaRequest request)
        => RespostaDe(await _authService.ResetarSenhaAsync(request));

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
        => RespostaDe(await _authService.RevogarTokenAsync(UsuarioIdAtual));

    [HttpDelete("conta")]
    [Authorize]
    public async Task<IActionResult> ExcluirConta()
        => RespostaDe(await _authService.ExcluirContaAsync(UsuarioIdAtual));
}
