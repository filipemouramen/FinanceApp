using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Application.DTOs.Auth;

public class RegistroRequest
{
    [Required(ErrorMessage = "O nome completo é obrigatório")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome completo deve conter entre 3 e 150 caracteres")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "A confirmação de senha é obrigatória")]
    [Compare("Senha", ErrorMessage = "As senhas não conferem")]
    public string ConfirmarSenha { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Telefone inválido")]
    public string? TelefoneWhatsApp { get; set; }
}

public class LoginRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Senha { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public UsuarioResponse Usuario { get; set; } = null!;
}

public class UsuarioResponse
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TelefoneWhatsApp { get; set; }
    public string? FotoUrl { get; set; }
}

public class AtualizarPerfilRequest
{
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome completo deve conter entre 3 e 150 caracteres")]
    public string? NomeCompleto { get; set; }

    [Phone]
    public string? TelefoneWhatsApp { get; set; }

    public string? FotoUrl { get; set; }
}

public class AlterarSenhaRequest
{
    [Required]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required]
    [Compare("NovaSenha", ErrorMessage = "As senhas não conferem")]
    public string ConfirmarNovaSenha { get; set; } = string.Empty;
}

public class ResetarSenhaRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NovaSenha { get; set; } = string.Empty;
}

public class SolicitarResetSenhaRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class VerificarCodigoRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Codigo { get; set; } = string.Empty;
}

public class VerificarCodigoResponse
{
    public string TokenTemporario { get; set; } = string.Empty;
    public int TentativasRestantes { get; set; }
}
