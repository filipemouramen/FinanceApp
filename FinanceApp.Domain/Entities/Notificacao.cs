using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class Notificacao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public TipoNotificacao Tipo { get; set; }
    public bool Lida { get; set; }
    public int? EntidadeRelacionadaId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public virtual Usuario Usuario { get; set; } = null!;
}
