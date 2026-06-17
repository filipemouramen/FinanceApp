using FinanceApp.Domain.Enums;
using FinanceApp.Domain.Interfaces;

namespace FinanceApp.Domain.Entities;

public class Conta : ISoftDeletable
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoConta TipoConta { get; set; } = TipoConta.CORRENTE;
    public string? Banco { get; set; }
    public string Cor { get; set; } = "#6C63FF";
    public string Icone { get; set; } = "wallet";
    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public bool Ativo { get; set; } = true;
    public bool Principal { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public virtual ICollection<CartaoCredito> CartoesCredito { get; set; } = new List<CartaoCredito>();
}
