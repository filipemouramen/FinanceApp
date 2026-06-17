using FinanceApp.Application.DTOs.Transacoes;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;

public class TransacaoService : ITransacaoService
{
    private readonly FinanceDbContext _context;
    private readonly INotificacaoService _notificacaoService;
    private readonly IAuditService _auditService;

    public TransacaoService(FinanceDbContext context, INotificacaoService notificacaoService, IAuditService auditService)
    {
        _context = context;
        _notificacaoService = notificacaoService;
        _auditService = auditService;
    }

    public async Task<Resultado<TransacaoResponse>> CriarAsync(int usuarioId, CriarTransacaoRequest request)
    {
        var categoria = await _context.Categorias
           .FirstOrDefaultAsync(c => c.Id == request.CategoriaId && c.Ativo);
        if (categoria == null)
            return Resultado<TransacaoResponse>.Falha("Categoria não encontrada.");

        if (request.ContaId.HasValue)
        {
            var contaExiste = await _context.Contas
                .AnyAsync(c => c.Id == request.ContaId && c.UsuarioId == usuarioId && c.Ativo);
            if (!contaExiste)
                return Resultado<TransacaoResponse>.Falha("Conta não encontrada.");
        }

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

        if (request.TotalParcelas.HasValue && request.TotalParcelas.Value >= 2)
            return await CriarParcelamentoAsync(usuarioId, request, tipo, dataTransacao, cartao);

        var transacao = new Transacao
        {
            UsuarioId = usuarioId,
            CategoriaId = request.CategoriaId,
            ContaId = request.ContaId,
            FormaPagamentoId = request.FormaPagamentoId,
            CartaoCreditoId = request.CartaoCreditoId,
            Valor = request.Valor,
            Tipo = tipo,
            Descricao = request.Descricao,
            DataTransacao = dataTransacao,
            Origem = OrigemTransacao.APP,
            Observacoes = request.Observacoes,
            Status = (request.Agendar || dataTransacao > DateOnly.FromDateTime(DateTime.UtcNow))
                ? StatusTransacao.PENDENTE
                : StatusTransacao.EFETIVADA
        };

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

        if (request.ContaId.HasValue && cartao == null && transacao.Status == StatusTransacao.EFETIVADA)
            await AtualizarSaldoContaAsync(request.ContaId.Value, request.Valor, tipo);

        await _context.SaveChangesAsync();

        if (tipo == TipoTransacao.DESPESA && transacao.Status == StatusTransacao.EFETIVADA && transacao.TransferenciaContaId == null)
            await VerificarAlertaOrcamentoAsync(usuarioId, request.CategoriaId, dataTransacao.Month, dataTransacao.Year);

        var response = await ObterTransacaoResponseAsync(transacao.Id);
        await _auditService.RegistrarAsync(usuarioId, "Transacao", transacao.Id, "CRIAR", null, new { transacao.Valor, transacao.Tipo, transacao.Status, transacao.CategoriaId });
        return Resultado<TransacaoResponse>.Criado(response!);
    }

    public async Task<Resultado<TransacaoResponse>> AtualizarAsync(int usuarioId, int transacaoId, AtualizarTransacaoRequest request)
    {
        var transacao = await _context.Transacoes
            .FirstOrDefaultAsync(t => t.Id == transacaoId && t.UsuarioId == usuarioId);

        if (transacao == null)
            return Resultado<TransacaoResponse>.NaoEncontrado("Transação não encontrada.");

        if (transacao.ParcelamentoId.HasValue)
            return Resultado<TransacaoResponse>.Falha("Não é possível editar parcelas individualmente. Exclua o parcelamento.");

        var valorAntigo = transacao.Valor;
        var tipoAntigo = transacao.Tipo;
        var contaAntigaId = transacao.ContaId;

        if (request.CategoriaId.HasValue)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == request.CategoriaId && c.Ativo);
            if (!categoriaExiste)
                return Resultado<TransacaoResponse>.Falha("Categoria não encontrada.");
            transacao.CategoriaId = request.CategoriaId.Value;
        }

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

        transacao.AtualizadoEm = DateTime.UtcNow;

        // Recalculate balance if value or account changed and transaction is EFETIVADA
        if (transacao.CartaoCreditoId == null && transacao.Status == StatusTransacao.EFETIVADA)
        {
            if (contaAntigaId.HasValue)
                await ReverterSaldoContaAsync(contaAntigaId.Value, valorAntigo, tipoAntigo);

            if (transacao.ContaId.HasValue)
                await AtualizarSaldoContaAsync(transacao.ContaId.Value, transacao.Valor, transacao.Tipo);
        }

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
        await _auditService.RegistrarAsync(usuarioId, "Transacao", transacao.Id, "EDITAR",
            new { Valor = valorAntigo, ContaId = contaAntigaId },
            new { transacao.Valor, transacao.ContaId, transacao.CategoriaId });
        return Resultado<TransacaoResponse>.Ok(response!, "Transação atualizada!");
    }

    public async Task<Resultado<TransacaoResponse>> AtualizarStatusAsync(int usuarioId, int transacaoId, AtualizarStatusRequest request)
    {
        var transacao = await _context.Transacoes
            .FirstOrDefaultAsync(t => t.Id == transacaoId && t.UsuarioId == usuarioId);

        if (transacao == null)
            return Resultado<TransacaoResponse>.NaoEncontrado("Transação não encontrada.");

        if (!Enum.TryParse<StatusTransacao>(request.Status, out var novoStatus))
            return Resultado<TransacaoResponse>.Falha("Status inválido. Use: PENDENTE, EFETIVADA ou CANCELADA.");

        var statusAtual = transacao.Status;

        // State machine: PENDENTE→EFETIVADA/CANCELADA, VENCIDA→EFETIVADA/CANCELADA, EFETIVADA→CANCELADA
        var transicoesValidas = new Dictionary<StatusTransacao, HashSet<StatusTransacao>>
        {
            [StatusTransacao.PENDENTE] = [StatusTransacao.EFETIVADA, StatusTransacao.CANCELADA],
            [StatusTransacao.VENCIDA] = [StatusTransacao.EFETIVADA, StatusTransacao.CANCELADA],
            [StatusTransacao.EFETIVADA] = [StatusTransacao.CANCELADA],
            [StatusTransacao.CANCELADA] = []
        };

        if (!transicoesValidas[statusAtual].Contains(novoStatus))
            return Resultado<TransacaoResponse>.Falha(
                $"Transição de status inválida: {statusAtual} → {novoStatus}.");

        transacao.Status = novoStatus;
        transacao.AtualizadoEm = DateTime.UtcNow;

        if ((statusAtual == StatusTransacao.PENDENTE || statusAtual == StatusTransacao.VENCIDA)
            && novoStatus == StatusTransacao.EFETIVADA)
        {
            if (transacao.ContaId.HasValue && transacao.CartaoCreditoId == null)
                await AtualizarSaldoContaAsync(transacao.ContaId.Value, transacao.Valor, transacao.Tipo);
        }

        if (statusAtual == StatusTransacao.EFETIVADA && novoStatus == StatusTransacao.CANCELADA)
        {
            if (transacao.ContaId.HasValue && transacao.CartaoCreditoId == null)
                await ReverterSaldoContaAsync(transacao.ContaId.Value, transacao.Valor, transacao.Tipo);

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
        }

        await _context.SaveChangesAsync();

        var response = await ObterTransacaoResponseAsync(transacao.Id);
        await _auditService.RegistrarAsync(usuarioId, "Transacao", transacao.Id, "MUDAR_STATUS",
            new { Status = statusAtual.ToString() },
            new { Status = novoStatus.ToString() });
        return Resultado<TransacaoResponse>.Ok(response!, "Status atualizado!");
    }

    public async Task<Resultado<bool>> ExcluirAsync(int usuarioId, int transacaoId)
    {
        var transacao = await _context.Transacoes
            .FirstOrDefaultAsync(t => t.Id == transacaoId && t.UsuarioId == usuarioId);

        if (transacao == null)
            return Resultado<bool>.NaoEncontrado("Transação não encontrada.");

        if (transacao.ParcelamentoId.HasValue)
            return await ExcluirParcelamentoAsync(usuarioId, transacao.ParcelamentoId.Value);

        if (transacao.ContaId.HasValue && transacao.CartaoCreditoId == null && transacao.Status == StatusTransacao.EFETIVADA)
            await ReverterSaldoContaAsync(transacao.ContaId.Value, transacao.Valor, transacao.Tipo);

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

        var valorExcluido = transacao.Valor;
        var tipoExcluido = transacao.Tipo;
        var statusExcluido = transacao.Status;
        var idExcluido = transacao.Id;

        _context.Transacoes.Remove(transacao);
        await _context.SaveChangesAsync();

        await _auditService.RegistrarAsync(usuarioId, "Transacao", idExcluido, "EXCLUIR",
            new { Valor = valorExcluido, Tipo = tipoExcluido.ToString(), Status = statusExcluido.ToString() }, null);

        return Resultado<bool>.Ok(true, "Transação excluída!");
    }

    public async Task<Resultado<bool>> CancelarParcelamentoAsync(int usuarioId, int parcelamentoId)
    {
        return await ExcluirParcelamentoAsync(usuarioId, parcelamentoId);
    }

    public async Task<Resultado<TransacaoResponse>> ObterPorIdAsync(int usuarioId, int transacaoId)
    {
        var response = await ObterTransacaoResponseAsync(transacaoId, usuarioId);
        if (response == null)
            return Resultado<TransacaoResponse>.NaoEncontrado("Transação não encontrada.");

        return Resultado<TransacaoResponse>.Ok(response);
    }

    public async Task<Resultado<ListaPaginada<TransacaoResponse>>> ListarAsync(int usuarioId, FiltroTransacaoRequest filtro)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId
                     && t.Status == StatusTransacao.PENDENTE
                     && t.DataTransacao < hoje)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, StatusTransacao.VENCIDA)
                .SetProperty(t => t.AtualizadoEm, DateTime.UtcNow));

        var query = _context.Transacoes
            .Include(t => t.Categoria)
            .Include(t => t.Conta)
            .Include(t => t.FormaPagamento)
            .Include(t => t.CartaoCredito)
            .Where(t => t.UsuarioId == usuarioId)
            .AsQueryable();

        if (!filtro.IncluirTransferencias)
            query = query.Where(t => t.TransferenciaContaId == null);

        if (filtro.Mes.HasValue)
            query = query.Where(t => t.DataTransacao.Month == filtro.Mes.Value);

        if (filtro.Ano.HasValue)
            query = query.Where(t => t.DataTransacao.Year == filtro.Ano.Value);

        if (filtro.DataInicio.HasValue)
            query = query.Where(t => t.DataTransacao >= filtro.DataInicio.Value);

        if (filtro.DataFim.HasValue)
            query = query.Where(t => t.DataTransacao <= filtro.DataFim.Value);

        if (!string.IsNullOrEmpty(filtro.Tipo) && Enum.TryParse<TipoTransacao>(filtro.Tipo, out var tipo))
            query = query.Where(t => t.Tipo == tipo);

        if (filtro.CategoriasIds != null && filtro.CategoriasIds.Count > 0)
            query = query.Where(t => filtro.CategoriasIds.Contains(t.CategoriaId));
        else if (filtro.CategoriaId.HasValue)
            query = query.Where(t => t.CategoriaId == filtro.CategoriaId.Value);

        if (filtro.ContaId.HasValue)
            query = query.Where(t => t.ContaId == filtro.ContaId.Value);

        if (filtro.CartaoCreditoId.HasValue)
            query = query.Where(t => t.CartaoCreditoId == filtro.CartaoCreditoId.Value);

        if (!string.IsNullOrEmpty(filtro.Origem) && Enum.TryParse<OrigemTransacao>(filtro.Origem, out var origem))
            query = query.Where(t => t.Origem == origem);

        if (!string.IsNullOrEmpty(filtro.Busca))
            query = query.Where(t => t.Descricao != null && t.Descricao.Contains(filtro.Busca));

        if (!string.IsNullOrEmpty(filtro.Status) && Enum.TryParse<StatusTransacao>(filtro.Status, out var statusFiltro))
            query = query.Where(t => t.Status == statusFiltro);

        var total = await query.CountAsync();

        var totalReceitas = await query
            .Where(t => t.Tipo == TipoTransacao.RECEITA && t.Status == StatusTransacao.EFETIVADA)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        var totalDespesas = await query
            .Where(t => t.Tipo == TipoTransacao.DESPESA && t.Status == StatusTransacao.EFETIVADA)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

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
                TransferenciaContaId = t.TransferenciaContaId,
                Valor = t.Valor,
                Tipo = t.Tipo.ToString(),
                Descricao = t.Descricao,
                DataTransacao = t.DataTransacao,
                Origem = t.Origem.ToString(),
                Status = t.Status.ToString(),
                Atrasada = t.Status == StatusTransacao.VENCIDA,
                Observacoes = t.Observacoes,
                Recorrente = t.Recorrente,
                NumeroParcela = t.NumeroParcela,
                TotalParcelas = t.TotalParcelas,
                ParcelamentoId = t.ParcelamentoId,
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
        int usuarioId,
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
                Status = (dataParcela <= hoje && cartao == null)
                    ? StatusTransacao.EFETIVADA
                    : StatusTransacao.PENDENTE
            };

            if (cartao != null)
            {
                var fatura = await ObterOuCriarFaturaAsync(cartao, usuarioId, dataParcela);
                transacao.FaturaCartaoId = fatura.Id;
                fatura.ValorTotal += valorEsta;
                fatura.AtualizadoEm = DateTime.UtcNow;
            }

            _context.Transacoes.Add(transacao);
        }

        if (cartao != null)
        {
            cartao.LimiteDisponivel -= request.Valor;
            cartao.AtualizadoEm = DateTime.UtcNow;
        }

        if (request.ContaId.HasValue && cartao == null)
            await AtualizarSaldoContaAsync(request.ContaId.Value, valorParcela, tipo);

        await _context.SaveChangesAsync();

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
                ParcelamentoId = t.ParcelamentoId,
                CriadoEm = t.CriadoEm
            })
            .FirstOrDefaultAsync();

        return Resultado<TransacaoResponse>.Criado(primeiraParcela!,
            $"Parcelamento criado: {totalParcelas}x de R${valorParcela:F2}");
    }

    private async Task<Resultado<bool>> ExcluirParcelamentoAsync(int usuarioId, int parcelamentoId)
    {
        var parcelas = await _context.Transacoes
            .Where(t => t.ParcelamentoId == parcelamentoId && t.UsuarioId == usuarioId)
            .ToListAsync();

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
                cartao = await _context.CartoesCredito.FindAsync(parcela.CartaoCreditoId);
        }

        if (cartao != null)
        {
            var valorTotal = parcelas.Sum(p => p.Valor);
            cartao.LimiteDisponivel += valorTotal;
            cartao.AtualizadoEm = DateTime.UtcNow;
        }

        foreach (var parcela in parcelas.Where(p => p.Status == StatusTransacao.EFETIVADA && p.ContaId.HasValue && p.CartaoCreditoId == null))
            await ReverterSaldoContaAsync(parcela.ContaId!.Value, parcela.Valor, parcela.Tipo);

        _context.Transacoes.RemoveRange(parcelas);

        var parcelamento = await _context.Parcelamentos.FindAsync(parcelamentoId);
        if (parcelamento != null)
            _context.Parcelamentos.Remove(parcelamento);

        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Parcelamento e todas as parcelas foram excluídos!");
    }

    private async Task<FaturaCartao> ObterOuCriarFaturaAsync(CartaoCredito cartao, int usuarioId, DateOnly dataTransacao)
    {
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

        var fatura = await _context.FaturasCartao
            .FirstOrDefaultAsync(f => f.CartaoCreditoId == cartao.Id
                                   && f.MesReferencia == mesFatura
                                   && f.AnoReferencia == anoFatura);

        if (fatura != null)
            return fatura;

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

    private async Task VerificarAlertaOrcamentoAsync(int usuarioId, int categoriaId, int mes, int ano)
    {
        var orcamento = await _context.Orcamentos
            .Include(o => o.Categoria)
            .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId
                                   && o.CategoriaId == categoriaId
                                   && o.Mes == mes
                                   && o.Ano == ano);

        if (orcamento == null) return;

        var totalGasto = await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId
                     && t.CategoriaId == categoriaId
                     && t.Tipo == TipoTransacao.DESPESA
                     && t.Status == StatusTransacao.EFETIVADA
                     && t.DataTransacao.Month == mes
                     && t.DataTransacao.Year == ano)
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        var percentual = orcamento.ValorLimite > 0
            ? Math.Round(totalGasto / orcamento.ValorLimite * 100, 0)
            : 0;

        var limiteAlerta = orcamento.PercentualAlerta > 0 ? orcamento.PercentualAlerta : 80;

        if (percentual >= 100)
        {
            var jaGerou100 = await _context.Notificacoes
                .AnyAsync(n => n.UsuarioId == usuarioId
                            && n.Tipo == TipoNotificacao.ALERTA_ORCAMENTO_100
                            && n.EntidadeRelacionadaId == orcamento.Id
                            && n.CriadoEm.Month == mes
                            && n.CriadoEm.Year == ano);

            if (!jaGerou100)
                await _notificacaoService.CriarAsync(
                    usuarioId,
                    TipoNotificacao.ALERTA_ORCAMENTO_100,
                    "Orçamento estourado!",
                    $"Você ultrapassou 100% do orçamento de '{orcamento.Categoria.Nome}' (R${orcamento.ValorLimite:F2}).",
                    orcamento.Id);
        }
        else if (percentual >= limiteAlerta)
        {
            var jaGerou80 = await _context.Notificacoes
                .AnyAsync(n => n.UsuarioId == usuarioId
                            && n.Tipo == TipoNotificacao.ALERTA_ORCAMENTO_80
                            && n.EntidadeRelacionadaId == orcamento.Id
                            && n.CriadoEm.Month == mes
                            && n.CriadoEm.Year == ano);

            if (!jaGerou80)
                await _notificacaoService.CriarAsync(
                    usuarioId,
                    TipoNotificacao.ALERTA_ORCAMENTO_80,
                    "Atenção ao orçamento!",
                    $"{percentual}% do orçamento de '{orcamento.Categoria.Nome}' utilizado (R${totalGasto:F2} de R${orcamento.ValorLimite:F2}).",
                    orcamento.Id);
        }
    }

    private async Task AtualizarSaldoContaAsync(int contaId, decimal valor, TipoTransacao tipo)
    {
        var conta = await _context.Contas.FindAsync(contaId);
        if (conta == null) return;

        conta.SaldoAtual += tipo == TipoTransacao.RECEITA ? valor : -valor;
        conta.AtualizadoEm = DateTime.UtcNow;
    }

    private async Task ReverterSaldoContaAsync(int contaId, decimal valor, TipoTransacao tipo)
    {
        var conta = await _context.Contas.FindAsync(contaId);
        if (conta == null) return;

        conta.SaldoAtual += tipo == TipoTransacao.RECEITA ? -valor : valor;
        conta.AtualizadoEm = DateTime.UtcNow;
    }

    private async Task<TransacaoResponse?> ObterTransacaoResponseAsync(int transacaoId, int? usuarioId = null)
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
                TransferenciaContaId = t.TransferenciaContaId,
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
                ParcelamentoId = t.ParcelamentoId,
                CriadoEm = t.CriadoEm
            })
            .FirstOrDefaultAsync();
    }
}
