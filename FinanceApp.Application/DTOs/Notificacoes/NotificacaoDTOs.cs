using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.DTOs.Notificacoes;

public class NotificacaoResponse
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Lida { get; set; }
    public Guid? EntidadeRelacionadaId { get; set; }
    public DateTime CriadoEm { get; set; }
    public string TempoAtras { get; set; } = string.Empty;
}

public class ContadorNotificacoesResponse
{
    public int Total { get; set; }
    public int NaoLidas { get; set; }
}