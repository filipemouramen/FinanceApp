using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class LancamentoMeta
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MetaEconomiaId { get; set; }
        public decimal Valor { get; set; }
        public string? Observacoes { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;


        public virtual MetaEconomia MetaEconomia { get; set; } = null!;
    }
}