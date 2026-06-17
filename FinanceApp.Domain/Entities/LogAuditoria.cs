namespace FinanceApp.Domain.Entities;

public class LogAuditoria
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? TipoEntidade { get; set; }
    public string? EntidadeId { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNovo { get; set; }
    public string? Detalhes { get; set; }
    public string? EnderecoIP { get; set; }
    public string? Navegador { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
