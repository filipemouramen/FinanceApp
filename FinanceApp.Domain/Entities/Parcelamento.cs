namespace FinanceApp.Domain.Entities;

public class Parcelamento
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int? CartaoCreditoId { get; set; }
    public int CategoriaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public decimal ValorParcela { get; set; }
    public int TotalParcelas { get; set; }
    public int ParcelasPagas { get; set; }
    public DateOnly DataPrimeiraParcela { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual CartaoCredito? CartaoCredito { get; set; }
    public virtual Categoria Categoria { get; set; } = null!;
    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}
