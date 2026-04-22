using FinanceApp.Application.DTOs.Metas;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MetasController : BaseController
{
    private readonly IMetaService _service;

    public MetasController(IMetaService service)
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
    public async Task<IActionResult> Criar([FromBody] CriarMetaRequest request)
        => RespostaDe(await _service.CriarAsync(UsuarioIdAtual, request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarMetaRequest request)
        => RespostaDe(await _service.AtualizarAsync(UsuarioIdAtual, id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
        => RespostaDe(await _service.ExcluirAsync(UsuarioIdAtual, id));

    [HttpPost("{id:guid}/lancamentos")]
    public async Task<IActionResult> AdicionarLancamento(Guid id, [FromBody] LancamentoMetaRequest request)
        => RespostaDe(await _service.AdicionarLancamentoAsync(UsuarioIdAtual, id, request));
}