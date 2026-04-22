using FinanceApp.Application.DTOs.CartoesCredito;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartoesController : BaseController
{
    private readonly ICartaoCreditoService _service;

    public CartoesController(ICartaoCreditoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
        => RespostaDe(await _service.ListarAsync(UsuarioIdAtual));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarCartaoCreditoRequest request)
        => RespostaDe(await _service.CriarAsync(UsuarioIdAtual, request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCartaoCreditoRequest request)
        => RespostaDe(await _service.AtualizarAsync(UsuarioIdAtual, id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
        => RespostaDe(await _service.ExcluirAsync(UsuarioIdAtual, id));

    [HttpGet("{id:guid}/faturas")]
    public async Task<IActionResult> ListarFaturas(Guid id)
        => RespostaDe(await _service.ListarFaturasAsync(UsuarioIdAtual, id));

    [HttpGet("{id:guid}/faturas/{mes:int}/{ano:int}")]
    public async Task<IActionResult> ObterFatura(Guid id, int mes, int ano)
        => RespostaDe(await _service.ObterFaturaAsync(UsuarioIdAtual, id, mes, ano));

    [HttpPost("faturas/{faturaId:guid}/pagar")]
    public async Task<IActionResult> PagarFatura(Guid faturaId, [FromBody] PagarFaturaRequest request)
        => RespostaDe(await _service.PagarFaturaAsync(UsuarioIdAtual, faturaId, request));
}