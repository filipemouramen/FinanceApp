using System.Security.Claims;
using FinanceApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected int UsuarioIdAtual =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Usuário não autenticado."));

    protected IActionResult RespostaDe<T>(Resultado<T> resultado)
    {
        if (resultado.Sucesso)
        {
            return resultado.StatusCode switch
            {
                201 => Created("", resultado),
                _ => Ok(resultado)
            };
        }

        return resultado.StatusCode switch
        {
            401 => Unauthorized(resultado),
            403 => StatusCode(403, resultado),
            404 => NotFound(resultado),
            422 => StatusCode(422, resultado),
            423 => StatusCode(423, resultado),
            _ => BadRequest(resultado)
        };
    }
}
