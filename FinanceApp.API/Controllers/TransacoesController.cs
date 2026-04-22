using FinanceApp.Application.DTOs.Transacoes;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransacoesController : BaseController
{
    private readonly ITransacaoService _service;

    public TransacoesController(ITransacaoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] FiltroTransacaoRequest filtro)
        => RespostaDe(await _service.ListarAsync(UsuarioIdAtual, filtro));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
        => RespostaDe(await _service.ObterPorIdAsync(UsuarioIdAtual, id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarTransacaoRequest request)
        => RespostaDe(await _service.CriarAsync(UsuarioIdAtual, request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarTransacaoRequest request)
        => RespostaDe(await _service.AtualizarAsync(UsuarioIdAtual, id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
        => RespostaDe(await _service.ExcluirAsync(UsuarioIdAtual, id));
}