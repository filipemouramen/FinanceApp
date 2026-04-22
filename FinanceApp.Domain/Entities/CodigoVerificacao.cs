using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities
{
    public class CodigoVerificacao
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public FinalidadeCodigo Finalidade { get; set; }
        public DateTime ExpiraEm { get; set; }
        public DateTime? UsadoEm { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public bool Expirado => DateTime.UtcNow >= ExpiraEm;
        public bool Valido => UsadoEm == null && !Expirado;

        public virtual Usuario Usuario { get; set; } = null!;
    }
}