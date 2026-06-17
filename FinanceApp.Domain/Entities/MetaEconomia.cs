using FinanceApp.Domain.Interfaces;

namespace FinanceApp.Domain.Entities;

public class MetaEconomia : ISoftDeletable
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal ValorAlvo { get; set; }
    public decimal ValorAtual { get; set; }
    public DateOnly? DataLimite { get; set; }
    public string? Icone { get; set; }
    public string? Cor { get; set; } = "#6C63FF";
    public bool Concluida { get; set; }
    public DateTime? ConcluidaEm { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<LancamentoMeta> Lancamentos { get; set; } = new List<LancamentoMeta>();
}
