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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
        => RespostaDe(await _service.ObterPorIdAsync(UsuarioIdAtual, id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarTransacaoRequest request)
        => RespostaDe(await _service.CriarAsync(UsuarioIdAtual, request));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarTransacaoRequest request)
        => RespostaDe(await _service.AtualizarAsync(UsuarioIdAtual, id, request));

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusRequest request)
        => RespostaDe(await _service.AtualizarStatusAsync(UsuarioIdAtual, id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
        => RespostaDe(await _service.ExcluirAsync(UsuarioIdAtual, id));

    [HttpDelete("parcelamento/{parcelamentoId:int}")]
    public async Task<IActionResult> CancelarParcelamento(int parcelamentoId)
        => RespostaDe(await _service.CancelarParcelamentoAsync(UsuarioIdAtual, parcelamentoId));
}