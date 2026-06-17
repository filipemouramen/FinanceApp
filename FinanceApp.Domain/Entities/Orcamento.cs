namespace FinanceApp.Domain.Entities;

public class Orcamento
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int CategoriaId { get; set; }
    public decimal ValorLimite { get; set; }
    public int Mes { get; set; }
    public int Ano { get; set; }
    public int PercentualAlerta { get; set; } = 80;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Categoria Categoria { get; set; } = null!;
}
