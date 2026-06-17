using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class FaturaCartao
{
    public int Id { get; set; }
    public int CartaoCreditoId { get; set; }
    public int UsuarioId { get; set; }
    public int MesReferencia { get; set; }
    public int AnoReferencia { get; set; }
    public DateOnly DataFechamento { get; set; }
    public DateOnly DataVencimento { get; set; }
    public DateOnly? DataPagamento { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorPago { get; set; }
    public StatusFatura Status { get; set; } = StatusFatura.ABERTA;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public virtual CartaoCredito CartaoCredito { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}
