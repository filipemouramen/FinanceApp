namespace FinanceApp.Domain.Entities;

public class TransferenciaConta
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int ContaOrigemId { get; set; }
    public int ContaDestinoId { get; set; }
    public decimal Valor { get; set; }
    public string? Descricao { get; set; }
    public DateOnly DataTransferencia { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Conta ContaOrigem { get; set; } = null!;
    public virtual Conta ContaDestino { get; set; } = null!;
    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}
