using FinanceApp.Application.DTOs.Orcamentos;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrcamentosController : BaseController
{
    private readonly IOrcamentoService _service;

    public OrcamentosController(IOrcamentoService service)
    {
        _service = service;
    }

    [HttpGet("{mes:int}/{ano:int}")]
    public async Task<IActionResult> ListarPorMes(int mes, int ano)
        => RespostaDe(await _service.ListarPorMesAsync(UsuarioIdAtual, mes, ano));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarOrcamentoRequest request)
        => RespostaDe(await _service.CriarAsync(UsuarioIdAtual, request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarOrcamentoRequest request)
        => RespostaDe(await _service.AtualizarAsync(UsuarioIdAtual, id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
        => RespostaDe(await _service.ExcluirAsync(UsuarioIdAtual, id));
}