using FinanceApp.Application.DTOs.Contas;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;

public class ContaService : IContaService
{
    private readonly FinanceDbContext _context;

    public ContaService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Resultado<List<ContaResponse>>> ListarAsync(int usuarioId)
    {
        var contas = await _context.Contas
            .Where(c => c.UsuarioId == usuarioId && c.Ativo)
            .OrderByDescending(c => c.Principal)
            .ThenBy(c => c.Nome)
            .Select(c => new ContaResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                TipoConta = c.TipoConta.ToString(),
                Banco = c.Banco,
                Cor = c.Cor,
                Icone = c.Icone,
                SaldoInicial = c.SaldoInicial,
                SaldoAtual = c.SaldoAtual,
                Principal = c.Principal,
                Ativo = c.Ativo,
                CriadoEm = c.CriadoEm,
                TemCartaoVinculado = _context.CartoesCredito
                    .Any(cc => cc.ContaId == c.Id && cc.Ativo)
            })
            .ToListAsync();

        return Resultado<List<ContaResponse>>.Ok(contas);
    }

    public async Task<Resultado<ContaResponse>> ObterPorIdAsync(int usuarioId, int contaId)
    {
        var conta = await _context.Contas
            .Where(c => c.Id == contaId && c.UsuarioId == usuarioId && c.Ativo)
            .Select(c => new ContaResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                TipoConta = c.TipoConta.ToString(),
                Banco = c.Banco,
                Cor = c.Cor,
                Icone = c.Icone,
                SaldoInicial = c.SaldoInicial,
                SaldoAtual = c.SaldoAtual,
                Principal = c.Principal,
                Ativo = c.Ativo,
                CriadoEm = c.CriadoEm,
                TemCartaoVinculado = _context.CartoesCredito
                    .Any(cc => cc.ContaId == c.Id && cc.Ativo)
            })
            .FirstOrDefaultAsync();

        if (conta == null)
            return Resultado<ContaResponse>.NaoEncontrado("Conta não encontrada.");

        return Resultado<ContaResponse>.Ok(conta);
    }

    public async Task<Resultado<ContaResponse>> CriarAsync(int usuarioId, CriarContaRequest request)
    {
        if (!Enum.TryParse<TipoConta>(request.TipoConta, out var tipoConta))
            return Resultado<ContaResponse>.Falha("Tipo de conta inválido. Use: CORRENTE, POUPANCA, CARTEIRA ou INVESTIMENTO.");

        if (request.Principal)
            await DesmarcarContaPrincipalAsync(usuarioId);

        var temContas = await _context.Contas
            .AnyAsync(c => c.UsuarioId == usuarioId && c.Ativo);

        var conta = new Conta
        {
            UsuarioId = usuarioId,
            Nome = request.Nome,
            TipoConta = tipoConta,
            Banco = request.Banco,
            Cor = request.Cor,
            Icone = request.Icone,
            SaldoInicial = request.SaldoInicial,
            SaldoAtual = request.SaldoInicial,
            Principal = request.Principal || !temContas
        };

        _context.Contas.Add(conta);
        await _context.SaveChangesAsync();

        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = usuarioId,
            Acao = "CONTA_CRIADA",
            TipoEntidade = "Conta",
            EntidadeId = conta.Id.ToString(),
            Detalhes = $"Conta '{conta.Nome}' criada com saldo inicial de R${conta.SaldoInicial:F2}"
        });
        await _context.SaveChangesAsync();

        return Resultado<ContaResponse>.Criado(MapearContaResponse(conta), "Conta criada com sucesso!");
    }

    public async Task<Resultado<ContaResponse>> AtualizarAsync(int usuarioId, int contaId, AtualizarContaRequest request)
    {
        var conta = await _context.Contas
            .FirstOrDefaultAsync(c => c.Id == contaId && c.UsuarioId == usuarioId && c.Ativo);

        if (conta == null)
            return Resultado<ContaResponse>.NaoEncontrado("Conta não encontrada.");

        if (!string.IsNullOrWhiteSpace(request.Nome))
            conta.Nome = request.Nome;

        if (request.Banco != null)
            conta.Banco = request.Banco;

        if (!string.IsNullOrWhiteSpace(request.Cor))
            conta.Cor = request.Cor;

        if (!string.IsNullOrWhiteSpace(request.Icone))
            conta.Icone = request.Icone;

        if (request.SaldoInicial.HasValue)
        {
            var diferenca = request.SaldoInicial.Value - conta.SaldoInicial;
            conta.SaldoInicial = request.SaldoInicial.Value;
            conta.SaldoAtual += diferenca;
        }

        if (request.Principal.HasValue && request.Principal.Value && !conta.Principal)
        {
            await DesmarcarContaPrincipalAsync(usuarioId);
            conta.Principal = true;
        }

        conta.AtualizadoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Resultado<ContaResponse>.Ok(MapearContaResponse(conta), "Conta atualizada!");
    }

    public async Task<Resultado<bool>> ExcluirAsync(int usuarioId, int contaId)
    {
        var conta = await _context.Contas
            .FirstOrDefaultAsync(c => c.Id == contaId && c.UsuarioId == usuarioId && c.Ativo);

        if (conta == null)
            return Resultado<bool>.NaoEncontrado("Conta não encontrada.");

        var temCartaoAtivo = await _context.CartoesCredito
            .AnyAsync(cc => cc.ContaId == contaId && cc.Ativo);
        if (temCartaoAtivo)
            return Resultado<bool>.Falha("Esta conta possui cartão de crédito ativo vinculado. Exclua o cartão antes de excluir a conta.");

        if (conta.Principal)
        {
            var outraConta = await _context.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Ativo && c.Id != contaId);
            if (outraConta != null)
                outraConta.Principal = true;
        }

        _context.Contas.Remove(conta);
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Conta excluída com sucesso!");
    }

    public async Task<Resultado<decimal>> RecalcularSaldoAsync(int usuarioId, int contaId)
    {
        var conta = await _context.Contas
            .FirstOrDefaultAsync(c => c.Id == contaId && c.UsuarioId == usuarioId);

        if (conta == null)
            return Resultado<decimal>.NaoEncontrado("Conta não encontrada.");

        var totalReceitas = await _context.Transacoes
            .Where(t => t.ContaId == contaId
                     && t.Tipo == TipoTransacao.RECEITA
                     && t.Status == StatusTransacao.EFETIVADA
                     && t.TransferenciaContaId == null)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        var totalDespesas = await _context.Transacoes
            .Where(t => t.ContaId == contaId
                     && t.Tipo == TipoTransacao.DESPESA
                     && t.Status == StatusTransacao.EFETIVADA
                     && t.CartaoCreditoId == null
                     && t.TransferenciaContaId == null)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        var entradasTransferencia = await _context.TransferenciasContas
            .Where(tc => tc.ContaDestinoId == contaId)
            .SumAsync(tc => (decimal?)tc.Valor) ?? 0;

        var saidasTransferencia = await _context.TransferenciasContas
            .Where(tc => tc.ContaOrigemId == contaId)
            .SumAsync(tc => (decimal?)tc.Valor) ?? 0;

        conta.SaldoAtual = conta.SaldoInicial + totalReceitas - totalDespesas + entradasTransferencia - saidasTransferencia;
        conta.AtualizadoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Resultado<decimal>.Ok(conta.SaldoAtual, "Saldo recalculado com sucesso!");
    }

    public async Task<Resultado<TransferenciaResponse>> TransferirAsync(int usuarioId, TransferenciaRequest request)
    {
        if (request.ContaOrigemId == request.ContaDestinoId)
            return Resultado<TransferenciaResponse>.Falha("Conta de origem e destino não podem ser a mesma.");

        var contaOrigem = await _context.Contas
            .FirstOrDefaultAsync(c => c.Id == request.ContaOrigemId && c.UsuarioId == usuarioId && c.Ativo);
        if (contaOrigem == null)
            return Resultado<TransferenciaResponse>.Falha("Conta de origem não encontrada.");

        var contaDestino = await _context.Contas
            .FirstOrDefaultAsync(c => c.Id == request.ContaDestinoId && c.UsuarioId == usuarioId && c.Ativo);
        if (contaDestino == null)
            return Resultado<TransferenciaResponse>.Falha("Conta de destino não encontrada.");

        if (contaOrigem.SaldoAtual < request.Valor)
            return Resultado<TransferenciaResponse>.Falha(
                $"Saldo insuficiente. Disponível: R${contaOrigem.SaldoAtual:F2}, Solicitado: R${request.Valor:F2}");

        contaOrigem.SaldoAtual -= request.Valor;
        contaOrigem.AtualizadoEm = DateTime.UtcNow;

        contaDestino.SaldoAtual += request.Valor;
        contaDestino.AtualizadoEm = DateTime.UtcNow;

        var transferencia = new TransferenciaConta
        {
            UsuarioId = usuarioId,
            ContaOrigemId = request.ContaOrigemId,
            ContaDestinoId = request.ContaDestinoId,
            Valor = request.Valor,
            Descricao = request.Descricao,
            DataTransferencia = request.Data ?? DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _context.TransferenciasContas.Add(transferencia);

        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = usuarioId,
            Acao = "TRANSFERENCIA",
            TipoEntidade = "TransferenciaConta",
            EntidadeId = transferencia.Id.ToString(),
            Detalhes = $"R${request.Valor:F2} de '{contaOrigem.Nome}' para '{contaDestino.Nome}'"
        });

        await _context.SaveChangesAsync();

        var response = new TransferenciaResponse
        {
            Id = transferencia.Id,
            ContaOrigem = contaOrigem.Nome,
            ContaDestino = contaDestino.Nome,
            Valor = request.Valor,
            Descricao = request.Descricao,
            DataTransferencia = transferencia.DataTransferencia
        };

        return Resultado<TransferenciaResponse>.Ok(response,
            $"Transferência de R${request.Valor:F2} realizada com sucesso!");
    }

    private async Task DesmarcarContaPrincipalAsync(int usuarioId)
    {
        var contaPrincipalAtual = await _context.Contas
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Principal && c.Ativo);

        if (contaPrincipalAtual != null)
        {
            contaPrincipalAtual.Principal = false;
            contaPrincipalAtual.AtualizadoEm = DateTime.UtcNow;
        }
    }

    private static ContaResponse MapearContaResponse(Conta conta)
    {
        return new ContaResponse
        {
            Id = conta.Id,
            Nome = conta.Nome,
            TipoConta = conta.TipoConta.ToString(),
            Banco = conta.Banco,
            Cor = conta.Cor,
            Icone = conta.Icone,
            SaldoInicial = conta.SaldoInicial,
            SaldoAtual = conta.SaldoAtual,
            Principal = conta.Principal,
            Ativo = conta.Ativo,
            CriadoEm = conta.CriadoEm
        };
    }
}
