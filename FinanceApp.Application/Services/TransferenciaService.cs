using FinanceApp.Application.DTOs.Transacoes;
using FinanceApp.Application.DTOs.Transferencias;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;

public class TransferenciaService : ITransferenciaService
{
    private readonly FinanceDbContext _context;

    // IDs de categorias de sistema usadas para transações de transferência
    // (excluídas dos totais pelo filtro TransferenciaContaId == null)
    private const int CategoriaTransferenciaDespesa = 15; // Outros
    private const int CategoriaTransferenciaReceita = 19; // Outros Ganhos

    public TransferenciaService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Resultado<TransferenciaResponseDTO>> CriarAsync(int usuarioId, CriarTransferenciaDTO dto)
    {
        if (dto.ContaOrigemId == dto.ContaDestinoId)
            return Resultado<TransferenciaResponseDTO>.Falha("Conta de origem e destino não podem ser a mesma.");

        if (dto.Valor <= 0)
            return Resultado<TransferenciaResponseDTO>.Falha("O valor da transferência deve ser maior que zero.");

        var contaOrigem = await _context.Contas
            .FirstOrDefaultAsync(c => c.Id == dto.ContaOrigemId && c.UsuarioId == usuarioId);
        if (contaOrigem == null)
            return Resultado<TransferenciaResponseDTO>.NaoEncontrado("Conta de origem não encontrada.");

        var contaDestino = await _context.Contas
            .FirstOrDefaultAsync(c => c.Id == dto.ContaDestinoId && c.UsuarioId == usuarioId);
        if (contaDestino == null)
            return Resultado<TransferenciaResponseDTO>.NaoEncontrado("Conta de destino não encontrada.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var transferencia = new TransferenciaConta
            {
                UsuarioId = usuarioId,
                ContaOrigemId = dto.ContaOrigemId,
                ContaDestinoId = dto.ContaDestinoId,
                Valor = dto.Valor,
                Descricao = dto.Descricao,
                DataTransferencia = dto.DataTransferencia
            };
            _context.TransferenciasContas.Add(transferencia);
            await _context.SaveChangesAsync();

            var transacaoSaida = new Transacao
            {
                UsuarioId = usuarioId,
                ContaId = dto.ContaOrigemId,
                CategoriaId = CategoriaTransferenciaDespesa,
                Tipo = TipoTransacao.DESPESA,
                Valor = dto.Valor,
                Status = StatusTransacao.EFETIVADA,
                Descricao = dto.Descricao ?? $"Transferência para {contaDestino.Nome}",
                DataTransacao = dto.DataTransferencia,
                TransferenciaContaId = transferencia.Id,
                Origem = OrigemTransacao.APP
            };

            var transacaoEntrada = new Transacao
            {
                UsuarioId = usuarioId,
                ContaId = dto.ContaDestinoId,
                CategoriaId = CategoriaTransferenciaReceita,
                Tipo = TipoTransacao.RECEITA,
                Valor = dto.Valor,
                Status = StatusTransacao.EFETIVADA,
                Descricao = dto.Descricao ?? $"Transferência de {contaOrigem.Nome}",
                DataTransacao = dto.DataTransferencia,
                TransferenciaContaId = transferencia.Id,
                Origem = OrigemTransacao.APP
            };

            _context.Transacoes.Add(transacaoSaida);
            _context.Transacoes.Add(transacaoEntrada);

            contaOrigem.SaldoAtual -= dto.Valor;
            contaOrigem.AtualizadoEm = DateTime.UtcNow;

            contaDestino.SaldoAtual += dto.Valor;
            contaDestino.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Resultado<TransferenciaResponseDTO>.Criado(new TransferenciaResponseDTO
            {
                Id = transferencia.Id,
                ContaOrigem = new ContaTransferenciaDTO
                {
                    Id = contaOrigem.Id,
                    Nome = contaOrigem.Nome,
                    Cor = contaOrigem.Cor,
                    NovoSaldo = contaOrigem.SaldoAtual
                },
                ContaDestino = new ContaTransferenciaDTO
                {
                    Id = contaDestino.Id,
                    Nome = contaDestino.Nome,
                    Cor = contaDestino.Cor,
                    NovoSaldo = contaDestino.SaldoAtual
                },
                Valor = dto.Valor,
                DataTransferencia = dto.DataTransferencia,
                Descricao = dto.Descricao,
                TransacaoOrigemId = transacaoSaida.Id,
                TransacaoDestinoId = transacaoEntrada.Id,
                CriadoEm = transferencia.CriadoEm
            }, "Transferência realizada com sucesso!");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Resultado<TransferenciaResponseDTO>.Falha("Erro ao realizar a transferência. Tente novamente.");
        }
    }

    public async Task<Resultado<ListaPaginada<TransferenciaResponseDTO>>> ListarAsync(int usuarioId, FiltroTransferenciaDTO filtro)
    {
        var query = _context.TransferenciasContas
            .Include(tc => tc.ContaOrigem)
            .Include(tc => tc.ContaDestino)
            .Where(tc => tc.UsuarioId == usuarioId);

        if (filtro.Mes.HasValue)
            query = query.Where(tc => tc.DataTransferencia.Month == filtro.Mes.Value);

        if (filtro.Ano.HasValue)
            query = query.Where(tc => tc.DataTransferencia.Year == filtro.Ano.Value);

        var totalItens = await query.CountAsync();

        var itens = await query
            .OrderByDescending(tc => tc.DataTransferencia)
            .ThenByDescending(tc => tc.CriadoEm)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(tc => new TransferenciaResponseDTO
            {
                Id = tc.Id,
                ContaOrigem = new ContaTransferenciaDTO
                {
                    Id = tc.ContaOrigem.Id,
                    Nome = tc.ContaOrigem.Nome,
                    Cor = tc.ContaOrigem.Cor
                },
                ContaDestino = new ContaTransferenciaDTO
                {
                    Id = tc.ContaDestino.Id,
                    Nome = tc.ContaDestino.Nome,
                    Cor = tc.ContaDestino.Cor
                },
                Valor = tc.Valor,
                DataTransferencia = tc.DataTransferencia,
                Descricao = tc.Descricao,
                CriadoEm = tc.CriadoEm
            })
            .ToListAsync();

        return Resultado<ListaPaginada<TransferenciaResponseDTO>>.Ok(new ListaPaginada<TransferenciaResponseDTO>
        {
            Itens = itens,
            TotalItens = totalItens,
            Pagina = filtro.Pagina,
            ItensPorPagina = filtro.TamanhoPagina
        });
    }

    public async Task<Resultado<CancelamentoTransferenciaResponse>> CancelarAsync(int id, int usuarioId)
    {
        var transferencia = await _context.TransferenciasContas
            .Include(tc => tc.ContaOrigem)
            .Include(tc => tc.ContaDestino)
            .Include(tc => tc.Transacoes)
            .FirstOrDefaultAsync(tc => tc.Id == id && tc.UsuarioId == usuarioId);

        if (transferencia == null)
            return Resultado<CancelamentoTransferenciaResponse>.NaoEncontrado("Transferência não encontrada.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            transferencia.ContaOrigem.SaldoAtual += transferencia.Valor;
            transferencia.ContaOrigem.AtualizadoEm = DateTime.UtcNow;

            transferencia.ContaDestino.SaldoAtual -= transferencia.Valor;
            transferencia.ContaDestino.AtualizadoEm = DateTime.UtcNow;

            _context.Transacoes.RemoveRange(transferencia.Transacoes);
            _context.TransferenciasContas.Remove(transferencia);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Resultado<CancelamentoTransferenciaResponse>.Ok(new CancelamentoTransferenciaResponse
            {
                Mensagem = "Transferência cancelada. Saldos revertidos.",
                ContaOrigem = new ContaTransferenciaDTO
                {
                    Id = transferencia.ContaOrigem.Id,
                    Nome = transferencia.ContaOrigem.Nome,
                    SaldoRestaurado = transferencia.ContaOrigem.SaldoAtual
                },
                ContaDestino = new ContaTransferenciaDTO
                {
                    Id = transferencia.ContaDestino.Id,
                    Nome = transferencia.ContaDestino.Nome,
                    SaldoRestaurado = transferencia.ContaDestino.SaldoAtual
                }
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Resultado<CancelamentoTransferenciaResponse>.Falha("Erro ao cancelar a transferência.");
        }
    }
}
