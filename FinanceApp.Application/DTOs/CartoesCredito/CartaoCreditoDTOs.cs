using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Application.DTOs.CartoesCredito;

public class CriarCartaoCreditoRequest
{
    public int? ContaId { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Bandeira { get; set; }

    [StringLength(4)]
    public string? UltimosDigitos { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal LimiteTotal { get; set; }

    [Required]
    [Range(1, 30)]
    public int DiaFechamento { get; set; }

    [Required]
    [Range(1, 30)]
    public int DiaVencimento { get; set; }

    [StringLength(7)]
    public string Cor { get; set; } = "#FF4757";
}

public class AtualizarCartaoCreditoRequest
{
    public int? ContaId { get; set; }

    [StringLength(100)]
    public string? Nome { get; set; }

    [StringLength(30)]
    public string? Bandeira { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? LimiteTotal { get; set; }

    [Range(1, 30)]
    public int? DiaFechamento { get; set; }

    [Range(1, 30)]
    public int? DiaVencimento { get; set; }

    [StringLength(7)]
    public string? Cor { get; set; }
}

public class CartaoCreditoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Bandeira { get; set; }
    public string? UltimosDigitos { get; set; }
    public decimal LimiteTotal { get; set; }
    public decimal LimiteDisponivel { get; set; }
    public decimal LimiteUtilizado => LimiteTotal - LimiteDisponivel;
    public decimal PercentualUtilizado => LimiteTotal > 0 ? Math.Round(LimiteUtilizado / LimiteTotal * 100, 1) : 0;
    public int DiaFechamento { get; set; }
    public int DiaVencimento { get; set; }
    public string Cor { get; set; } = string.Empty;
    public int? ContaId { get; set; }
    public string? NomeConta { get; set; }
    public bool Ativo { get; set; }
}

public class FaturaCartaoResponse
{
    public int Id { get; set; }
    public int CartaoCreditoId { get; set; }
    public string NomeCartao { get; set; } = string.Empty;
    public int MesReferencia { get; set; }
    public int AnoReferencia { get; set; }
    public DateOnly DataFechamento { get; set; }
    public DateOnly DataVencimento { get; set; }
    public DateOnly? DataPagamento { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorPago { get; set; }
    public decimal ValorRestante => ValorTotal - ValorPago;
    public string Status { get; set; } = string.Empty;
    public List<TransacaoFaturaResponse> Transacoes { get; set; } = new();
}

public class TransacaoFaturaResponse
{
    public int Id { get; set; }
    public string? Descricao { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public string CorCategoria { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly DataTransacao { get; set; }
    public int? NumeroParcela { get; set; }
    public int? TotalParcelas { get; set; }
}

public class PagarFaturaRequest
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Valor { get; set; }

    [Required]
    public int ContaId { get; set; }

    public DateOnly? DataPagamento { get; set; }
}
