using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities
{
    public class Notificacao
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public TipoNotificacao Tipo { get; set; }
        public bool Lida { get; set; }
        public Guid? EntidadeRelacionadaId { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;


        public virtual Usuario Usuario { get; set; } = null!;
    }
}