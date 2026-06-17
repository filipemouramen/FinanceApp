using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class Transacao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int CategoriaId { get; set; }
    public int? ContaId { get; set; }
    public int? FormaPagamentoId { get; set; }
    public int? CartaoCreditoId { get; set; }
    public int? FaturaCartaoId { get; set; }
    public int? TransferenciaContaId { get; set; }
    public int? RegraRecorrenciaId { get; set; }
    public int? ParcelamentoId { get; set; }
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
    public string? Descricao { get; set; }
    public DateOnly DataTransacao { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public OrigemTransacao Origem { get; set; } = OrigemTransacao.APP;
    public StatusTransacao Status { get; set; } = StatusTransacao.EFETIVADA;
    public string? Observacoes { get; set; }
    public bool Recorrente { get; set; }
    public int? NumeroParcela { get; set; }
    public int? TotalParcelas { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Categoria Categoria { get; set; } = null!;
    public virtual Conta? Conta { get; set; }
    public virtual FormaPagamento? FormaPagamento { get; set; }
    public virtual CartaoCredito? CartaoCredito { get; set; }
    public virtual FaturaCartao? FaturaCartao { get; set; }
    public virtual TransferenciaConta? TransferenciaConta { get; set; }
    public virtual RegraRecorrencia? RegraRecorrencia { get; set; }
    public virtual Parcelamento? Parcelamento { get; set; }
    public virtual ICollection<Anexo> Anexos { get; set; } = new List<Anexo>();
}
