using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Application.DTOs.Contas;

public class CriarContaRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string TipoConta { get; set; } = "CORRENTE";

    [StringLength(80)]
    public string? Banco { get; set; }

    [StringLength(7)]
    public string Cor { get; set; } = "#6C63FF";

    [StringLength(50)]
    public string Icone { get; set; } = "wallet";

    public decimal SaldoInicial { get; set; }
    public bool Principal { get; set; }
}

public class AtualizarContaRequest
{
    [StringLength(100)]
    public string? Nome { get; set; }

    [StringLength(80)]
    public string? Banco { get; set; }

    [StringLength(7)]
    public string? Cor { get; set; }

    [StringLength(50)]
    public string? Icone { get; set; }

    public bool? Principal { get; set; }
    public decimal? SaldoInicial { get; set; }
}

public class ContaResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TipoConta { get; set; } = string.Empty;
    public string? Banco { get; set; }
    public string Cor { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public bool Principal { get; set; }
    public bool Ativo { get; set; }
    public bool TemCartaoVinculado { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class TransferenciaRequest
{
    [Required]
    public int ContaOrigemId { get; set; }

    [Required]
    public int ContaDestinoId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [StringLength(300)]
    public string? Descricao { get; set; }

    public DateOnly? Data { get; set; }
}

public class TransferenciaResponse
{
    public int Id { get; set; }
    public string ContaOrigem { get; set; } = string.Empty;
    public string ContaDestino { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string? Descricao { get; set; }
    public DateOnly DataTransferencia { get; set; }
}
