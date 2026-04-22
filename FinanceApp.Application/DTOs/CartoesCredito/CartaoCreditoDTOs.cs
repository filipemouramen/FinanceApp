using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Application.DTOs.CartoesCredito
{
    public class CriarCartaoCreditoRequest
    {
        public Guid? ContaId { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Bandeira { get; set; }

        [StringLength(4)]
        public string? UltimosDigitos { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Limite { get; set; }

        [Required]
        [Range(1, 28)]
        public int DiaFechamento { get; set; }

        [Required]
        [Range(1, 28)]
        public int DiaVencimento { get; set; }

        [StringLength(7)]
        public string Cor { get; set; } = "#FF4757";
    }

    public class AtualizarCartaoCreditoRequest
    {
        public Guid? ContaId { get; set; }

        [StringLength(100)]
        public string? Nome { get; set; }

        [StringLength(30)]
        public string? Bandeira { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Limite { get; set; }

        [Range(1, 28)]
        public int? DiaFechamento { get; set; }

        [Range(1, 28)]
        public int? DiaVencimento { get; set; }

        [StringLength(7)]
        public string? Cor { get; set; }
    }

    public class CartaoCreditoResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Bandeira { get; set; }
        public string? UltimosDigitos { get; set; }
        public decimal Limite { get; set; }
        public decimal LimiteDisponivel { get; set; }
        public decimal LimiteUtilizado => Limite - LimiteDisponivel;
        public decimal PercentualUtilizado => Limite > 0 ? Math.Round(LimiteUtilizado / Limite * 100, 1) : 0;
        public int DiaFechamento { get; set; }
        public int DiaVencimento { get; set; }
        public string Cor { get; set; } = string.Empty;
        public string? NomeConta { get; set; }
        public bool Ativo { get; set; }
    }

    public class FaturaCartaoResponse
    {
        public Guid Id { get; set; }
        public Guid CartaoCreditoId { get; set; }
        public string NomeCartao { get; set; } = string.Empty;
        public int MesReferencia { get; set; }
        public int AnoReferencia { get; set; }
        public DateOnly DataFechamento { get; set; }
        public DateOnly DataVencimento { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ValorPago { get; set; }
        public decimal ValorRestante => ValorTotal - ValorPago;
        public string Status { get; set; } = string.Empty;
        public List<TransacaoFaturaResponse> Transacoes { get; set; } = new();
    }

    public class TransacaoFaturaResponse
    {
        public Guid Id { get; set; }
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
        public Guid ContaId { get; set; }
    }
}