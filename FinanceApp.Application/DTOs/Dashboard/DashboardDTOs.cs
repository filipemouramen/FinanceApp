using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.DTOs.Dashboard
{
    // ===== DASHBOARD PRINCIPAL =====
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

    // ===== RESUMO FINANCEIRO DO MES =====
    public class ResumoFinanceiroResponse
    {
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal Saldo => TotalReceitas - TotalDespesas;
        public decimal SaldoTotalContas { get; set; }
        public int TotalTransacoesMes { get; set; }
        public decimal MediaDiariaDespesas { get; set; }
        public decimal MaiorDespesaMes { get; set; }
        public string? CategoriaMaisGasta { get; set; }
    }

    // ===== GASTO POR CATEGORIA =====
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

    // ===== BALANCO MENSAL =====
    public class BalancoMensalResponse
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string NomeMes { get; set; } = string.Empty;
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal Saldo => TotalReceitas - TotalDespesas;
    }

    // ===== TRANSACAO RECENTE =====
    public class TransacaoRecenteResponse
    {
        public Guid Id { get; set; }
        public string? Descricao { get; set; }
        public string NomeCategoria { get; set; } = string.Empty;
        public string IconeCategoria { get; set; } = string.Empty;
        public string CorCategoria { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public DateOnly DataTransacao { get; set; }
    }

    // ===== ORCAMENTO RESUMO =====
    public class OrcamentoResumoResponse
    {
        public string NomeCategoria { get; set; } = string.Empty;
        public string IconeCategoria { get; set; } = string.Empty;
        public string CorCategoria { get; set; } = string.Empty;
        public decimal ValorLimite { get; set; }
        public decimal TotalGasto { get; set; }
        public decimal PercentualUsado => ValorLimite > 0 ? Math.Round(TotalGasto / ValorLimite * 100, 1) : 0;
        public bool Estourado => TotalGasto > ValorLimite;
    }

    // ===== META RESUMO =====
    public class MetaResumoResponse
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public decimal ValorAlvo { get; set; }
        public decimal ValorAtual { get; set; }
        public decimal PercentualConcluido => ValorAlvo > 0 ? Math.Round(ValorAtual / ValorAlvo * 100, 1) : 0;
        public string? Cor { get; set; }
        public string? Icone { get; set; }
    }

    // ===== FATURA RESUMO =====
    public class FaturaResumoResponse
    {
        public Guid Id { get; set; }
        public string NomeCartao { get; set; } = string.Empty;
        public string CorCartao { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public decimal ValorPago { get; set; }
        public DateOnly DataVencimento { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DiasParaVencimento => (int)(DataVencimento.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).TotalDays;
    }

    // ===== SALDO POR CONTA =====
    public class ContaSaldoResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Banco { get; set; }
        public string Cor { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;
        public decimal SaldoAtual { get; set; }
    }
}