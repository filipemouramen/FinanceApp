using FinanceApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class RegraRecorrencia
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public int CategoriaId { get; set; }
        public Guid? ContaId { get; set; }
        public int? FormaPagamentoId { get; set; }
        public decimal Valor { get; set; }
        public TipoTransacao Tipo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public FrequenciaRecorrencia Frequencia { get; set; }
        public int? DiaMes { get; set; }
        public DateOnly DataInicio { get; set; }
        public DateOnly? DataFim { get; set; }
        public bool Ativo { get; set; } = true;
        public DateOnly? UltimaGeracaoEm { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;


        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Categoria Categoria { get; set; } = null!;
        public virtual Conta? Conta { get; set; }
        public virtual FormaPagamento? FormaPagamento { get; set; }
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}