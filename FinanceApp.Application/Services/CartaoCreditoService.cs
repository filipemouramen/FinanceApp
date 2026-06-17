using FinanceApp.Application.DTOs.CartoesCredito;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;

public class CartaoCreditoService : ICartaoCreditoService
{
    private readonly FinanceDbContext _context;
    private readonly INotificacaoService _notificacaoService;

    public CartaoCreditoService(FinanceDbContext context, INotificacaoService notificacaoService)
    {
        _context = context;
        _notificacaoService = notificacaoService;
    }

    public async Task<Resultado<List<CartaoCreditoResponse>>> ListarAsync(int usuarioId)
    {
        var cartoes = await _context.CartoesCredito
            .Include(c => c.Conta)
            .Where(c => c.UsuarioId == usuarioId && c.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new CartaoCreditoResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Bandeira = c.Bandeira,
                UltimosDigitos = c.UltimosDigitos,
                LimiteTotal = c.LimiteTotal,
                LimiteDisponivel = c.LimiteDisponivel,
                DiaFechamento = c.DiaFechamento,
                DiaVencimento = c.DiaVencimento,
                Cor = c.Cor,
                ContaId = c.ContaId,
                NomeConta = c.Conta != null ? c.Conta.Nome : null,
                Ativo = c.Ativo
            })
            .ToListAsync();

        return Resultado<List<CartaoCreditoResponse>>.Ok(cartoes);
    }

    public async Task<Resultado<CartaoCreditoResponse>> ObterPorIdAsync(int usuarioId, int cartaoId)
    {
        var cartao = await _context.CartoesCredito
            .Include(c => c.Conta)
            .Where(c => c.Id == cartaoId && c.UsuarioId == usuarioId && c.Ativo)
            .Select(c => new CartaoCreditoResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Bandeira = c.Bandeira,
                UltimosDigitos = c.UltimosDigitos,
                LimiteTotal = c.LimiteTotal,
                LimiteDisponivel = c.LimiteDisponivel,
                DiaFechamento = c.DiaFechamento,
                DiaVencimento = c.DiaVencimento,
                Cor = c.Cor,
                ContaId = c.ContaId,
                NomeConta = c.Conta != null ? c.Conta.Nome : null,
                Ativo = c.Ativo
            })
            .FirstOrDefaultAsync();

        if (cartao == null)
            return Resultado<CartaoCreditoResponse>.NaoEncontrado("Cartão não encontrado.");

        return Resultado<CartaoCreditoResponse>.Ok(cartao);
    }

    public async Task<Resultado<CartaoCreditoResponse>> CriarAsync(int usuarioId, CriarCartaoCreditoRequest request)
    {
        if (request.ContaId.HasValue)
        {
            var contaExiste = await _context.Contas
                .AnyAsync(c => c.Id == request.ContaId && c.UsuarioId == usuarioId && c.Ativo);
            if (!contaExiste)
                return Resultado<CartaoCreditoResponse>.Falha("Conta vinculada não encontrada.");
        }

        var cartao = new CartaoCredito
        {
            UsuarioId = usuarioId,
            ContaId = request.ContaId,
            Nome = request.Nome,
            Bandeira = request.Bandeira,
            UltimosDigitos = request.UltimosDigitos,
            LimiteTotal = request.LimiteTotal,
            LimiteDisponivel = request.LimiteTotal,
            DiaFechamento = request.DiaFechamento,
            DiaVencimento = request.DiaVencimento,
            Cor = request.Cor
        };

        _context.CartoesCredito.Add(cartao);

        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = usuarioId,
            Acao = "CARTAO_CRIADO",
            TipoEntidade = "CartaoCredito",
            EntidadeId = cartao.Id.ToString(),
            Detalhes = $"Cartão '{cartao.Nome}' criado com limite de R${cartao.LimiteTotal:F2}"
        });

        await _context.SaveChangesAsync();

        var response = await _context.CartoesCredito
            .Include(c => c.Conta)
            .Where(c => c.Id == cartao.Id)
            .Select(c => new CartaoCreditoResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Bandeira = c.Bandeira,
                UltimosDigitos = c.UltimosDigitos,
                LimiteTotal = c.LimiteTotal,
                LimiteDisponivel = c.LimiteDisponivel,
                DiaFechamento = c.DiaFechamento,
                DiaVencimento = c.DiaVencimento,
                Cor = c.Cor,
                ContaId = c.ContaId,
                NomeConta = c.Conta != null ? c.Conta.Nome : null,
                Ativo = c.Ativo
            })
            .FirstAsync();

        return Resultado<CartaoCreditoResponse>.Criado(response, "Cartão criado com sucesso!");
    }

    public async Task<Resultado<CartaoCreditoResponse>> AtualizarAsync(int usuarioId, int cartaoId, AtualizarCartaoCreditoRequest request)
    {
        var cartao = await _context.CartoesCredito
            .Include(c => c.Conta)
            .FirstOrDefaultAsync(c => c.Id == cartaoId && c.UsuarioId == usuarioId && c.Ativo);

        if (cartao == null)
            return Resultado<CartaoCreditoResponse>.NaoEncontrado("Cartão não encontrado.");

        if (request.ContaId.HasValue)
        {
            var contaExiste = await _context.Contas
                .AnyAsync(c => c.Id == request.ContaId && c.UsuarioId == usuarioId && c.Ativo);
            if (!contaExiste)
                return Resultado<CartaoCreditoResponse>.Falha("Conta vinculada não encontrada.");
            cartao.ContaId = request.ContaId;
        }

        if (!string.IsNullOrWhiteSpace(request.Nome))
            cartao.Nome = request.Nome;

        if (request.Bandeira != null)
            cartao.Bandeira = request.Bandeira;

        if (!string.IsNullOrWhiteSpace(request.Cor))
            cartao.Cor = request.Cor;

        if (request.DiaFechamento.HasValue)
            cartao.DiaFechamento = request.DiaFechamento.Value;

        if (request.DiaVencimento.HasValue)
            cartao.DiaVencimento = request.DiaVencimento.Value;

        if (request.LimiteTotal.HasValue && request.LimiteTotal.Value != cartao.LimiteTotal)
        {
            var limiteUsado = cartao.LimiteTotal - cartao.LimiteDisponivel;
            cartao.LimiteTotal = request.LimiteTotal.Value;
            cartao.LimiteDisponivel = request.LimiteTotal.Value - limiteUsado;

            if (cartao.LimiteDisponivel < 0)
                cartao.LimiteDisponivel = 0;
        }

        cartao.AtualizadoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Resultado<CartaoCreditoResponse>.Ok(new CartaoCreditoResponse
        {
            Id = cartao.Id,
            Nome = cartao.Nome,
            Bandeira = cartao.Bandeira,
            UltimosDigitos = cartao.UltimosDigitos,
            LimiteTotal = cartao.LimiteTotal,
            LimiteDisponivel = cartao.LimiteDisponivel,
            DiaFechamento = cartao.DiaFechamento,
            DiaVencimento = cartao.DiaVencimento,
            Cor = cartao.Cor,
            ContaId = cartao.ContaId,
            NomeConta = cartao.Conta?.Nome,
            Ativo = cartao.Ativo
        }, "Cartão atualizado!");
    }

    public async Task<Resultado<bool>> ExcluirAsync(int usuarioId, int cartaoId)
    {
        var cartao = await _context.CartoesCredito
            .FirstOrDefaultAsync(c => c.Id == cartaoId && c.UsuarioId == usuarioId && c.Ativo);

        if (cartao == null)
            return Resultado<bool>.NaoEncontrado("Cartão não encontrado.");

        var temFaturasAbertas = await _context.FaturasCartao
            .AnyAsync(f => f.CartaoCreditoId == cartaoId
                        && (f.Status == StatusFatura.ABERTA || f.Status == StatusFatura.FECHADA));

        if (temFaturasAbertas)
            return Resultado<bool>.Falha("Não é possível excluir o cartão. Existem faturas abertas ou pendentes de pagamento.");

        // soft delete via AuditInterceptor
        _context.CartoesCredito.Remove(cartao);
        await _context.SaveChangesAsync();

        return Resultado<bool>.Ok(true, "Cartão desativado com sucesso!");
    }

    public async Task<Resultado<FaturaCartaoResponse>> ObterFaturaAsync(int usuarioId, int cartaoId, int mes, int ano)
    {
        var cartao = await _context.CartoesCredito
            .FirstOrDefaultAsync(c => c.Id == cartaoId && c.UsuarioId == usuarioId);
        if (cartao == null)
            return Resultado<FaturaCartaoResponse>.NaoEncontrado("Cartão não encontrado.");

        var fatura = await _context.FaturasCartao
            .FirstOrDefaultAsync(f => f.CartaoCreditoId == cartaoId
                                   && f.MesReferencia == mes
                                   && f.AnoReferencia == ano);

        if (fatura == null)
            return Resultado<FaturaCartaoResponse>.NaoEncontrado($"Nenhuma fatura encontrada para {mes:D2}/{ano}.");

        var transacoes = await _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.FaturaCartaoId == fatura.Id)
            .OrderByDescending(t => t.DataTransacao)
            .Select(t => new TransacaoFaturaResponse
            {
                Id = t.Id,
                Descricao = t.Descricao,
                NomeCategoria = t.Categoria.Nome,
                CorCategoria = t.Categoria.Cor,
                Valor = t.Valor,
                DataTransacao = t.DataTransacao,
                NumeroParcela = t.NumeroParcela,
                TotalParcelas = t.TotalParcelas
            })
            .ToListAsync();

        var response = new FaturaCartaoResponse
        {
            Id = fatura.Id,
            CartaoCreditoId = fatura.CartaoCreditoId,
            NomeCartao = cartao.Nome,
            MesReferencia = fatura.MesReferencia,
            AnoReferencia = fatura.AnoReferencia,
            DataFechamento = fatura.DataFechamento,
            DataVencimento = fatura.DataVencimento,
            DataPagamento = fatura.DataPagamento,
            ValorTotal = fatura.ValorTotal,
            ValorPago = fatura.ValorPago,
            Status = fatura.Status.ToString(),
            Transacoes = transacoes
        };

        return Resultado<FaturaCartaoResponse>.Ok(response);
    }

    public async Task<Resultado<FaturaCartaoResponse>> ObterFaturaPorIdAsync(int usuarioId, int cartaoId, int faturaId)
    {
        var cartao = await _context.CartoesCredito
            .FirstOrDefaultAsync(c => c.Id == cartaoId && c.UsuarioId == usuarioId);
        if (cartao == null)
            return Resultado<FaturaCartaoResponse>.NaoEncontrado("Cartão não encontrado.");

        var fatura = await _context.FaturasCartao
            .FirstOrDefaultAsync(f => f.Id == faturaId && f.CartaoCreditoId == cartaoId);
        if (fatura == null)
            return Resultado<FaturaCartaoResponse>.NaoEncontrado("Fatura não encontrada.");

        var transacoes = await _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.FaturaCartaoId == fatura.Id)
            .OrderByDescending(t => t.DataTransacao)
            .Select(t => new TransacaoFaturaResponse
            {
                Id = t.Id,
                Descricao = t.Descricao,
                NomeCategoria = t.Categoria.Nome,
                CorCategoria = t.Categoria.Cor,
                Valor = t.Valor,
                DataTransacao = t.DataTransacao,
                NumeroParcela = t.NumeroParcela,
                TotalParcelas = t.TotalParcelas
            })
            .ToListAsync();

        return Resultado<FaturaCartaoResponse>.Ok(new FaturaCartaoResponse
        {
            Id = fatura.Id,
            CartaoCreditoId = fatura.CartaoCreditoId,
            NomeCartao = cartao.Nome,
            MesReferencia = fatura.MesReferencia,
            AnoReferencia = fatura.AnoReferencia,
            DataFechamento = fatura.DataFechamento,
            DataVencimento = fatura.DataVencimento,
            DataPagamento = fatura.DataPagamento,
            ValorTotal = fatura.ValorTotal,
            ValorPago = fatura.ValorPago,
            Status = fatura.Status.ToString(),
            Transacoes = transacoes
        });
    }

    public async Task<Resultado<List<FaturaCartaoResponse>>> ListarFaturasAsync(int usuarioId, int cartaoId)
    {
        var cartao = await _context.CartoesCredito
            .FirstOrDefaultAsync(c => c.Id == cartaoId && c.UsuarioId == usuarioId);
        if (cartao == null)
            return Resultado<List<FaturaCartaoResponse>>.NaoEncontrado("Cartão não encontrado.");

        await FecharFaturasVencidasAsync(usuarioId, cartaoId, cartao.Nome);

        var faturas = await _context.FaturasCartao
            .Where(f => f.CartaoCreditoId == cartaoId)
            .OrderByDescending(f => f.AnoReferencia)
            .ThenByDescending(f => f.MesReferencia)
            .Select(f => new FaturaCartaoResponse
            {
                Id = f.Id,
                CartaoCreditoId = f.CartaoCreditoId,
                NomeCartao = cartao.Nome,
                MesReferencia = f.MesReferencia,
                AnoReferencia = f.AnoReferencia,
                DataFechamento = f.DataFechamento,
                DataVencimento = f.DataVencimento,
                DataPagamento = f.DataPagamento,
                ValorTotal = f.ValorTotal,
                ValorPago = f.ValorPago,
                Status = f.Status.ToString(),
                Transacoes = new List<TransacaoFaturaResponse>()
            })
            .ToListAsync();

        return Resultado<List<FaturaCartaoResponse>>.Ok(faturas);
    }

    public async Task<Resultado<bool>> PagarFaturaAsync(int usuarioId, int faturaId, PagarFaturaRequest request)
    {
        var fatura = await _context.FaturasCartao
            .Include(f => f.CartaoCredito)
            .FirstOrDefaultAsync(f => f.Id == faturaId && f.UsuarioId == usuarioId);

        if (fatura == null)
            return Resultado<bool>.NaoEncontrado("Fatura não encontrada.");

        if (fatura.Status == StatusFatura.PAGA)
            return Resultado<bool>.Falha("Esta fatura já foi paga.");

        var conta = await _context.Contas
            .FirstOrDefaultAsync(c => c.Id == request.ContaId && c.UsuarioId == usuarioId && c.Ativo);
        if (conta == null)
            return Resultado<bool>.Falha("Conta para débito não encontrada.");

        var valorRestante = fatura.ValorTotal - fatura.ValorPago;

        if (request.Valor > valorRestante)
            return Resultado<bool>.Falha(
                $"Valor do pagamento (R${request.Valor:F2}) excede o restante da fatura (R${valorRestante:F2}).");

        if (conta.SaldoAtual < request.Valor)
            return Resultado<bool>.Falha(
                $"Saldo insuficiente. Disponível: R${conta.SaldoAtual:F2}, Necessário: R${request.Valor:F2}");

        conta.SaldoAtual -= request.Valor;
        conta.AtualizadoEm = DateTime.UtcNow;

        fatura.ValorPago += request.Valor;
        fatura.AtualizadoEm = DateTime.UtcNow;

        if (fatura.ValorPago >= fatura.ValorTotal)
        {
            fatura.Status = StatusFatura.PAGA;
            fatura.DataPagamento = request.DataPagamento ?? DateOnly.FromDateTime(DateTime.UtcNow);
        }
        else
        {
            fatura.Status = StatusFatura.PARCIAL;
        }

        var cartao = fatura.CartaoCredito;
        cartao.LimiteDisponivel += request.Valor;
        cartao.AtualizadoEm = DateTime.UtcNow;

        var transacaoPagamento = new Transacao
        {
            UsuarioId = usuarioId,
            CategoriaId = 13,
            ContaId = request.ContaId,
            Valor = request.Valor,
            Tipo = TipoTransacao.DESPESA,
            Descricao = $"Pagamento fatura {cartao.Nome} - {fatura.MesReferencia:D2}/{fatura.AnoReferencia}",
            DataTransacao = DateOnly.FromDateTime(DateTime.UtcNow),
            Origem = OrigemTransacao.APP,
            Status = StatusTransacao.EFETIVADA
        };
        _context.Transacoes.Add(transacaoPagamento);

        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = usuarioId,
            Acao = "PAGAMENTO_FATURA",
            TipoEntidade = "FaturaCartao",
            EntidadeId = faturaId.ToString(),
            Detalhes = $"R${request.Valor:F2} pago da fatura {cartao.Nome} {fatura.MesReferencia:D2}/{fatura.AnoReferencia}. Status: {fatura.Status}"
        });

        await _context.SaveChangesAsync();

        var statusMsg = fatura.Status == StatusFatura.PAGA
            ? "Fatura paga integralmente!"
            : $"Pagamento parcial de R${request.Valor:F2} registrado. Restam R${(fatura.ValorTotal - fatura.ValorPago):F2}.";

        return Resultado<bool>.Ok(true, statusMsg);
    }

    private async Task FecharFaturasVencidasAsync(int usuarioId, int cartaoId, string nomeCartao)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var faturasParaFechar = await _context.FaturasCartao
            .Where(f => f.CartaoCreditoId == cartaoId
                     && f.Status == StatusFatura.ABERTA
                     && f.DataFechamento < hoje)
            .ToListAsync();

        foreach (var fatura in faturasParaFechar)
        {
            fatura.Status = StatusFatura.FECHADA;
            fatura.AtualizadoEm = DateTime.UtcNow;

            await _notificacaoService.CriarAsync(
                usuarioId,
                TipoNotificacao.FATURA_FECHADA,
                "Fatura fechada",
                $"A fatura de {fatura.MesReferencia:D2}/{fatura.AnoReferencia} do cartão {nomeCartao} foi fechada. Valor: R${fatura.ValorTotal:F2}. Vence em {fatura.DataVencimento:dd/MM/yyyy}.",
                fatura.Id);
        }

        if (faturasParaFechar.Count > 0)
            await _context.SaveChangesAsync();
    }
}
