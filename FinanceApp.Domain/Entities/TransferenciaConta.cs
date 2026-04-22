using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities
{
    public class TransferenciaConta
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public Guid ContaOrigemId { get; set; }
        public Guid ContaDestinoId { get; set; }
        public decimal Valor { get; set; }
        public string? Descricao { get; set; }
        public DateOnly DataTransferencia { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Conta ContaOrigem { get; set; } = null!;
        public virtual Conta ContaDestino { get; set; } = null!;
    }
}