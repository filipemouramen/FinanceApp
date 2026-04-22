using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class CartaoCredito
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public Guid? ContaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Bandeira { get; set; }
        public string? UltimosDigitos { get; set; }
        public decimal Limite { get; set; }
        public decimal LimiteDisponivel { get; set; }
        public int DiaFechamento { get; set; }
        public int DiaVencimento { get; set; }
        public string Cor { get; set; } = "#FF4757";
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;


        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Conta? Conta { get; set; }
        public virtual ICollection<FaturaCartao> Faturas { get; set; } = new List<FaturaCartao>();
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public virtual ICollection<Parcelamento> Parcelamentos { get; set; } = new List<Parcelamento>();
    }
}
