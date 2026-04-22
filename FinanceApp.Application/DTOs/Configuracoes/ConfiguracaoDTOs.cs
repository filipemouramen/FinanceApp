using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.DTOs.Configuracoes;

public class ConfiguracaoResponse
{
    public string Moeda { get; set; } = "BRL";
    public int DiaInicioMes { get; set; }
    public bool WhatsAppAtivado { get; set; }
    public bool NotificacoesPush { get; set; }
    public bool AlertasOrcamento { get; set; }
    public bool AlertasFatura { get; set; }
    public bool ModoEscuro { get; set; }
    public string Idioma { get; set; } = "pt-BR";
}

public class AtualizarConfiguracaoRequest
{
    public string? Moeda { get; set; }
    public int? DiaInicioMes { get; set; }
    public bool? WhatsAppAtivado { get; set; }
    public bool? NotificacoesPush { get; set; }
    public bool? AlertasOrcamento { get; set; }
    public bool? AlertasFatura { get; set; }
    public bool? ModoEscuro { get; set; }
    public string? Idioma { get; set; }
}