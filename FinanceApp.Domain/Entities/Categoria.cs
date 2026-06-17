using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public string Cor { get; set; } = "#6C63FF";
    public TipoTransacao Tipo { get; set; } = TipoTransacao.DESPESA;
    public bool Padrao { get; set; } = true;
    public int? UsuarioId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public virtual Usuario? Usuario { get; set; }
    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public virtual ICollection<ApelidoCategoria> Apelidos { get; set; } = new List<ApelidoCategoria>();
}
