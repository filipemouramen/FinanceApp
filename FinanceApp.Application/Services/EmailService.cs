using FinanceApp.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace FinanceApp.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task EnviarCodigoResetAsync(string email, string codigo)
    {
        var host = _config["Email:Host"];
        var portaStr = _config["Email:Porta"] ?? "587";
        var usuario = _config["Email:Usuario"];
        var senha = _config["Email:Senha"];
        var remetente = _config["Email:Remetente"] ?? usuario;

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(usuario))
        {
            _logger.LogWarning("SMTP não configurado. Código para {Email}: {Codigo}", email, codigo);
            return;
        }

        var mensagem = new MimeMessage();
        mensagem.From.Add(MailboxAddress.Parse(remetente));
        mensagem.To.Add(MailboxAddress.Parse(email));
        mensagem.Subject = "Redefinição de senha — Finance App";
        mensagem.Body = new TextPart("html")
        {
            Text = $@"
<div style='font-family:sans-serif;max-width:480px;margin:0 auto;padding:24px'>
  <h2 style='color:#6C63FF'>Finance App</h2>
  <p>Recebemos uma solicitação para redefinir sua senha.</p>
  <p>Seu código de verificação é:</p>
  <div style='font-size:32px;font-weight:bold;letter-spacing:8px;color:#6C63FF;
              background:#F0EEFF;border-radius:8px;padding:16px;text-align:center'>
    {codigo}
  </div>
  <p style='color:#666;font-size:13px;margin-top:16px'>Válido por 15 minutos. Ignore este e-mail se não solicitou.</p>
</div>"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, int.Parse(portaStr), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(usuario, senha);
        await client.SendAsync(mensagem);
        await client.DisconnectAsync(true);
    }
}
