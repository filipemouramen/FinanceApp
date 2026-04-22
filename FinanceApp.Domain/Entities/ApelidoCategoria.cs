using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class ApelidoCategoria
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string Apelido { get; set; } = string.Empty;
        public Guid? UsuarioId { get; set; }


        public virtual Categoria Categoria { get; set; } = null!;
        public virtual Usuario? Usuario { get; set; }
    }
}