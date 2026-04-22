using Microsoft.AspNetCore.Identity;    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class Usuario : IdentityUser<Guid>
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string? TelefoneWhatsApp { get; set; }
        public string? FotoUrl { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;


        public virtual ICollection<TokenAtualizacao> TokensAtualizacao { get; set; } = new List<TokenAtualizacao>();
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public virtual ICollection<Conta> Contas { get; set; } = new List<Conta>();
        public virtual ICollection<CartaoCredito> CartoesCredito { get; set; } = new List<CartaoCredito>();
        public virtual ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();
        public virtual ICollection<MetaEconomia> MetasEconomia { get; set; } = new List<MetaEconomia>();
        public virtual ICollection<RegraRecorrencia> RegrasRecorrencia { get; set; } = new List<RegraRecorrencia>();
        public virtual ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
        public virtual ConfiguracaoUsuario? Configuracoes { get; set; }
    }
}
