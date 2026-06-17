namespace FinanceApp.Application.DTOs.Notificacoes;

public class NotificacaoResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Lida { get; set; }
    public int? EntidadeRelacionadaId { get; set; }
    public DateTime CriadoEm { get; set; }
    public string TempoAtras { get; set; } = string.Empty;
}

public class ListaNotificacoesResponse
{
    public int TotalNaoLidas { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
    public List<NotificacaoResponse> Itens { get; set; } = new();
}

public class ContadorNotificacoesResponse
{
    public int Count { get; set; }
}
