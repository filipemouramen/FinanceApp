using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanceApp.Application.DTOs.Auth;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FinanceApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly FinanceDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(UserManager<Usuario> userManager, FinanceDbContext context, IConfiguration config)
    {
        _userManager = userManager;
        _context = context;
        _config = config;
    }

    //REGISTRO
    public async Task<Resultado<AuthResponse>> RegistrarAsync(RegistroRequest request)
    {
        var existente = await _userManager.FindByEmailAsync(request.Email);
        if (existente != null)
            return Resultado<AuthResponse>.Falha("Este e-mail já está cadastrado.");

        if (!string.IsNullOrWhiteSpace(request.TelefoneWhatsApp))
        {
            var telefoneExiste = await _context.Users
                .AnyAsync(u => u.TelefoneWhatsApp == request.TelefoneWhatsApp);
            if (telefoneExiste)
                return Resultado<AuthResponse>.Falha("Este telefone já está cadastrado.");
        }

        var usuario = new Usuario
        {
            UserName = request.Email,
            Email = request.Email,
            NomeCompleto = request.NomeCompleto,
            TelefoneWhatsApp = request.TelefoneWhatsApp,
            EmailConfirmed = true
        };

        var resultado = await _userManager.CreateAsync(usuario, request.Senha);
        if (!resultado.Succeeded)
        {
            var erros = resultado.Errors.Select(e => TraduzirErroIdentity(e.Code, e.Description)).ToList();
            return Resultado<AuthResponse>.Falha(erros);
        }

        _context.ConfiguracoesUsuario.Add(new ConfiguracaoUsuario
        {
            UsuarioId = usuario.Id
        });
        await _context.SaveChangesAsync();

        var authResponse = await GerarTokensAsync(usuario);
        return Resultado<AuthResponse>.Criado(authResponse, "Conta criada com sucesso!");
    }

    //LOGIN
    public async Task<Resultado<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email);
        if (usuario == null || !usuario.Ativo)
            return Resultado<AuthResponse>.Falha("E-mail ou senha inválidos.", 401);

        if (await _userManager.IsLockedOutAsync(usuario))
            return Resultado<AuthResponse>.Falha("Conta bloqueada temporariamente. Tente novamente em alguns minutos.", 423);

        var senhaValida = await _userManager.CheckPasswordAsync(usuario, request.Senha);
        if (!senhaValida)
        {
            await _userManager.AccessFailedAsync(usuario);
            var tentativasRestantes = _userManager.Options.Lockout.MaxFailedAccessAttempts
                - await _userManager.GetAccessFailedCountAsync(usuario);
            return Resultado<AuthResponse>.Falha(
                $"E-mail ou senha inválidos. {tentativasRestantes} tentativas restantes.", 401);
        }

        await _userManager.ResetAccessFailedCountAsync(usuario);

        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = usuario.Id,
            Acao = "LOGIN",
            TipoEntidade = "Usuario",
            EntidadeId = usuario.Id.ToString(),
            Detalhes = "Login realizado com sucesso"
        });
        await _context.SaveChangesAsync();

        var authResponse = await GerarTokensAsync(usuario);
        return Resultado<AuthResponse>.Ok(authResponse);
    }

    //REFRESH TOKEN
    public async Task<Resultado<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = ObterPrincipalDoTokenExpirado(request.Token);
        if (principal == null)
            return Resultado<AuthResponse>.Falha("Token inválido.", 401);

        var usuarioIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Resultado<AuthResponse>.Falha("Token inválido.", 401);

        var refreshToken = await _context.TokensAtualizacao
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && t.UsuarioId == usuarioId);

        if (refreshToken == null)
            return Resultado<AuthResponse>.Falha("Refresh token não encontrado.", 401);

        if (!refreshToken.Ativo)
        {
            await RevogarFamiliaTokensAsync(usuarioId);
            return Resultado<AuthResponse>.Falha("Refresh token expirado. Faça login novamente.", 401);
        }

        refreshToken.RevogadoEm = DateTime.UtcNow;

        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
        if (usuario == null || !usuario.Ativo)
            return Resultado<AuthResponse>.Falha("Usuário não encontrado.", 401);

        var authResponse = await GerarTokensAsync(usuario);

        refreshToken.SubstituidoPor = authResponse.RefreshToken;
        await _context.SaveChangesAsync();

        return Resultado<AuthResponse>.Ok(authResponse);
    }

    //OBTER PERFIL
    public async Task<Resultado<UsuarioResponse>> ObterPerfilAsync(Guid usuarioId)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
        if (usuario == null)
            return Resultado<UsuarioResponse>.NaoEncontrado("Usuário não encontrado.");

        return Resultado<UsuarioResponse>.Ok(MapearUsuarioResponse(usuario));
    }

    //ATUALIZAR PERFIL
    public async Task<Resultado<UsuarioResponse>> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilRequest request)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
        if (usuario == null)
            return Resultado<UsuarioResponse>.NaoEncontrado("Usuário não encontrado.");

        if (!string.IsNullOrWhiteSpace(request.NomeCompleto))
            usuario.NomeCompleto = request.NomeCompleto;

        if (request.TelefoneWhatsApp != null)
        {
            if (!string.IsNullOrWhiteSpace(request.TelefoneWhatsApp))
            {
                var telefoneExiste = await _context.Users
                    .AnyAsync(u => u.TelefoneWhatsApp == request.TelefoneWhatsApp && u.Id != usuarioId);
                if (telefoneExiste)
                    return Resultado<UsuarioResponse>.Falha("Este telefone já está cadastrado.");
            }
            usuario.TelefoneWhatsApp = request.TelefoneWhatsApp;
        }

        if (request.FotoUrl != null)
            usuario.FotoUrl = request.FotoUrl;

        usuario.AtualizadoEm = DateTime.UtcNow;
        await _userManager.UpdateAsync(usuario);

        return Resultado<UsuarioResponse>.Ok(MapearUsuarioResponse(usuario), "Perfil atualizado!");
    }

    //ALTERAR SENHA
    public async Task<Resultado<bool>> AlterarSenhaAsync(Guid usuarioId, AlterarSenhaRequest request)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
        if (usuario == null)
            return Resultado<bool>.NaoEncontrado("Usuário não encontrado.");

        var resultado = await _userManager.ChangePasswordAsync(usuario, request.SenhaAtual, request.NovaSenha);
        if (!resultado.Succeeded)
        {
            var erros = resultado.Errors.Select(e => TraduzirErroIdentity(e.Code, e.Description)).ToList();
            return Resultado<bool>.Falha(erros);
        }

        await RevogarFamiliaTokensAsync(usuarioId);

        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = usuarioId,
            Acao = "ALTERAR_SENHA",
            TipoEntidade = "Usuario",
            EntidadeId = usuarioId.ToString()
        });
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Senha alterada com sucesso!");
    }

    //SOLICITAR RESET SENHA
    public async Task<Resultado<bool>> SolicitarResetSenhaAsync(SolicitarResetSenhaRequest request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email);
        if (usuario == null)
            return Resultado<bool>.Ok(true, "Se o e-mail existir, um código será enviado.");

        var codigosAntigos = await _context.CodigosVerificacao
            .Where(c => c.UsuarioId == usuario.Id
                     && c.Finalidade == FinalidadeCodigo.RESETAR_SENHA
                     && c.UsadoEm == null)
            .ToListAsync();

        foreach (var c in codigosAntigos)
            c.UsadoEm = DateTime.UtcNow;

        var codigo = new Random().Next(100000, 999999).ToString();
        _context.CodigosVerificacao.Add(new CodigoVerificacao
        {
            UsuarioId = usuario.Id,
            Codigo = codigo,
            Finalidade = FinalidadeCodigo.RESETAR_SENHA,
            ExpiraEm = DateTime.UtcNow.AddMinutes(15)
        });
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, $"Código enviado para o e-mail. (DEV: {codigo})");
    }

    public async Task<Resultado<bool>> ResetarSenhaAsync(ResetarSenhaRequest request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email);
        if (usuario == null)
            return Resultado<bool>.Falha("Dados inválidos.");

        var codigoVerificacao = await _context.CodigosVerificacao
            .Where(c => c.UsuarioId == usuario.Id
                     && c.Codigo == request.Codigo
                     && c.Finalidade == FinalidadeCodigo.RESETAR_SENHA
                     && c.UsadoEm == null
                     && c.ExpiraEm > DateTime.UtcNow)
            .OrderByDescending(c => c.CriadoEm)
            .FirstOrDefaultAsync();

        if (codigoVerificacao == null)
            return Resultado<bool>.Falha("Código inválido ou expirado.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var resultado = await _userManager.ResetPasswordAsync(usuario, token, request.NovaSenha);

        if (!resultado.Succeeded)
        {
            var erros = resultado.Errors.Select(e => TraduzirErroIdentity(e.Code, e.Description)).ToList();
            return Resultado<bool>.Falha(erros);
        }

        codigoVerificacao.UsadoEm = DateTime.UtcNow;

        await RevogarFamiliaTokensAsync(usuario.Id);

        // Log
        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = usuario.Id,
            Acao = "RESETAR_SENHA",
            TipoEntidade = "Usuario",
            EntidadeId = usuario.Id.ToString()
        });
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Senha resetada com sucesso! Faça login com a nova senha.");
    }

    //REVOGAR TOKEN (LOGOUT)
    public async Task<Resultado<bool>> RevogarTokenAsync(Guid usuarioId)
    {
        await RevogarFamiliaTokensAsync(usuarioId);

        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = usuarioId,
            Acao = "LOGOUT",
            TipoEntidade = "Usuario",
            EntidadeId = usuarioId.ToString()
        });
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Logout realizado. Todos os tokens foram revogados.");
    }

    private async Task<AuthResponse> GerarTokensAsync(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email!),
            new(ClaimTypes.Name, usuario.NomeCompleto),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("telefone", usuario.TelefoneWhatsApp ?? "")
        };

        var roles = await _userManager.GetRolesAsync(usuario);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiracao = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiracaoMinutos"]!));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Emissor"],
            audience: _config["Jwt:Audiencia"],
            claims: claims,
            expires: expiracao,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = GerarRefreshToken();
        _context.TokensAtualizacao.Add(new TokenAtualizacao
        {
            UsuarioId = usuario.Id,
            Token = refreshToken,
            ExpiraEm = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenDias"]!))
        });
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = tokenString,
            RefreshToken = refreshToken,
            ExpiraEm = expiracao,
            Usuario = MapearUsuarioResponse(usuario)
        };
    }

    private static string GerarRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task RevogarFamiliaTokensAsync(Guid usuarioId)
    {
        var tokens = await _context.TokensAtualizacao
            .Where(t => t.UsuarioId == usuarioId && t.RevogadoEm == null)
            .ToListAsync();

        foreach (var token in tokens)
            token.RevogadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private ClaimsPrincipal? ObterPrincipalDoTokenExpirado(string token)
    {
        var parametros = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = _config["Jwt:Emissor"],
            ValidateAudience = true,
            ValidAudience = _config["Jwt:Audiencia"],
            ValidateLifetime = false
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, parametros, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static UsuarioResponse MapearUsuarioResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email!,
            TelefoneWhatsApp = usuario.TelefoneWhatsApp,
            FotoUrl = usuario.FotoUrl
        };
    }

    private static string TraduzirErroIdentity(string code, string defaultMessage)
    {
        return code switch
        {
            "DuplicateUserName" => "Este e-mail já está cadastrado.",
            "DuplicateEmail" => "Este e-mail já está cadastrado.",
            "PasswordTooShort" => "A senha deve ter no mínimo 6 caracteres.",
            "PasswordRequiresDigit" => "A senha deve conter pelo menos um número.",
            "PasswordRequiresLower" => "A senha deve conter pelo menos uma letra minúscula.",
            "PasswordRequiresUpper" => "A senha deve conter pelo menos uma letra maiúscula.",
            "PasswordRequiresNonAlphanumeric" => "A senha deve conter pelo menos um caractere especial.",
            "PasswordMismatch" => "Senha atual incorreta.",
            "InvalidEmail" => "E-mail inválido.",
            "UserLockedOut" => "Conta bloqueada. Tente novamente mais tarde.",
            _ => defaultMessage
        };
    }
}