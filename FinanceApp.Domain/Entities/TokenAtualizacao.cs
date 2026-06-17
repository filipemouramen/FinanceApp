namespace FinanceApp.Domain.Entities;

public class TokenAtualizacao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? RevogadoEm { get; set; }
    public string? SubstituidoPor { get; set; }

    public bool Expirado => DateTime.UtcNow >= ExpiraEm;
    public bool Ativo => RevogadoEm == null && !Expirado;

    public virtual Usuario Usuario { get; set; } = null!;
}
