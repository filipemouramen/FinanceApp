namespace FinanceApp.Application.DTOs.Dashboard;

public class DashboardResponse
{
    public ResumoFinanceiroResponse Resumo { get; set; } = new();
    public List<GastoPorCategoriaResponse> GastosPorCategoria { get; set; } = new();
    public List<BalancoMensalResponse> BalancoUltimos6Meses { get; set; } = new();
    public List<TransacaoRecenteResponse> UltimasTransacoes { get; set; } = new();
    public List<OrcamentoResumoResponse> Orcamentos { get; set; } = new();
    public List<MetaResumoResponse> Metas { get; set; } = new();
    public List<FaturaResumoResponse> ProximasFaturas { get; set; } = new();
    public List<ContaSaldoResponse> Contas { get; set; } = new();
}

public class ResumoFinanceiroResponse
{
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal TotalTransferencias { get; set; }
    public decimal Saldo => TotalReceitas - TotalDespesas;
    public decimal SaldoTotalContas { get; set; }
    public int TotalTransacoesMes { get; set; }
    public decimal MediaDiariaDespesas { get; set; }
    public decimal MaiorDespesaMes { get; set; }
    public string? CategoriaMaisGasta { get; set; }
}

public class GastoPorCategoriaResponse
{
    public int CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public string IconeCategoria { get; set; } = string.Empty;
    public string CorCategoria { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public decimal Percentual { get; set; }
    public int QuantidadeTransacoes { get; set; }
}

public class BalancoMensalResponse
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string NomeMes { get; set; } = string.Empty;
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Saldo => TotalReceitas - TotalDespesas;
}

public class TransacaoRecenteResponse
{
    public int Id { get; set; }
    public string? Descricao { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public string IconeCategoria { get; set; } = string.Empty;
    public string CorCategoria { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public DateOnly DataTransacao { get; set; }
}

public class OrcamentoResumoResponse
{
    public int Id { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public string IconeCategoria { get; set; } = string.Empty;
    public string CorCategoria { get; set; } = string.Empty;
    public decimal ValorLimite { get; set; }
    public decimal TotalGasto { get; set; }
    public decimal PercentualUsado => ValorLimite > 0 ? Math.Round(TotalGasto / ValorLimite * 100, 1) : 0;
    public bool Estourado => TotalGasto > ValorLimite;
}

public class MetaResumoResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal ValorAlvo { get; set; }
    public decimal ValorAtual { get; set; }
    public decimal PercentualConcluido => ValorAlvo > 0 ? Math.Round(ValorAtual / ValorAlvo * 100, 1) : 0;
    public string? Cor { get; set; }
    public string? Icone { get; set; }
}

public class FaturaResumoResponse
{
    public int Id { get; set; }
    public string NomeCartao { get; set; } = string.Empty;
    public string CorCartao { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public decimal ValorPago { get; set; }
    public DateOnly DataVencimento { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DiasParaVencimento => (int)(DataVencimento.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).TotalDays;
}

public class ContaSaldoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Banco { get; set; }
    public string Cor { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public decimal SaldoAtual { get; set; }
}
