using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Application.DTOs.Configuracoes;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Application.Services;
public class ConfiguracaoService : IConfiguracaoService
{
    private readonly FinanceDbContext _context;

    public ConfiguracaoService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Resultado<ConfiguracaoResponse>> ObterAsync(Guid usuarioId)
    {
        var config = await _context.ConfiguracoesUsuario
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (config == null)
        {
            config = new ConfiguracaoUsuario { UsuarioId = usuarioId };
            _context.ConfiguracoesUsuario.Add(config);
            await _context.SaveChangesAsync();
        }

        return Resultado<ConfiguracaoResponse>.Ok(new ConfiguracaoResponse
        {
            Moeda = config.Moeda,
            DiaInicioMes = config.DiaInicioMes,
            WhatsAppAtivado = config.WhatsAppAtivado,
            NotificacoesPush = config.NotificacoesPush,
            AlertasOrcamento = config.AlertasOrcamento,
            AlertasFatura = config.AlertasFatura,
            ModoEscuro = config.ModoEscuro,
            Idioma = config.Idioma
        });
    }

    public async Task<Resultado<ConfiguracaoResponse>> AtualizarAsync(Guid usuarioId, AtualizarConfiguracaoRequest request)
    {
        var config = await _context.ConfiguracoesUsuario
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (config == null)
        {
            config = new ConfiguracaoUsuario { UsuarioId = usuarioId };
            _context.ConfiguracoesUsuario.Add(config);
        }

        if (request.Moeda != null)
            config.Moeda = request.Moeda;

        if (request.DiaInicioMes.HasValue)
        {
            if (request.DiaInicioMes.Value < 1 || request.DiaInicioMes.Value > 28)
                return Resultado<ConfiguracaoResponse>.Falha("Dia de início do mês deve ser entre 1 e 28.");
            config.DiaInicioMes = request.DiaInicioMes.Value;
        }

        if (request.WhatsAppAtivado.HasValue)
        {
            if (request.WhatsAppAtivado.Value)
            {
                var usuario = await _context.Users.FindAsync(usuarioId);
                if (string.IsNullOrEmpty(usuario?.TelefoneWhatsApp))
                    return Resultado<ConfiguracaoResponse>.Falha(
                        "Cadastre um número de WhatsApp no seu perfil antes de ativar.");
            }
            config.WhatsAppAtivado = request.WhatsAppAtivado.Value;
        }

        if (request.NotificacoesPush.HasValue)
            config.NotificacoesPush = request.NotificacoesPush.Value;

        if (request.AlertasOrcamento.HasValue)
            config.AlertasOrcamento = request.AlertasOrcamento.Value;

        if (request.AlertasFatura.HasValue)
            config.AlertasFatura = request.AlertasFatura.Value;

        if (request.ModoEscuro.HasValue)
            config.ModoEscuro = request.ModoEscuro.Value;

        if (request.Idioma != null)
            config.Idioma = request.Idioma;

        config.AtualizadoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Resultado<ConfiguracaoResponse>.Ok(new ConfiguracaoResponse
        {
            Moeda = config.Moeda,
            DiaInicioMes = config.DiaInicioMes,
            WhatsAppAtivado = config.WhatsAppAtivado,
            NotificacoesPush = config.NotificacoesPush,
            AlertasOrcamento = config.AlertasOrcamento,
            AlertasFatura = config.AlertasFatura,
            ModoEscuro = config.ModoEscuro,
            Idioma = config.Idioma
        }, "Configurações atualizadas!");
    }
}