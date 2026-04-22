using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class MensagemWhatsApp
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UsuarioId { get; set; }
        public string NumeroTelefone { get; set; } = string.Empty;
        public string MensagemOriginal { get; set; } = string.Empty;
        public string? CategoriaIdentificada { get; set; }
        public decimal? ValorIdentificado { get; set; }
        public bool ProcessadoComSucesso { get; set; }
        public Guid? TransacaoId { get; set; }
        public string? MensagemErro { get; set; }
        public DateTime ProcessadoEm { get; set; } = DateTime.UtcNow;


        public virtual Usuario? Usuario { get; set; }
        public virtual Transacao? Transacao { get; set; }
    }
}