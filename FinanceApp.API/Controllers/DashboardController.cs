using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ObterAtual()
    {
        var hoje = DateTime.UtcNow;
        return RespostaDe(await _service.ObterAsync(UsuarioIdAtual, hoje.Month, hoje.Year));
    }

    [HttpGet("{mes:int}/{ano:int}")]
    public async Task<IActionResult> ObterPorMes(int mes, int ano)
        => RespostaDe(await _service.ObterAsync(UsuarioIdAtual, mes, ano));
}