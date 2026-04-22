using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class ConfiguracaoUsuario
    {
        public Guid UsuarioId { get; set; }
        public string Moeda { get; set; } = "BRL";
        public int DiaInicioMes { get; set; } = 1;
        public bool WhatsAppAtivado { get; set; }
        public bool NotificacoesPush { get; set; } = true;
        public bool AlertasOrcamento { get; set; } = true;
        public bool AlertasFatura { get; set; } = true;
        public bool ModoEscuro { get; set; }
        public string Idioma { get; set; } = "pt-BR";
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;


        public virtual Usuario Usuario { get; set; } = null!;
    }
}