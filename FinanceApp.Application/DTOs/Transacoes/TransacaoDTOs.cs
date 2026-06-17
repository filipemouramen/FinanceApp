using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Application.DTOs.Transacoes;

public class CriarTransacaoRequest
{
    [Required]
    public int CategoriaId { get; set; }

    public int? ContaId { get; set; }
    public int? FormaPagamentoId { get; set; }
    public int? CartaoCreditoId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [Required]
    public string Tipo { get; set; } = "DESPESA";

    [StringLength(300)]
    public string? Descricao { get; set; }

    public DateOnly? DataTransacao { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public bool Agendar { get; set; } = false;

    [Range(2, 210, ErrorMessage = "Parcelas devem ser entre 2 e 210")]
    public int? TotalParcelas { get; set; }
}

public class AtualizarTransacaoRequest
{
    public int? CategoriaId { get; set; }
    public int? ContaId { get; set; }
    public int? FormaPagamentoId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Valor { get; set; }

    [StringLength(300)]
    public string? Descricao { get; set; }

    public DateOnly? DataTransacao { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}

public class AtualizarStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class TransacaoResponse
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public string IconeCategoria { get; set; } = string.Empty;
    public string CorCategoria { get; set; } = string.Empty;
    public int? ContaId { get; set; }
    public string? NomeConta { get; set; }
    public int? FormaPagamentoId { get; set; }
    public string? NomeFormaPagamento { get; set; }
    public int? CartaoCreditoId { get; set; }
    public string? NomeCartaoCredito { get; set; }
    public int? TransferenciaContaId { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateOnly DataTransacao { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Atrasada { get; set; }
    public string? Observacoes { get; set; }
    public bool Recorrente { get; set; }
    public int? NumeroParcela { get; set; }
    public int? TotalParcelas { get; set; }
    public int? ParcelamentoId { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class FiltroTransacaoRequest
{
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public int? Mes { get; set; }
    public int? Ano { get; set; }
    public string? Tipo { get; set; }
    public int? CategoriaId { get; set; }
    public List<int>? CategoriasIds { get; set; }
    public int? ContaId { get; set; }
    public int? CartaoCreditoId { get; set; }
    public string? Origem { get; set; }
    public string? Busca { get; set; }
    public string? Status { get; set; }
    public bool IncluirTransferencias { get; set; } = false;

    [Range(1, int.MaxValue)]
    public int Pagina { get; set; } = 1;

    [Range(1, 100)]
    public int ItensPorPagina { get; set; } = 20;
}

public class ListaPaginada<T>
{
    public List<T> Itens { get; set; } = new();
    public int TotalItens { get; set; }
    public int Pagina { get; set; }
    public int ItensPorPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling(TotalItens / (double)ItensPorPagina);
    public bool TemAnterior => Pagina > 1;
    public bool TemProximo => Pagina < TotalPaginas;
}

public class ListaTransacoesResponse
{
    public List<TransacaoResponse> Itens { get; set; } = new();
    public int TotalItens { get; set; }
    public int Pagina { get; set; }
    public int ItensPorPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling(TotalItens / (double)ItensPorPagina);
    public bool TemAnterior => Pagina > 1;
    public bool TemProximo => Pagina < TotalPaginas;
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal SaldoPeriodo => TotalReceitas - TotalDespesas;
}
