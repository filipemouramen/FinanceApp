using FinanceApp.Application.DTOs.Contas;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContasController : BaseController
{
    private readonly IContaService _service;

    public ContasController(IContaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
        => RespostaDe(await _service.ListarAsync(UsuarioIdAtual));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
        => RespostaDe(await _service.ObterPorIdAsync(UsuarioIdAtual, id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarContaRequest request)
        => RespostaDe(await _service.CriarAsync(UsuarioIdAtual, request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarContaRequest request)
        => RespostaDe(await _service.AtualizarAsync(UsuarioIdAtual, id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
        => RespostaDe(await _service.ExcluirAsync(UsuarioIdAtual, id));

    [HttpPost("transferir")]
    public async Task<IActionResult> Transferir([FromBody] TransferenciaRequest request)
        => RespostaDe(await _service.TransferirAsync(UsuarioIdAtual, request));
}