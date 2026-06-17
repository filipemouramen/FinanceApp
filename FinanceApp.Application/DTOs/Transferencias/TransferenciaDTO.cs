namespace FinanceApp.Application.DTOs.Transferencias;

public class CriarTransferenciaDTO
{
    public int ContaOrigemId { get; set; }
    public int ContaDestinoId { get; set; }
    public decimal Valor { get; set; }
    public DateOnly DataTransferencia { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string? Descricao { get; set; }
}

public class ContaTransferenciaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cor { get; set; }
    public decimal? NovoSaldo { get; set; }
    public decimal? SaldoRestaurado { get; set; }
}

public class TransferenciaResponseDTO
{
    public int Id { get; set; }
    public ContaTransferenciaDTO ContaOrigem { get; set; } = null!;
    public ContaTransferenciaDTO ContaDestino { get; set; } = null!;
    public decimal Valor { get; set; }
    public DateOnly DataTransferencia { get; set; }
    public string? Descricao { get; set; }
    public int? TransacaoOrigemId { get; set; }
    public int? TransacaoDestinoId { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class FiltroTransferenciaDTO
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public int? Mes { get; set; }
    public int? Ano { get; set; }
}

public class CancelamentoTransferenciaResponse
{
    public string Mensagem { get; set; } = string.Empty;
    public ContaTransferenciaDTO ContaOrigem { get; set; } = null!;
    public ContaTransferenciaDTO ContaDestino { get; set; } = null!;
}
