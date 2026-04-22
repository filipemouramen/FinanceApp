using FinanceApp.Application.DTOs.Configuracoes;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ConfiguracoesController : BaseController
{
    private readonly IConfiguracaoService _service;

    public ConfiguracoesController(IConfiguracaoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Obter()
        => RespostaDe(await _service.ObterAsync(UsuarioIdAtual));

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarConfiguracaoRequest request)
        => RespostaDe(await _service.AtualizarAsync(UsuarioIdAtual, request));
}