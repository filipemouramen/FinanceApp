using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities
{
    public class Conta
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public TipoConta TipoConta { get; set; } = TipoConta.CORRENTE;
        public string? Banco { get; set; }
        public string Cor { get; set; } = "#6C63FF";
        public string Icone { get; set; } = "wallet";
        public decimal SaldoInicial { get; set; }
        public decimal SaldoAtual { get; set; }
        public bool Ativo { get; set; } = true;
        public bool Principal { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;


        public virtual Usuario Usuario { get; set; } = null!;
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public virtual ICollection<CartaoCredito> CartoesCredito { get; set; } = new List<CartaoCredito>();
    }
}