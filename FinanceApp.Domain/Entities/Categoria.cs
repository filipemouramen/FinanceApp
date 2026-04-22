using FinanceApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;
        public string Cor { get; set; } = "#6C63FF";
        public TipoTransacao Tipo { get; set; } = TipoTransacao.DESPESA;
        public bool Padrao { get; set; } = true;
        public Guid? UsuarioId { get; set; } 
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public virtual Usuario? Usuario { get; set; }
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public virtual ICollection<ApelidoCategoria> Apelidos { get; set; } = new List<ApelidoCategoria>();

    }
}
