using FinanceApp.Domain.Interfaces;

namespace FinanceApp.Domain.Entities;

public class CartaoCredito : ISoftDeletable
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int? ContaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Bandeira { get; set; }
    public string? UltimosDigitos { get; set; }
    public decimal LimiteTotal { get; set; }
    public decimal LimiteDisponivel { get; set; }
    public int DiaFechamento { get; set; }
    public int DiaVencimento { get; set; }
    public string Cor { get; set; } = "#FF4757";
    public bool Ativo { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Conta? Conta { get; set; }
    public virtual ICollection<FaturaCartao> Faturas { get; set; } = new List<FaturaCartao>();
    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public virtual ICollection<Parcelamento> Parcelamentos { get; set; } = new List<Parcelamento>();
}
