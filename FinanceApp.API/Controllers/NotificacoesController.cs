using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificacoesController : BaseController
{
    private readonly INotificacaoService _service;

    public NotificacoesController(INotificacaoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool? apenasNaoLidas = null, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
        => RespostaDe(await _service.ListarAsync(UsuarioIdAtual, apenasNaoLidas, pagina, tamanhoPagina));

    [HttpGet("contador")]
    public async Task<IActionResult> Contar()
        => RespostaDe(await _service.ContarNaoLidasAsync(UsuarioIdAtual));

    [HttpPut("{id:int}/lida")]
    public async Task<IActionResult> MarcarComoLida(int id)
        => RespostaDe(await _service.MarcarComoLidaAsync(UsuarioIdAtual, id));

    [HttpPut("marcar-todas-lidas")]
    public async Task<IActionResult> MarcarTodasComoLidas()
        => RespostaDe(await _service.MarcarTodasComoLidasAsync(UsuarioIdAtual));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
        => RespostaDe(await _service.ExcluirAsync(UsuarioIdAtual, id));
}
