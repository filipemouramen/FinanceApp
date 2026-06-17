namespace FinanceApp.Domain.Entities;

public class LancamentoMeta
{
    public int Id { get; set; }
    public int MetaEconomiaId { get; set; }
    public decimal Valor { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public virtual MetaEconomia MetaEconomia { get; set; } = null!;
}
