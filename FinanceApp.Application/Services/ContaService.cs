using FinanceApp.Application.DTOs.Contas;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace FinanceApp.Application.Services
{
    public class ContaService : IContaService
    {
        private readonly FinanceDbContext _context;

        public ContaService(FinanceDbContext context)
        {
            _context = context;
        }

        //listando contas
        public async Task<Resultado<List<ContaResponse>>> ListarAsync(Guid usuarioId)
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
                    CriadoEm = c.CriadoEm
                })
                .ToListAsync();

            return Resultado<List<ContaResponse>>.Ok(contas);
        }

        public async Task<Resultado<ContaResponse>> ObterPorIdAsync(Guid usuarioId, Guid contaId)
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
                    CriadoEm = c.CriadoEm
                })
                .FirstOrDefaultAsync();

            if (conta == null)
                return Resultado<ContaResponse>.NaoEncontrado("Conta não encontrada.");

            return Resultado<ContaResponse>.Ok(conta);
        }

        //criando conta
        public async Task<Resultado<ContaResponse>> CriarAsync(Guid usuarioId, CriarContaRequest request)
        {
            if (!Enum.TryParse<TipoConta>(request.TipoConta, out var tipoConta))
                return Resultado<ContaResponse>.Falha("Tipo de conta inválido. Use: CORRENTE, POUPANCA, CARTEIRA ou INVESTIMENTO.");

            if (request.Principal)
            {
                await DesmarcarContaPrincipalAsync(usuarioId);
            }

            //primeira conta do usuário, marcar como principal automaticamente
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
                Principal = request.Principal || !temContas // primeira conta é sempre a principal
            };

            _context.Contas.Add(conta);
            await _context.SaveChangesAsync();

            // Log de auditoria
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

        public async Task<Resultado<ContaResponse>> AtualizarAsync(Guid usuarioId, Guid contaId, AtualizarContaRequest request)
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

            // se marcou como principal, desmarcar a anterior
            if (request.Principal.HasValue && request.Principal.Value && !conta.Principal)
            {
                await DesmarcarContaPrincipalAsync(usuarioId);
                conta.Principal = true;
            }

            conta.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Resultado<ContaResponse>.Ok(MapearContaResponse(conta), "Conta atualizada!");
        }

        public async Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid contaId)
        {
            var conta = await _context.Contas
                .FirstOrDefaultAsync(c => c.Id == contaId && c.UsuarioId == usuarioId && c.Ativo);

            if (conta == null)
                return Resultado<bool>.NaoEncontrado("Conta não encontrada.");

            var temTransacoes = await _context.Transacoes
                .AnyAsync(t => t.ContaId == contaId);

            if (temTransacoes)
            {
                //não apaga, apenas desativa
                conta.Ativo = false;
                conta.AtualizadoEm = DateTime.UtcNow;

                // se era principal, promove outra conta pra ser a principal
                if (conta.Principal)
                {
                    conta.Principal = false;
                    var outraConta = await _context.Contas
                        .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Ativo && c.Id != contaId);
                    if (outraConta != null)
                        outraConta.Principal = true;
                }

                await _context.SaveChangesAsync();
                return Resultado<bool>.Ok(true, "Conta desativada. Não foi excluída porque possui transações vinculadas.");
            }

            //não tem transações, exclui de verdade
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

        //transf entre contas
        public async Task<Resultado<TransferenciaResponse>> TransferirAsync(Guid usuarioId, TransferenciaRequest request)
        {
            //valida que não é a mesma conta
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

            // Log de auditoria
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

        private async Task DesmarcarContaPrincipalAsync(Guid usuarioId)
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
}