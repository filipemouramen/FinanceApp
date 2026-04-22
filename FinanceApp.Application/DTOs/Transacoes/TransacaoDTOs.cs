using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Application.DTOs.Transacoes
{
    public class CriarTransacaoRequest
    {
        [Required]
        public int CategoriaId { get; set; }

        public Guid? ContaId { get; set; }
        public int? FormaPagamentoId { get; set; }
        public Guid? CartaoCreditoId { get; set; }

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
        public Guid? ContaId { get; set; }
        public int? FormaPagamentoId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Valor { get; set; }

        [StringLength(300)]
        public string? Descricao { get; set; }

        public DateOnly? DataTransacao { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }
        public string? Status { get; set; }
    }

    public class TransacaoResponse
    {
        public Guid Id { get; set; }
        public int CategoriaId { get; set; }
        public string NomeCategoria { get; set; } = string.Empty;
        public string IconeCategoria { get; set; } = string.Empty;
        public string CorCategoria { get; set; } = string.Empty;
        public Guid? ContaId { get; set; }
        public string? NomeConta { get; set; }
        public int? FormaPagamentoId { get; set; }
        public string? NomeFormaPagamento { get; set; }
        public Guid? CartaoCreditoId { get; set; }
        public string? NomeCartaoCredito { get; set; }
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
        public DateTime CriadoEm { get; set; }
    }

    public class FiltroTransacaoRequest
    {
        public DateOnly? DataInicio { get; set; }
        public DateOnly? DataFim { get; set; }
        public string? Tipo { get; set; }
        public int? CategoriaId { get; set; }
        public Guid? ContaId { get; set; }
        public Guid? CartaoCreditoId { get; set; }
        public string? Origem { get; set; }
        public string? Busca { get; set; }
        public string? Status { get; set; }

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
}