using FinanceApp.Application.DTOs.Exportacao;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExportacaoController : BaseController
{
    private readonly IExportacaoService _service;

    public ExportacaoController(IExportacaoService service)
    {
        _service = service;
    }

    [HttpPost("pdf")]
    public async Task<IActionResult> GerarPdf([FromBody] ExportacaoPdfRequestDTO request)
    {
        var resultado = await _service.GerarExtratoPdfAsync(
            UsuarioIdAtual, request.DataInicio, request.DataFim);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(new
        {
            sucesso = true,
            dados = new
            {
                base64 = Convert.ToBase64String(resultado.Dados!.Bytes),
                nomeArquivo = resultado.Dados.NomeArquivo
            }
        });
    }
}
