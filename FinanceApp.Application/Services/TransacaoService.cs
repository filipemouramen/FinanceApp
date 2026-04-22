using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Application.DTOs.Transacoes;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services
{
    public class TransacaoService : ITransacaoService
    {
        private readonly FinanceDbContext _context; //context do EF para acessar o banco de dados

        public TransacaoService(FinanceDbContext context)
        {
            _context = context;
        }

        //primeio passo é validar os dados da entrada, como categoria, conta e cartão de crédito (se informado)
        public async Task<Resultado<TransacaoResponse>> CriarAsync(Guid usuarioId, CriarTransacaoRequest request)
        {
            //validando categoria da transação
            var categoria = await _context.Categorias
               .FirstOrDefaultAsync(c => c.Id == request.CategoriaId && c.Ativo);
            if (categoria == null)
            {
                return Resultado<TransacaoResponse>.Falha("Categoria não encontrada.");
            }

            //se informada, a conta será validada
            if (request.ContaId.HasValue)
            {
                var contaExiste = await _context.Contas
                    .AnyAsync(c => c.Id == request.ContaId && c.UsuarioId == usuarioId && c.Ativo);
                if (!contaExiste)
                {
                    return Resultado<TransacaoResponse>.Falha("Conta não encontrada.");
                }
            }

            //validação cartão de crédito se informado
            CartaoCredito? cartao = null;
            if (request.CartaoCreditoId.HasValue)
            {
                cartao = await _context.CartoesCredito
                    .FirstOrDefaultAsync(c => c.Id == request.CartaoCreditoId && c.UsuarioId == usuarioId && c.Ativo);
                if (cartao == null)
                    return Resultado<TransacaoResponse>.Falha("Cartão de crédito não encontrado.");
            }

            var tipo = Enum.Parse<TipoTransacao>(request.Tipo);
            var dataTransacao = request.DataTransacao ?? DateOnly.FromDateTime(DateTime.UtcNow);

            //se for um lançamento parcelado
            if (request.TotalParcelas.HasValue && request.TotalParcelas.Value >= 2)
            {
                return await CriarParcelamentoAsync(usuarioId, request, tipo, dataTransacao, cartao);
            }

            // ===== TRANSACAO SIMPLES =====
            var transacao = new Transacao
            {
                UsuarioId = usuarioId,
                CategoriaId = request.CategoriaId,
                ContaId = request.ContaId,
                FormaPagamentoId = request.FormaPagamentoId, //apenas informativo pra sair nos relatórios
                CartaoCreditoId = request.CartaoCreditoId,
                Valor = request.Valor,
                Tipo = tipo,
                Descricao = request.Descricao,
                DataTransacao = dataTransacao,
                Origem = OrigemTransacao.APP,
                Observacoes = request.Observacoes,
                // Se agendou OU data é futura → PENDENTE, senão EFETIVADA
                Status = (request.Agendar || dataTransacao > DateOnly.FromDateTime(DateTime.UtcNow))
                    ? StatusTransacao.PENDENTE
                    : StatusTransacao.EFETIVADA
            };

            //vinculando o parcelamento na fatura
            if (cartao != null)
            {
                var fatura = await ObterOuCriarFaturaAsync(cartao, usuarioId, dataTransacao);
                transacao.FaturaCartaoId = fatura.Id;
                fatura.ValorTotal += transacao.Valor;
                fatura.AtualizadoEm = DateTime.UtcNow;

                cartao.LimiteDisponivel -= request.Valor;
                cartao.AtualizadoEm = DateTime.UtcNow;
            }

            _context.Transacoes.Add(transacao);

            // Só debita da conta se a transação está EFETIVADA
            if (request.ContaId.HasValue && cartao == null && transacao.Status == StatusTransacao.EFETIVADA)
            {
                await AtualizarSaldoContaAsync(request.ContaId.Value, request.Valor, tipo);
            }

            await _context.SaveChangesAsync();

            var response = await ObterTransacaoResponseAsync(transacao.Id);
            return Resultado<TransacaoResponse>.Criado(response!);
        }

        //atualizar transação, não permite editar parcelas individualmente, apenas o lançamento avulso
        public async Task<Resultado<TransacaoResponse>> AtualizarAsync(Guid usuarioId, Guid transacaoId, AtualizarTransacaoRequest request)
        {
            var transacao = await _context.Transacoes
                .FirstOrDefaultAsync(t => t.Id == transacaoId && t.UsuarioId == usuarioId);

            if (transacao == null)
                return Resultado<TransacaoResponse>.NaoEncontrado("Transação não encontrada.");

            // Não permite editar transações de parcelamento individualmente
            if (transacao.ParcelamentoId.HasValue)
                return Resultado<TransacaoResponse>.Falha("Não é possível editar parcelas individualmente. Exclua o parcelamento.");

            var valorAntigo = transacao.Valor;
            var tipoAntigo = transacao.Tipo;
            var contaAntigaId = transacao.ContaId;

            //validando categoria da transação
            if (request.CategoriaId.HasValue)
            {
                var categoriaExiste = await _context.Categorias
                    .AnyAsync(c => c.Id == request.CategoriaId && c.Ativo);
                if (!categoriaExiste)
                    return Resultado<TransacaoResponse>.Falha("Categoria não encontrada.");
                transacao.CategoriaId = request.CategoriaId.Value;
            }

            //validando contas
            if (request.ContaId.HasValue)
            {
                var contaExiste = await _context.Contas
                    .AnyAsync(c => c.Id == request.ContaId && c.UsuarioId == usuarioId && c.Ativo);
                if (!contaExiste)
                    return Resultado<TransacaoResponse>.Falha("Conta não encontrada.");
                transacao.ContaId = request.ContaId;
            }

            if (request.FormaPagamentoId.HasValue)
                transacao.FormaPagamentoId = request.FormaPagamentoId;

            if (request.Valor.HasValue)
                transacao.Valor = request.Valor.Value;

            if (request.Descricao != null)
                transacao.Descricao = request.Descricao;

            if (request.DataTransacao.HasValue)
                transacao.DataTransacao = request.DataTransacao.Value;

            if (request.Observacoes != null)
                transacao.Observacoes = request.Observacoes;

            // Lógica de mudança de status (PENDENTE → EFETIVADA debita, EFETIVADA → CANCELADA reverte)
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<StatusTransacao>(request.Status, out var novoStatus))
            {
                var statusAntigo = transacao.Status;
                transacao.Status = novoStatus;

                // Se estava PENDENTE e agora está EFETIVADA → debitar da conta
                if (statusAntigo == StatusTransacao.PENDENTE && novoStatus == StatusTransacao.EFETIVADA)
                {
                    if (transacao.ContaId.HasValue && transacao.CartaoCreditoId == null)
                    {
                        await AtualizarSaldoContaAsync(transacao.ContaId.Value, transacao.Valor, transacao.Tipo);
                    }
                }

                // Se estava EFETIVADA e agora está CANCELADA → reverter saldo
                if (statusAntigo == StatusTransacao.EFETIVADA && novoStatus == StatusTransacao.CANCELADA)
                {
                    if (transacao.ContaId.HasValue && transacao.CartaoCreditoId == null)
                    {
                        await ReverterSaldoContaAsync(transacao.ContaId.Value, transacao.Valor, transacao.Tipo);
                    }
                }
            }

            transacao.AtualizadoEm = DateTime.UtcNow;

            // Recalcular saldo se valor ou conta mudou (apenas se não houve mudança de status, pra não debitar duas vezes)
            if (string.IsNullOrEmpty(request.Status) && transacao.CartaoCreditoId == null && transacao.Status == StatusTransacao.EFETIVADA)
            {
                if (contaAntigaId.HasValue)
                    await ReverterSaldoContaAsync(contaAntigaId.Value, valorAntigo, tipoAntigo);

                if (transacao.ContaId.HasValue)
                    await AtualizarSaldoContaAsync(transacao.ContaId.Value, transacao.Valor, transacao.Tipo);
            }

            //valor mudou e é cartão, atualizar fatura e limite
            if (request.Valor.HasValue && transacao.CartaoCreditoId.HasValue)
            {
                var diferenca = request.Valor.Value - valorAntigo;

                if (transacao.FaturaCartaoId.HasValue)
                {
                    var fatura = await _context.FaturasCartao.FindAsync(transacao.FaturaCartaoId);
                    if (fatura != null)
                    {
                        fatura.ValorTotal += diferenca;
                        fatura.AtualizadoEm = DateTime.UtcNow;
                    }
                }

                var cartao = await _context.CartoesCredito.FindAsync(transacao.CartaoCreditoId);
                if (cartao != null)
                {
                    cartao.LimiteDisponivel -= diferenca;
                    cartao.AtualizadoEm = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            var response = await ObterTransacaoResponseAsync(transacao.Id);
            return Resultado<TransacaoResponse>.Ok(response!, "Transação atualizada!");
        }

        //exclusão de transações
        public async Task<Resultado<bool>> ExcluirAsync(Guid usuarioId, Guid transacaoId)
        {
            var transacao = await _context.Transacoes
                .FirstOrDefaultAsync(t => t.Id == transacaoId && t.UsuarioId == usuarioId);

            if (transacao == null)
                return Resultado<bool>.NaoEncontrado("Transação não encontrada.");

            //faz parte de parcelamento, excluir todas as parcelas
            if (transacao.ParcelamentoId.HasValue)
            {
                return await ExcluirParcelamentoAsync(usuarioId, transacao.ParcelamentoId.Value);
            }

            //reverter saldo da conta (apenas se estava EFETIVADA)
            if (transacao.ContaId.HasValue && transacao.CartaoCreditoId == null && transacao.Status == StatusTransacao.EFETIVADA)
            {
                await ReverterSaldoContaAsync(transacao.ContaId.Value, transacao.Valor, transacao.Tipo);
            }

            //reverter fatura e limite do cartão
            if (transacao.CartaoCreditoId.HasValue)
            {
                if (transacao.FaturaCartaoId.HasValue)
                {
                    var fatura = await _context.FaturasCartao.FindAsync(transacao.FaturaCartaoId);
                    if (fatura != null)
                    {
                        fatura.ValorTotal -= transacao.Valor;
                        fatura.AtualizadoEm = DateTime.UtcNow;
                    }
                }

                var cartao = await _context.CartoesCredito.FindAsync(transacao.CartaoCreditoId);
                if (cartao != null)
                {
                    cartao.LimiteDisponivel += transacao.Valor;
                    cartao.AtualizadoEm = DateTime.UtcNow;
                }
            }

            _context.Transacoes.Remove(transacao);
            await _context.SaveChangesAsync();

            return Resultado<bool>.Ok(true, "Transação excluída!");
        }

        //obter transação por id
        public async Task<Resultado<TransacaoResponse>> ObterPorIdAsync(Guid usuarioId, Guid transacaoId)
        {
            var response = await ObterTransacaoResponseAsync(transacaoId, usuarioId);
            if (response == null)
                return Resultado<TransacaoResponse>.NaoEncontrado("Transação não encontrada.");

            return Resultado<TransacaoResponse>.Ok(response);
        }

        //listando transações com filtros e paginação
        public async Task<Resultado<ListaPaginada<TransacaoResponse>>> ListarAsync(Guid usuarioId, FiltroTransacaoRequest filtro)
        {
            var query = _context.Transacoes
                .Include(t => t.Categoria)
                .Include(t => t.Conta)
                .Include(t => t.FormaPagamento)
                .Include(t => t.CartaoCredito)
                .Where(t => t.UsuarioId == usuarioId)
                .AsQueryable();

            //aplicar filtros
            if (filtro.DataInicio.HasValue)
                query = query.Where(t => t.DataTransacao >= filtro.DataInicio.Value);

            if (filtro.DataFim.HasValue)
                query = query.Where(t => t.DataTransacao <= filtro.DataFim.Value);

            if (!string.IsNullOrEmpty(filtro.Tipo) && Enum.TryParse<TipoTransacao>(filtro.Tipo, out var tipo))
                query = query.Where(t => t.Tipo == tipo);

            if (filtro.CategoriaId.HasValue)
                query = query.Where(t => t.CategoriaId == filtro.CategoriaId.Value);

            if (filtro.ContaId.HasValue)
                query = query.Where(t => t.ContaId == filtro.ContaId.Value);

            if (filtro.CartaoCreditoId.HasValue)
                query = query.Where(t => t.CartaoCreditoId == filtro.CartaoCreditoId.Value);

            if (!string.IsNullOrEmpty(filtro.Origem) && Enum.TryParse<OrigemTransacao>(filtro.Origem, out var origem))
                query = query.Where(t => t.Origem == origem);

            if (!string.IsNullOrEmpty(filtro.Busca))
                query = query.Where(t => t.Descricao != null && t.Descricao.Contains(filtro.Busca));

            // Filtro por status
            if (!string.IsNullOrEmpty(filtro.Status) && Enum.TryParse<StatusTransacao>(filtro.Status, out var statusFiltro))
                query = query.Where(t => t.Status == statusFiltro);

            // Total antes de paginar
            var total = await query.CountAsync();

            // Ordenar e paginar
            var itens = await query
                .OrderByDescending(t => t.DataTransacao)
                .ThenByDescending(t => t.CriadoEm)
                .Skip((filtro.Pagina - 1) * filtro.ItensPorPagina)
                .Take(filtro.ItensPorPagina)
                .Select(t => new TransacaoResponse
                {
                    Id = t.Id,
                    CategoriaId = t.CategoriaId,
                    NomeCategoria = t.Categoria.Nome,
                    IconeCategoria = t.Categoria.Icone,
                    CorCategoria = t.Categoria.Cor,
                    ContaId = t.ContaId,
                    NomeConta = t.Conta != null ? t.Conta.Nome : null,
                    FormaPagamentoId = t.FormaPagamentoId,
                    NomeFormaPagamento = t.FormaPagamento != null ? t.FormaPagamento.Nome : null,
                    CartaoCreditoId = t.CartaoCreditoId,
                    NomeCartaoCredito = t.CartaoCredito != null ? t.CartaoCredito.Nome : null,
                    Valor = t.Valor,
                    Tipo = t.Tipo.ToString(),
                    Descricao = t.Descricao,
                    DataTransacao = t.DataTransacao,
                    Origem = t.Origem.ToString(),
                    Status = t.Status.ToString(),
                    Atrasada = t.Status == StatusTransacao.PENDENTE && t.DataTransacao < DateOnly.FromDateTime(DateTime.UtcNow),
                    Observacoes = t.Observacoes,
                    Recorrente = t.Recorrente,
                    NumeroParcela = t.NumeroParcela,
                    TotalParcelas = t.TotalParcelas,
                    CriadoEm = t.CriadoEm
                })
                .ToListAsync();

            var resultado = new ListaPaginada<TransacaoResponse>
            {
                Itens = itens,
                TotalItens = total,
                Pagina = filtro.Pagina,
                ItensPorPagina = filtro.ItensPorPagina
            };

            return Resultado<ListaPaginada<TransacaoResponse>>.Ok(resultado);
        }

        private async Task<Resultado<TransacaoResponse>> CriarParcelamentoAsync(
            Guid usuarioId,
            CriarTransacaoRequest request,
            TipoTransacao tipo,
            DateOnly dataTransacao,
            CartaoCredito? cartao)
        {
            var totalParcelas = request.TotalParcelas!.Value;
            var valorParcela = Math.Round(request.Valor / totalParcelas, 2);

            var valorUltimaParcela = request.Valor - (valorParcela * (totalParcelas - 1));

            var parcelamento = new Parcelamento
            {
                UsuarioId = usuarioId,
                CartaoCreditoId = request.CartaoCreditoId,
                CategoriaId = request.CategoriaId,
                Descricao = request.Descricao ?? "Parcelamento",
                ValorTotal = request.Valor,
                ValorParcela = valorParcela,
                TotalParcelas = totalParcelas,
                DataPrimeiraParcela = dataTransacao
            };
            _context.Parcelamentos.Add(parcelamento);
            await _context.SaveChangesAsync();

            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

            // Criar cada parcela como transação
            for (int i = 1; i <= totalParcelas; i++)
            {
                var dataParcela = dataTransacao.AddMonths(i - 1);
                var valorEsta = (i == totalParcelas) ? valorUltimaParcela : valorParcela;

                var transacao = new Transacao
                {
                    UsuarioId = usuarioId,
                    CategoriaId = request.CategoriaId,
                    ContaId = request.ContaId,
                    FormaPagamentoId = request.FormaPagamentoId,
                    CartaoCreditoId = request.CartaoCreditoId,
                    Valor = valorEsta,
                    Tipo = tipo,
                    Descricao = $"{request.Descricao} ({i}/{totalParcelas})",
                    DataTransacao = dataParcela,
                    Origem = OrigemTransacao.APP,
                    Observacoes = request.Observacoes,
                    ParcelamentoId = parcelamento.Id,
                    NumeroParcela = i,
                    TotalParcelas = totalParcelas,
                    // Primeira parcela pode ser efetivada, futuras ficam pendentes
                    Status = (dataParcela <= hoje && cartao == null)
                        ? StatusTransacao.EFETIVADA
                        : StatusTransacao.PENDENTE
                };

                // Vincular cada parcela à fatura correta
                if (cartao != null)
                {
                    var fatura = await ObterOuCriarFaturaAsync(cartao, usuarioId, dataParcela);
                    transacao.FaturaCartaoId = fatura.Id;
                    fatura.ValorTotal += valorEsta;
                    fatura.AtualizadoEm = DateTime.UtcNow;
                }

                _context.Transacoes.Add(transacao);
            }

            // Reduzir limite do cartão pelo valor total
            if (cartao != null)
            {
                cartao.LimiteDisponivel -= request.Valor;
                cartao.AtualizadoEm = DateTime.UtcNow;
            }

            if (request.ContaId.HasValue && cartao == null)
            {
                await AtualizarSaldoContaAsync(request.ContaId.Value, valorParcela, tipo);
            }

            await _context.SaveChangesAsync();

            // Retornar a primeira parcela
            var primeiraParcela = await _context.Transacoes
                .Include(t => t.Categoria)
                .Where(t => t.ParcelamentoId == parcelamento.Id && t.NumeroParcela == 1)
                .Select(t => new TransacaoResponse
                {
                    Id = t.Id,
                    CategoriaId = t.CategoriaId,
                    NomeCategoria = t.Categoria.Nome,
                    IconeCategoria = t.Categoria.Icone,
                    CorCategoria = t.Categoria.Cor,
                    Valor = t.Valor,
                    Tipo = t.Tipo.ToString(),
                    Descricao = t.Descricao,
                    DataTransacao = t.DataTransacao,
                    Origem = t.Origem.ToString(),
                    Status = t.Status.ToString(),
                    Atrasada = t.Status == StatusTransacao.PENDENTE && t.DataTransacao < DateOnly.FromDateTime(DateTime.UtcNow),
                    NumeroParcela = t.NumeroParcela,
                    TotalParcelas = t.TotalParcelas,
                    CriadoEm = t.CriadoEm
                })
                .FirstOrDefaultAsync();

            return Resultado<TransacaoResponse>.Criado(primeiraParcela!,
                $"Parcelamento criado: {totalParcelas}x de R${valorParcela:F2}");
        }

        private async Task<Resultado<bool>> ExcluirParcelamentoAsync(Guid usuarioId, Guid parcelamentoId)
        {
            var parcelas = await _context.Transacoes
                .Where(t => t.ParcelamentoId == parcelamentoId && t.UsuarioId == usuarioId)
                .ToListAsync();

            // Reverter faturas e limite do cartão
            CartaoCredito? cartao = null;
            foreach (var parcela in parcelas)
            {
                if (parcela.FaturaCartaoId.HasValue)
                {
                    var fatura = await _context.FaturasCartao.FindAsync(parcela.FaturaCartaoId);
                    if (fatura != null)
                    {
                        fatura.ValorTotal -= parcela.Valor;
                        fatura.AtualizadoEm = DateTime.UtcNow;
                    }
                }

                if (parcela.CartaoCreditoId.HasValue && cartao == null)
                {
                    cartao = await _context.CartoesCredito.FindAsync(parcela.CartaoCreditoId);
                }
            }

            // Devolver limite total ao cartão
            if (cartao != null)
            {
                var valorTotal = parcelas.Sum(p => p.Valor);
                cartao.LimiteDisponivel += valorTotal;
                cartao.AtualizadoEm = DateTime.UtcNow;
            }

            foreach (var parcela in parcelas.Where(p => p.Status == StatusTransacao.EFETIVADA && p.ContaId.HasValue && p.CartaoCreditoId == null))
            {
                await ReverterSaldoContaAsync(parcela.ContaId!.Value, parcela.Valor, parcela.Tipo);
            }


            _context.Transacoes.RemoveRange(parcelas);

            var parcelamento = await _context.Parcelamentos.FindAsync(parcelamentoId);
            if (parcelamento != null)
                _context.Parcelamentos.Remove(parcelamento);

            await _context.SaveChangesAsync();

            return Resultado<bool>.Ok(true, "Parcelamento e todas as parcelas foram excluídos!");
        }

        private async Task<FaturaCartao> ObterOuCriarFaturaAsync(CartaoCredito cartao, Guid usuarioId, DateOnly dataTransacao)
        {
            // Lógica: se a compra é DEPOIS do dia de fechamento, cai na fatura do próximo mês
            int mesFatura, anoFatura;

            if (dataTransacao.Day > cartao.DiaFechamento)
            {
                var proximoMes = dataTransacao.AddMonths(1);
                mesFatura = proximoMes.Month;
                anoFatura = proximoMes.Year;
            }
            else
            {
                mesFatura = dataTransacao.Month;
                anoFatura = dataTransacao.Year;
            }

            // Buscar fatura existente
            var fatura = await _context.FaturasCartao
                .FirstOrDefaultAsync(f => f.CartaoCreditoId == cartao.Id
                                       && f.MesReferencia == mesFatura
                                       && f.AnoReferencia == anoFatura);

            if (fatura != null)
                return fatura;

            // Criar fatura nova
            var diasNoMes = DateTime.DaysInMonth(anoFatura, mesFatura);
            var diaFech = Math.Min(cartao.DiaFechamento, diasNoMes);
            var diaVenc = Math.Min(cartao.DiaVencimento, diasNoMes);

            fatura = new FaturaCartao
            {
                CartaoCreditoId = cartao.Id,
                UsuarioId = usuarioId,
                MesReferencia = mesFatura,
                AnoReferencia = anoFatura,
                DataFechamento = new DateOnly(anoFatura, mesFatura, diaFech),
                DataVencimento = new DateOnly(anoFatura, mesFatura, diaVenc),
                Status = StatusFatura.ABERTA
            };

            _context.FaturasCartao.Add(fatura);
            await _context.SaveChangesAsync();

            return fatura;
        }

        private async Task AtualizarSaldoContaAsync(Guid contaId, decimal valor, TipoTransacao tipo)
        {
            var conta = await _context.Contas.FindAsync(contaId);
            if (conta == null) return;

            conta.SaldoAtual += tipo == TipoTransacao.RECEITA ? valor : -valor;
            conta.AtualizadoEm = DateTime.UtcNow;
        }

        private async Task ReverterSaldoContaAsync(Guid contaId, decimal valor, TipoTransacao tipo)
        {
            var conta = await _context.Contas.FindAsync(contaId);
            if (conta == null) return;

            conta.SaldoAtual += tipo == TipoTransacao.RECEITA ? -valor : valor;
            conta.AtualizadoEm = DateTime.UtcNow;
        }

        private async Task<TransacaoResponse?> ObterTransacaoResponseAsync(Guid transacaoId, Guid? usuarioId = null)
        {
            var query = _context.Transacoes
                .Include(t => t.Categoria)
                .Include(t => t.Conta)
                .Include(t => t.FormaPagamento)
                .Include(t => t.CartaoCredito)
                .Where(t => t.Id == transacaoId);

            if (usuarioId.HasValue)
                query = query.Where(t => t.UsuarioId == usuarioId.Value);

            return await query
                .Select(t => new TransacaoResponse
                {
                    Id = t.Id,
                    CategoriaId = t.CategoriaId,
                    NomeCategoria = t.Categoria.Nome,
                    IconeCategoria = t.Categoria.Icone,
                    CorCategoria = t.Categoria.Cor,
                    ContaId = t.ContaId,
                    NomeConta = t.Conta != null ? t.Conta.Nome : null,
                    FormaPagamentoId = t.FormaPagamentoId,
                    NomeFormaPagamento = t.FormaPagamento != null ? t.FormaPagamento.Nome : null,
                    CartaoCreditoId = t.CartaoCreditoId,
                    NomeCartaoCredito = t.CartaoCredito != null ? t.CartaoCredito.Nome : null,
                    Valor = t.Valor,
                    Tipo = t.Tipo.ToString(),
                    Descricao = t.Descricao,
                    DataTransacao = t.DataTransacao,
                    Origem = t.Origem.ToString(),
                    Status = t.Status.ToString(),
                    Atrasada = t.Status == StatusTransacao.PENDENTE && t.DataTransacao < DateOnly.FromDateTime(DateTime.UtcNow),
                    Observacoes = t.Observacoes,
                    Recorrente = t.Recorrente,
                    NumeroParcela = t.NumeroParcela,
                    TotalParcelas = t.TotalParcelas,
                    CriadoEm = t.CriadoEm
                })
                .FirstOrDefaultAsync();
        }
    }
}