namespace FinanceApp.Application.DTOs.Exportacao;

public class ExportacaoPdfRequestDTO
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
}

public class ExportacaoPdfResponseDTO
{
    public byte[] Bytes { get; set; } = [];
    public string NomeArquivo { get; set; } = string.Empty;
}
