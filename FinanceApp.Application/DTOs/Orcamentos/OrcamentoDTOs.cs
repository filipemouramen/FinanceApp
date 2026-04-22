using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Application.DTOs.Orcamentos
{
    public class CriarOrcamentoRequest
    {
        [Required]
        public int CategoriaId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ValorLimite { get; set; }

        [Range(1, 12)]
        public int? Mes { get; set; }

        public int? Ano { get; set; }

        [Range(1, 100)]
        public int PercentualAlerta { get; set; } = 80;
    }

    public class AtualizarOrcamentoRequest
    {
        [Range(0.01, double.MaxValue)]
        public decimal? ValorLimite { get; set; }

        [Range(1, 100)]
        public int? PercentualAlerta { get; set; }
    }

    public class OrcamentoResponse
    {
        public Guid Id { get; set; }
        public int CategoriaId { get; set; }
        public string NomeCategoria { get; set; } = string.Empty;
        public string IconeCategoria { get; set; } = string.Empty;
        public string CorCategoria { get; set; } = string.Empty;
        public decimal ValorLimite { get; set; }
        public decimal TotalGasto { get; set; }
        public decimal ValorRestante => ValorLimite - TotalGasto;
        public decimal PercentualUsado => ValorLimite > 0 ? Math.Round(TotalGasto / ValorLimite * 100, 1) : 0;
        public int PercentualAlerta { get; set; }
        public bool Estourado => TotalGasto > ValorLimite;
        public bool EmAlerta => PercentualUsado >= PercentualAlerta;
        public int Mes { get; set; }
        public int Ano { get; set; }
    }
}