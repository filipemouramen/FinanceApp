namespace FinanceApp.Domain.Entities;

public class ApelidoCategoria
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public string Apelido { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }

    public virtual Categoria Categoria { get; set; } = null!;
    public virtual Usuario? Usuario { get; set; }
}
