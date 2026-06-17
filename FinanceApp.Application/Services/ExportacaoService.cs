using FinanceApp.Application.DTOs.Exportacao;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinanceApp.Application.Services;

public class ExportacaoService : IExportacaoService
{
    private readonly FinanceDbContext _context;
    private readonly UserManager<Domain.Entities.Usuario> _userManager;

    public ExportacaoService(FinanceDbContext context, UserManager<Domain.Entities.Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Resultado<ExportacaoPdfResponseDTO>> GerarExtratoPdfAsync(
        int usuarioId, DateTime dataInicio, DateTime dataFim)
    {
        if ((dataFim - dataInicio).TotalDays > 92)
            return Resultado<ExportacaoPdfResponseDTO>.Falha("O período máximo para exportação é de 3 meses.");

        if (dataFim < dataInicio)
            return Resultado<ExportacaoPdfResponseDTO>.Falha("A data fim deve ser posterior à data início.");

        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
        if (usuario == null)
            return Resultado<ExportacaoPdfResponseDTO>.Falha("Usuário não encontrado.");

        var inicio = DateOnly.FromDateTime(dataInicio);
        var fim = DateOnly.FromDateTime(dataFim);

        var transacoes = await _context.Transacoes
            .Include(t => t.Categoria)
            .Include(t => t.Conta)
            .Where(t => t.UsuarioId == usuarioId
                     && t.DataTransacao >= inicio
                     && t.DataTransacao <= fim
                     && t.Status != StatusTransacao.CANCELADA)
            .OrderBy(t => t.DataTransacao)
            .ThenBy(t => t.Id)
            .ToListAsync();

        var totalReceitas = transacoes
            .Where(t => t.Tipo == TipoTransacao.RECEITA)
            .Sum(t => t.Valor);
        var totalDespesas = transacoes
            .Where(t => t.Tipo == TipoTransacao.DESPESA)
            .Sum(t => t.Valor);
        var saldo = totalReceitas - totalDespesas;

        QuestPDF.Settings.License = LicenseType.Community;

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("FinanceApp — Extrato Financeiro")
                        .FontSize(18).Bold().FontColor(Colors.Indigo.Medium);
                    col.Item().Text($"{usuario.NomeCompleto} | {usuario.Email}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2);
                    col.Item().Text($"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    // Transações
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(90);
                            columns.ConstantColumn(60);
                        });

                        // Cabeçalho
                        table.Header(header =>
                        {
                            void HeaderCell(string text) =>
                                header.Cell().Background(Colors.Indigo.Medium).Padding(4)
                                    .Text(text).FontColor(Colors.White).Bold().FontSize(9);

                            HeaderCell("Data");
                            HeaderCell("Descrição / Categoria");
                            HeaderCell("Conta");
                            HeaderCell("Valor");
                            HeaderCell("Tipo");
                        });

                        // Linhas
                        foreach (var (t, i) in transacoes.Select((t, i) => (t, i)))
                        {
                            var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            var valorColor = t.Tipo == TipoTransacao.RECEITA
                                ? Colors.Green.Darken2 : Colors.Red.Darken2;
                            var descricao = string.IsNullOrWhiteSpace(t.Descricao)
                                ? t.Categoria.Nome : t.Descricao;

                            table.Cell().Background(bg).Padding(4)
                                .Text(t.DataTransacao.ToString("dd/MM/yy")).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Column(c =>
                            {
                                c.Item().Text(descricao).FontSize(9).Bold();
                                c.Item().Text(t.Categoria.Nome).FontSize(8).FontColor(Colors.Grey.Darken1);
                            });
                            table.Cell().Background(bg).Padding(4)
                                .Text(t.Conta?.Nome ?? "-").FontSize(9).FontColor(Colors.Grey.Darken2);
                            table.Cell().Background(bg).Padding(4)
                                .Text($"R$ {t.Valor:N2}").FontSize(9).Bold().FontColor(valorColor);
                            table.Cell().Background(bg).Padding(4)
                                .Text(t.Tipo == TipoTransacao.RECEITA ? "Receita" : "Despesa")
                                .FontSize(9).FontColor(valorColor);
                        }
                    });

                    // Resumo
                    col.Item().PaddingTop(16).BorderTop(1).BorderColor(Colors.Grey.Medium)
                        .PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Total Receitas: R$ {totalReceitas:N2}")
                                .FontSize(11).Bold().FontColor(Colors.Green.Darken2);
                            c.Item().Text($"Total Despesas: R$ {totalDespesas:N2}")
                                .FontSize(11).Bold().FontColor(Colors.Red.Darken2);
                            c.Item().PaddingTop(4).Text($"Saldo do período: R$ {saldo:N2}")
                                .FontSize(13).Bold()
                                .FontColor(saldo >= 0 ? Colors.Green.Darken3 : Colors.Red.Darken3);
                        });
                        row.ConstantItem(120).AlignRight().Column(c =>
                        {
                            c.Item().Text($"{transacoes.Count} transações").FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Gerado em ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span(" — FinanceApp").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();

        var nomeArquivo = $"extrato_{dataInicio:yyyyMMdd}_{dataFim:yyyyMMdd}.pdf";

        return Resultado<ExportacaoPdfResponseDTO>.Ok(new ExportacaoPdfResponseDTO
        {
            Bytes = bytes,
            NomeArquivo = nomeArquivo,
        });
    }
}
