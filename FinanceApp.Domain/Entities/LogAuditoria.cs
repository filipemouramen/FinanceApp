using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class LogAuditoria
    {
        public long Id { get; set; }
        public Guid? UsuarioId { get; set; }
        public string Acao { get; set; } = string.Empty;
        public string? TipoEntidade { get; set; }
        public string? EntidadeId { get; set; }
        public string? Detalhes { get; set; }
        public string? EnderecoIP { get; set; }
        public string? Navegador { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}