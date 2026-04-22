using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class Anexo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TransacaoId { get; set; }
        public Guid UsuarioId { get; set; }
        public string NomeArquivo { get; set; } = string.Empty;
        public string UrlArquivo { get; set; } = string.Empty;
        public string TipoArquivo { get; set; } = string.Empty;
        public long? TamanhoBytes { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;


        public virtual Transacao Transacao { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
