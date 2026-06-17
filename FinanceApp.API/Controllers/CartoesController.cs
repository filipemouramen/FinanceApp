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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
        => RespostaDe(await _service.ObterPorIdAsync(UsuarioIdAtual, id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarCartaoCreditoRequest request)
        => RespostaDe(await _service.CriarAsync(UsuarioIdAtual, request));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarCartaoCreditoRequest request)
        => RespostaDe(await _service.AtualizarAsync(UsuarioIdAtual, id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
        => RespostaDe(await _service.ExcluirAsync(UsuarioIdAtual, id));

    [HttpGet("{id:int}/faturas")]
    public async Task<IActionResult> ListarFaturas(int id)
        => RespostaDe(await _service.ListarFaturasAsync(UsuarioIdAtual, id));

    [HttpGet("{cartaoId:int}/faturas/{faturaId:int}")]
    public async Task<IActionResult> ObterFaturaPorId(int cartaoId, int faturaId)
        => RespostaDe(await _service.ObterFaturaPorIdAsync(UsuarioIdAtual, cartaoId, faturaId));

    [HttpGet("{id:int}/faturas/{mes:int}/{ano:int}")]
    public async Task<IActionResult> ObterFatura(int id, int mes, int ano)
        => RespostaDe(await _service.ObterFaturaAsync(UsuarioIdAtual, id, mes, ano));

    [HttpPost("{cartaoId:int}/faturas/{faturaId:int}/pagar")]
    public async Task<IActionResult> PagarFatura(int cartaoId, int faturaId, [FromBody] PagarFaturaRequest request)
        => RespostaDe(await _service.PagarFaturaAsync(UsuarioIdAtual, faturaId, request));
}
