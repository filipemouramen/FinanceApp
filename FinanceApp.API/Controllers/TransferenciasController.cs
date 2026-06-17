using FinanceApp.Application.DTOs.Transferencias;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransferenciasController : BaseController
{
    private readonly ITransferenciaService _service;

    public TransferenciasController(ITransferenciaService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarTransferenciaDTO dto)
        => RespostaDe(await _service.CriarAsync(UsuarioIdAtual, dto));

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] FiltroTransferenciaDTO filtro)
        => RespostaDe(await _service.ListarAsync(UsuarioIdAtual, filtro));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id)
        => RespostaDe(await _service.CancelarAsync(id, UsuarioIdAtual));
}
