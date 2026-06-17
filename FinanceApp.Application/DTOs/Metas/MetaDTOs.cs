using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Application.DTOs.Metas;

public class CriarMetaRequest
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(150, MinimumLength = 2)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal ValorAlvo { get; set; }

    public DateOnly? DataLimite { get; set; }

    [StringLength(50)]
    public string? Icone { get; set; }

    [StringLength(7)]
    public string? Cor { get; set; }
}

public class AtualizarMetaRequest
{
    [StringLength(150, MinimumLength = 2)]
    public string? Titulo { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? ValorAlvo { get; set; }

    public DateOnly? DataLimite { get; set; }

    [StringLength(50)]
    public string? Icone { get; set; }

    [StringLength(7)]
    public string? Cor { get; set; }
}

public class MetaResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal ValorAlvo { get; set; }
    public decimal ValorAtual { get; set; }
    public decimal ValorRestante => ValorAlvo - ValorAtual;
    public decimal PercentualConcluido => ValorAlvo > 0 ? Math.Round(ValorAtual / ValorAlvo * 100, 1) : 0;
    public DateOnly? DataLimite { get; set; }
    public int? DiasRestantes => DataLimite.HasValue
        ? (int)(DataLimite.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).TotalDays
        : null;
    public string? Icone { get; set; }
    public string? Cor { get; set; }
    public bool Concluida { get; set; }
    public DateTime CriadoEm { get; set; }
    public List<LancamentoMetaResponse> UltimosLancamentos { get; set; } = new();
}

public class LancamentoMetaRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [StringLength(300)]
    public string? Observacoes { get; set; }
}

public class LancamentoMetaResponse
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CriadoEm { get; set; }
}
