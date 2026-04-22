using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.Data;

public class FinanceDbContext : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }

    public DbSet<TokenAtualizacao> TokensAtualizacao => Set<TokenAtualizacao>();
    public DbSet<CodigoVerificacao> CodigosVerificacao => Set<CodigoVerificacao>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<FormaPagamento> FormasPagamento => Set<FormaPagamento>();
    public DbSet<Conta> Contas => Set<Conta>();
    public DbSet<CartaoCredito> CartoesCredito => Set<CartaoCredito>();
    public DbSet<FaturaCartao> FaturasCartao => Set<FaturaCartao>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();
    public DbSet<Parcelamento> Parcelamentos => Set<Parcelamento>();
    public DbSet<RegraRecorrencia> RegrasRecorrencia => Set<RegraRecorrencia>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
    public DbSet<MetaEconomia> MetasEconomia => Set<MetaEconomia>();
    public DbSet<LancamentoMeta> LancamentosMeta => Set<LancamentoMeta>();
    public DbSet<TransferenciaConta> TransferenciasContas => Set<TransferenciaConta>();
    public DbSet<Anexo> Anexos => Set<Anexo>();
    public DbSet<MensagemWhatsApp> MensagensWhatsApp => Set<MensagemWhatsApp>();
    public DbSet<ApelidoCategoria> ApelidosCategorias => Set<ApelidoCategoria>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<ConfiguracaoUsuario> ConfiguracoesUsuario => Set<ConfiguracaoUsuario>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Usuario>().ToTable("Usuarios", "FinanceApp");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles", "FinanceApp");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UsuarioRoles", "FinanceApp");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UsuarioClaims", "FinanceApp");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UsuarioLogins", "FinanceApp");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UsuarioTokens", "FinanceApp");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "FinanceApp");

        builder.Entity<Usuario>(e =>
        {
            e.Property(u => u.NomeCompleto).HasMaxLength(150).IsRequired();
            e.Property(u => u.TelefoneWhatsApp).HasMaxLength(20);
            e.Property(u => u.FotoUrl).HasMaxLength(500);
            e.HasIndex(u => u.TelefoneWhatsApp)
                .IsUnique()
                .HasFilter("[TelefoneWhatsApp] IS NOT NULL");
        });

        builder.Entity<TokenAtualizacao>(e =>
        {
            e.ToTable("TokensAtualizacao", "FinanceApp");
            e.HasKey(t => t.Id);
            e.Property(t => t.Token).HasMaxLength(512).IsRequired();
            e.Property(t => t.SubstituidoPor).HasMaxLength(512);
            e.HasIndex(t => t.Token);
            e.HasIndex(t => t.UsuarioId);
            e.Ignore(t => t.Expirado);
            e.Ignore(t => t.Ativo);
            e.HasOne(t => t.Usuario)
                .WithMany(u => u.TokensAtualizacao)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CodigoVerificacao>(e =>
        {
            e.ToTable("CodigosVerificacao", "FinanceApp");
            e.HasKey(c => c.Id);
            e.Property(c => c.Codigo).HasMaxLength(10).IsRequired();
            e.Property(c => c.Finalidade).HasMaxLength(30)
                .HasConversion<string>().IsRequired();
            e.Ignore(c => c.Expirado);
            e.Ignore(c => c.Valido);
            e.HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias", "FinanceApp");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).UseIdentityColumn();
            e.Property(c => c.Nome).HasMaxLength(80).IsRequired();
            e.Property(c => c.Icone).HasMaxLength(50).IsRequired();
            e.Property(c => c.Cor).HasMaxLength(7);
            e.Property(c => c.Tipo).HasMaxLength(10)
                .HasConversion<string>().IsRequired();
            e.HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<FormaPagamento>(e =>
        {
            e.ToTable("FormasPagamento", "FinanceApp");
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).UseIdentityColumn();
            e.Property(f => f.Nome).HasMaxLength(50).IsRequired();
            e.Property(f => f.Icone).HasMaxLength(50).IsRequired();
        });

        builder.Entity<Conta>(e =>
        {
            e.ToTable("Contas", "FinanceApp");
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).HasMaxLength(100).IsRequired();
            e.Property(c => c.TipoConta).HasMaxLength(20)
                .HasConversion<string>().IsRequired();
            e.Property(c => c.Banco).HasMaxLength(80);
            e.Property(c => c.Cor).HasMaxLength(7);
            e.Property(c => c.Icone).HasMaxLength(50);
            e.Property(c => c.SaldoInicial).HasColumnType("decimal(18,2)");
            e.Property(c => c.SaldoAtual).HasColumnType("decimal(18,2)");
            e.HasIndex(c => c.UsuarioId);
            e.HasOne(c => c.Usuario)
                .WithMany(u => u.Contas)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CartaoCredito>(e =>
        {
            e.ToTable("CartoesCredito", "FinanceApp");
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).HasMaxLength(100).IsRequired();
            e.Property(c => c.Bandeira).HasMaxLength(30);
            e.Property(c => c.UltimosDigitos).HasMaxLength(4);
            e.Property(c => c.Limite).HasColumnType("decimal(18,2)");
            e.Property(c => c.LimiteDisponivel).HasColumnType("decimal(18,2)");
            e.Property(c => c.Cor).HasMaxLength(7);
            e.HasIndex(c => c.UsuarioId);
            e.HasOne(c => c.Usuario)
                .WithMany(u => u.CartoesCredito)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Conta)
                .WithMany(ct => ct.CartoesCredito)
                .HasForeignKey(c => c.ContaId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<FaturaCartao>(e =>
        {
            e.ToTable("FaturasCartao", "FinanceApp");
            e.HasKey(f => f.Id);
            e.Property(f => f.ValorTotal).HasColumnType("decimal(18,2)");
            e.Property(f => f.ValorPago).HasColumnType("decimal(18,2)");
            e.Property(f => f.Status).HasMaxLength(15)
                .HasConversion<string>().IsRequired();
            e.HasIndex(f => f.UsuarioId);
            e.HasIndex(f => new { f.CartaoCreditoId, f.MesReferencia, f.AnoReferencia })
                .IsUnique();
            e.HasOne(f => f.CartaoCredito)
                .WithMany(c => c.Faturas)
                .HasForeignKey(f => f.CartaoCreditoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(f => f.Usuario)
                .WithMany()
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Transacao>(e =>
        {
            e.ToTable("Transacoes", "FinanceApp");
            e.HasKey(t => t.Id);
            e.Property(t => t.Valor).HasColumnType("decimal(18,2)");
            e.Property(t => t.Tipo).HasMaxLength(10)
                .HasConversion<string>().IsRequired();
            e.Property(t => t.Descricao).HasMaxLength(300);
            e.Property(t => t.Origem).HasMaxLength(10)
                .HasConversion<string>().IsRequired();
            e.Property(t => t.Status).HasMaxLength(15)
                .HasConversion<string>().IsRequired();
            e.Property(t => t.Observacoes).HasMaxLength(500);

            e.HasIndex(t => t.UsuarioId);
            e.HasIndex(t => new { t.UsuarioId, t.DataTransacao })
                .IsDescending(false, true);
            e.HasIndex(t => new { t.UsuarioId, t.CategoriaId });
            e.HasIndex(t => new { t.UsuarioId, t.ContaId });
            e.HasIndex(t => t.FaturaCartaoId);

            e.HasOne(t => t.Usuario)
                .WithMany(u => u.Transacoes)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.Categoria)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.CategoriaId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(t => t.Conta)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.ContaId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(t => t.FormaPagamento)
                .WithMany(f => f.Transacoes)
                .HasForeignKey(t => t.FormaPagamentoId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(t => t.CartaoCredito)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.CartaoCreditoId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(t => t.FaturaCartao)
                .WithMany(f => f.Transacoes)
                .HasForeignKey(t => t.FaturaCartaoId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(t => t.RegraRecorrencia)
                .WithMany(r => r.Transacoes)
                .HasForeignKey(t => t.RegraRecorrenciaId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(t => t.Parcelamento)
                .WithMany(p => p.Transacoes)
                .HasForeignKey(t => t.ParcelamentoId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Parcelamento>(e =>
        {
            e.ToTable("Parcelamentos", "FinanceApp");
            e.HasKey(p => p.Id);
            e.Property(p => p.Descricao).HasMaxLength(300).IsRequired();
            e.Property(p => p.ValorTotal).HasColumnType("decimal(18,2)");
            e.Property(p => p.ValorParcela).HasColumnType("decimal(18,2)");
            e.HasIndex(p => p.UsuarioId);
            e.HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.CartaoCredito)
                .WithMany(c => c.Parcelamentos)
                .HasForeignKey(p => p.CartaoCreditoId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<RegraRecorrencia>(e =>
        {
            e.ToTable("RegrasRecorrencia", "FinanceApp");
            e.HasKey(r => r.Id);
            e.Property(r => r.Descricao).HasMaxLength(300).IsRequired();
            e.Property(r => r.Valor).HasColumnType("decimal(18,2)");
            e.Property(r => r.Tipo).HasMaxLength(10)
                .HasConversion<string>().IsRequired();
            e.Property(r => r.Frequencia).HasMaxLength(15)
                .HasConversion<string>().IsRequired();
            e.HasOne(r => r.Usuario)
                .WithMany(u => u.RegrasRecorrencia)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Categoria)
                .WithMany()
                .HasForeignKey(r => r.CategoriaId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(r => r.Conta)
                .WithMany()
                .HasForeignKey(r => r.ContaId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(r => r.FormaPagamento)
                .WithMany()
                .HasForeignKey(r => r.FormaPagamentoId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Orcamento>(e =>
        {
            e.ToTable("Orcamentos", "FinanceApp");
            e.HasKey(o => o.Id);
            e.Property(o => o.ValorLimite).HasColumnType("decimal(18,2)");
            e.HasIndex(o => new { o.UsuarioId, o.CategoriaId, o.Mes, o.Ano })
                .IsUnique();
            e.HasOne(o => o.Usuario)
                .WithMany(u => u.Orcamentos)
                .HasForeignKey(o => o.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(o => o.Categoria)
                .WithMany()
                .HasForeignKey(o => o.CategoriaId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<MetaEconomia>(e =>
        {
            e.ToTable("MetasEconomia", "FinanceApp");
            e.HasKey(m => m.Id);
            e.Property(m => m.Titulo).HasMaxLength(150).IsRequired();
            e.Property(m => m.ValorAlvo).HasColumnType("decimal(18,2)");
            e.Property(m => m.ValorAtual).HasColumnType("decimal(18,2)");
            e.Property(m => m.Icone).HasMaxLength(50);
            e.Property(m => m.Cor).HasMaxLength(7);
            e.HasOne(m => m.Usuario)
                .WithMany(u => u.MetasEconomia)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LancamentoMeta>(e =>
        {
            e.ToTable("LancamentosMeta", "FinanceApp");
            e.HasKey(l => l.Id);
            e.Property(l => l.Valor).HasColumnType("decimal(18,2)");
            e.Property(l => l.Observacoes).HasMaxLength(300);
            e.HasOne(l => l.MetaEconomia)
                .WithMany(m => m.Lancamentos)
                .HasForeignKey(l => l.MetaEconomiaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TransferenciaConta>(e =>
        {
            e.ToTable("TransferenciasContas", "FinanceApp");
            e.HasKey(t => t.Id);
            e.Property(t => t.Valor).HasColumnType("decimal(18,2)");
            e.Property(t => t.Descricao).HasMaxLength(300);
            e.HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.ContaOrigem)
                .WithMany()
                .HasForeignKey(t => t.ContaOrigemId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(t => t.ContaDestino)
                .WithMany()
                .HasForeignKey(t => t.ContaDestinoId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Anexo>(e =>
        {
            e.ToTable("Anexos", "FinanceApp");
            e.HasKey(a => a.Id);
            e.Property(a => a.NomeArquivo).HasMaxLength(255).IsRequired();
            e.Property(a => a.UrlArquivo).HasMaxLength(500).IsRequired();
            e.Property(a => a.TipoArquivo).HasMaxLength(20).IsRequired();
            e.HasIndex(a => a.TransacaoId);
            e.HasOne(a => a.Transacao)
                .WithMany(t => t.Anexos)
                .HasForeignKey(a => a.TransacaoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<MensagemWhatsApp>(e =>
        {
            e.ToTable("MensagensWhatsApp", "FinanceApp");
            e.HasKey(m => m.Id);
            e.Property(m => m.NumeroTelefone).HasMaxLength(20).IsRequired();
            e.Property(m => m.MensagemOriginal).HasMaxLength(1000).IsRequired();
            e.Property(m => m.CategoriaIdentificada).HasMaxLength(80);
            e.Property(m => m.ValorIdentificado).HasColumnType("decimal(18,2)");
            e.Property(m => m.MensagemErro).HasMaxLength(500);
            e.HasIndex(m => m.NumeroTelefone);
            e.HasIndex(m => m.UsuarioId);
            e.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(m => m.Transacao)
                .WithMany()
                .HasForeignKey(m => m.TransacaoId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<ApelidoCategoria>(e =>
        {
            e.ToTable("ApelidosCategorias", "FinanceApp");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).UseIdentityColumn();
            e.Property(a => a.Apelido).HasMaxLength(80).IsRequired();
            e.HasIndex(a => new { a.Apelido, a.UsuarioId }).IsUnique();
            e.HasOne(a => a.Categoria)
                .WithMany(c => c.Apelidos)
                .HasForeignKey(a => a.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Notificacao>(e =>
        {
            e.ToTable("Notificacoes", "FinanceApp");
            e.HasKey(n => n.Id);
            e.Property(n => n.Titulo).HasMaxLength(150).IsRequired();
            e.Property(n => n.Mensagem).HasMaxLength(500).IsRequired();
            e.Property(n => n.Tipo).HasMaxLength(30)
                .HasConversion<string>().IsRequired();
            e.HasIndex(n => new { n.UsuarioId, n.Lida, n.CriadoEm })
                .IsDescending(false, false, true);
            e.HasOne(n => n.Usuario)
                .WithMany(u => u.Notificacoes)
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConfiguracaoUsuario>(e =>
        {
            e.ToTable("ConfiguracoesUsuario", "FinanceApp");
            e.HasKey(c => c.UsuarioId);
            e.Property(c => c.Moeda).HasMaxLength(5);
            e.Property(c => c.Idioma).HasMaxLength(5);
            e.HasOne(c => c.Usuario)
                .WithOne(u => u.Configuracoes)
                .HasForeignKey<ConfiguracaoUsuario>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LogAuditoria>(e =>
        {
            e.ToTable("LogsAuditoria", "FinanceApp");
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).UseIdentityColumn();
            e.Property(l => l.Acao).HasMaxLength(50).IsRequired();
            e.Property(l => l.TipoEntidade).HasMaxLength(50);
            e.Property(l => l.EntidadeId).HasMaxLength(50);
            e.Property(l => l.EnderecoIP).HasMaxLength(45);
            e.Property(l => l.Navegador).HasMaxLength(500);
            e.HasIndex(l => new { l.UsuarioId, l.CriadoEm })
                .IsDescending(false, true);
        });

        builder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Alimentacao", Icone = "utensils", Cor = "#FF6B6B", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 2, Nome = "Transporte", Icone = "car", Cor = "#4ECDC4", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 3, Nome = "Moradia", Icone = "home", Cor = "#45B7D1", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 4, Nome = "Saude", Icone = "heart-pulse", Cor = "#96CEB4", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 5, Nome = "Educacao", Icone = "graduation-cap", Cor = "#FFEAA7", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 6, Nome = "Lazer", Icone = "gamepad-2", Cor = "#DDA0DD", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 7, Nome = "Vestuario", Icone = "shirt", Cor = "#F0E68C", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 8, Nome = "Combustivel", Icone = "fuel", Cor = "#FFB347", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 9, Nome = "Assinaturas", Icone = "repeat", Cor = "#87CEEB", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 10, Nome = "Mercado", Icone = "shopping-cart", Cor = "#98D8C8", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 11, Nome = "Pets", Icone = "paw-print", Cor = "#C9B1FF", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 12, Nome = "Beleza", Icone = "sparkles", Cor = "#FFB6C1", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 13, Nome = "Cartao de Credito", Icone = "credit-card", Cor = "#FF4757", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 14, Nome = "Investimentos", Icone = "trending-down", Cor = "#E17055", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 15, Nome = "Outros", Icone = "ellipsis", Cor = "#B0BEC5", Tipo = TipoTransacao.DESPESA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 16, Nome = "Salario", Icone = "banknote", Cor = "#27AE60", Tipo = TipoTransacao.RECEITA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 17, Nome = "Freelance", Icone = "laptop", Cor = "#2ECC71", Tipo = TipoTransacao.RECEITA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 18, Nome = "Rendimentos", Icone = "trending-up", Cor = "#1ABC9C", Tipo = TipoTransacao.RECEITA, Padrao = true, UsuarioId = null },
            new Categoria { Id = 19, Nome = "Outros Ganhos", Icone = "plus-circle", Cor = "#3498DB", Tipo = TipoTransacao.RECEITA, Padrao = true, UsuarioId = null }
        );

        builder.Entity<FormaPagamento>().HasData(
            new FormaPagamento { Id = 1, Nome = "Dinheiro", Icone = "banknote" },
            new FormaPagamento { Id = 2, Nome = "Cartao de Credito", Icone = "credit-card" },
            new FormaPagamento { Id = 3, Nome = "Cartao de Debito", Icone = "credit-card" },
            new FormaPagamento { Id = 4, Nome = "PIX", Icone = "qr-code" },
            new FormaPagamento { Id = 5, Nome = "Boleto", Icone = "file-text" },
            new FormaPagamento { Id = 6, Nome = "Transferencia", Icone = "arrow-right-left" }
        );

        builder.Entity<ApelidoCategoria>().HasData(
            // Alimentacao
            new { Id = 1, CategoriaId = 1, Apelido = "comida", UsuarioId = (Guid?)null },
            new { Id = 2, CategoriaId = 1, Apelido = "almoco", UsuarioId = (Guid?)null },
            new { Id = 3, CategoriaId = 1, Apelido = "janta", UsuarioId = (Guid?)null },
            new { Id = 4, CategoriaId = 1, Apelido = "lanche", UsuarioId = (Guid?)null },
            new { Id = 5, CategoriaId = 1, Apelido = "ifood", UsuarioId = (Guid?)null },
            new { Id = 6, CategoriaId = 1, Apelido = "restaurante", UsuarioId = (Guid?)null },
            new { Id = 7, CategoriaId = 1, Apelido = "cafe", UsuarioId = (Guid?)null },
            new { Id = 8, CategoriaId = 1, Apelido = "padaria", UsuarioId = (Guid?)null },
            // Transporte
            new { Id = 9, CategoriaId = 2, Apelido = "uber", UsuarioId = (Guid?)null },
            new { Id = 10, CategoriaId = 2, Apelido = "onibus", UsuarioId = (Guid?)null },
            new { Id = 11, CategoriaId = 2, Apelido = "99", UsuarioId = (Guid?)null },
            new { Id = 12, CategoriaId = 2, Apelido = "taxi", UsuarioId = (Guid?)null },
            new { Id = 13, CategoriaId = 2, Apelido = "passagem", UsuarioId = (Guid?)null },
            new { Id = 14, CategoriaId = 2, Apelido = "metro", UsuarioId = (Guid?)null },
            // Moradia
            new { Id = 15, CategoriaId = 3, Apelido = "aluguel", UsuarioId = (Guid?)null },
            new { Id = 16, CategoriaId = 3, Apelido = "condominio", UsuarioId = (Guid?)null },
            new { Id = 17, CategoriaId = 3, Apelido = "luz", UsuarioId = (Guid?)null },
            new { Id = 18, CategoriaId = 3, Apelido = "agua", UsuarioId = (Guid?)null },
            new { Id = 19, CategoriaId = 3, Apelido = "internet", UsuarioId = (Guid?)null },
            new { Id = 20, CategoriaId = 3, Apelido = "energia", UsuarioId = (Guid?)null },
            new { Id = 21, CategoriaId = 3, Apelido = "gas", UsuarioId = (Guid?)null },
            new { Id = 22, CategoriaId = 3, Apelido = "iptu", UsuarioId = (Guid?)null },
            // Saude
            new { Id = 23, CategoriaId = 4, Apelido = "remedio", UsuarioId = (Guid?)null },
            new { Id = 24, CategoriaId = 4, Apelido = "farmacia", UsuarioId = (Guid?)null },
            new { Id = 25, CategoriaId = 4, Apelido = "medico", UsuarioId = (Guid?)null },
            new { Id = 26, CategoriaId = 4, Apelido = "consulta", UsuarioId = (Guid?)null },
            new { Id = 27, CategoriaId = 4, Apelido = "dentista", UsuarioId = (Guid?)null },
            new { Id = 28, CategoriaId = 4, Apelido = "exame", UsuarioId = (Guid?)null },
            // Educacao
            new { Id = 29, CategoriaId = 5, Apelido = "curso", UsuarioId = (Guid?)null },
            new { Id = 30, CategoriaId = 5, Apelido = "livro", UsuarioId = (Guid?)null },
            new { Id = 31, CategoriaId = 5, Apelido = "faculdade", UsuarioId = (Guid?)null },
            new { Id = 32, CategoriaId = 5, Apelido = "escola", UsuarioId = (Guid?)null },
            // Lazer
            new { Id = 33, CategoriaId = 6, Apelido = "cinema", UsuarioId = (Guid?)null },
            new { Id = 34, CategoriaId = 6, Apelido = "netflix", UsuarioId = (Guid?)null },
            new { Id = 35, CategoriaId = 6, Apelido = "spotify", UsuarioId = (Guid?)null },
            new { Id = 36, CategoriaId = 6, Apelido = "jogo", UsuarioId = (Guid?)null },
            new { Id = 37, CategoriaId = 6, Apelido = "bar", UsuarioId = (Guid?)null },
            new { Id = 38, CategoriaId = 6, Apelido = "festa", UsuarioId = (Guid?)null },
            new { Id = 39, CategoriaId = 6, Apelido = "viagem", UsuarioId = (Guid?)null },
            // Vestuario
            new { Id = 40, CategoriaId = 7, Apelido = "roupa", UsuarioId = (Guid?)null },
            new { Id = 41, CategoriaId = 7, Apelido = "calcado", UsuarioId = (Guid?)null },
            new { Id = 42, CategoriaId = 7, Apelido = "tenis", UsuarioId = (Guid?)null },
            new { Id = 43, CategoriaId = 7, Apelido = "shein", UsuarioId = (Guid?)null },
            // Combustivel
            new { Id = 44, CategoriaId = 8, Apelido = "gasolina", UsuarioId = (Guid?)null },
            new { Id = 45, CategoriaId = 8, Apelido = "etanol", UsuarioId = (Guid?)null },
            new { Id = 46, CategoriaId = 8, Apelido = "combustivel", UsuarioId = (Guid?)null },
            new { Id = 47, CategoriaId = 8, Apelido = "posto", UsuarioId = (Guid?)null },
            new { Id = 48, CategoriaId = 8, Apelido = "diesel", UsuarioId = (Guid?)null },
            // Assinaturas
            new { Id = 49, CategoriaId = 9, Apelido = "assinatura", UsuarioId = (Guid?)null },
            new { Id = 50, CategoriaId = 9, Apelido = "mensalidade", UsuarioId = (Guid?)null },
            new { Id = 51, CategoriaId = 9, Apelido = "academia", UsuarioId = (Guid?)null },
            new { Id = 52, CategoriaId = 9, Apelido = "streaming", UsuarioId = (Guid?)null },
            // Mercado
            new { Id = 53, CategoriaId = 10, Apelido = "mercado", UsuarioId = (Guid?)null },
            new { Id = 54, CategoriaId = 10, Apelido = "supermercado", UsuarioId = (Guid?)null },
            new { Id = 55, CategoriaId = 10, Apelido = "feira", UsuarioId = (Guid?)null },
            new { Id = 56, CategoriaId = 10, Apelido = "hortifruti", UsuarioId = (Guid?)null },
            new { Id = 57, CategoriaId = 10, Apelido = "acougue", UsuarioId = (Guid?)null },
            // Pets
            new { Id = 58, CategoriaId = 11, Apelido = "racao", UsuarioId = (Guid?)null },
            new { Id = 59, CategoriaId = 11, Apelido = "veterinario", UsuarioId = (Guid?)null },
            new { Id = 60, CategoriaId = 11, Apelido = "petshop", UsuarioId = (Guid?)null },
            // Beleza
            new { Id = 61, CategoriaId = 12, Apelido = "cabelo", UsuarioId = (Guid?)null },
            new { Id = 62, CategoriaId = 12, Apelido = "unha", UsuarioId = (Guid?)null },
            new { Id = 63, CategoriaId = 12, Apelido = "salao", UsuarioId = (Guid?)null },
            new { Id = 64, CategoriaId = 12, Apelido = "barbearia", UsuarioId = (Guid?)null },
            // Cartao de Credito
            new { Id = 65, CategoriaId = 13, Apelido = "fatura", UsuarioId = (Guid?)null },
            new { Id = 66, CategoriaId = 13, Apelido = "cartao", UsuarioId = (Guid?)null },
            // Investimentos
            new { Id = 67, CategoriaId = 14, Apelido = "acao", UsuarioId = (Guid?)null },
            new { Id = 68, CategoriaId = 14, Apelido = "acoes", UsuarioId = (Guid?)null },
            new { Id = 69, CategoriaId = 14, Apelido = "fii", UsuarioId = (Guid?)null },
            new { Id = 70, CategoriaId = 14, Apelido = "cdb", UsuarioId = (Guid?)null },
            new { Id = 71, CategoriaId = 14, Apelido = "tesouro", UsuarioId = (Guid?)null },
            new { Id = 72, CategoriaId = 14, Apelido = "cripto", UsuarioId = (Guid?)null },
            new { Id = 73, CategoriaId = 14, Apelido = "bitcoin", UsuarioId = (Guid?)null },
            new { Id = 74, CategoriaId = 14, Apelido = "poupanca", UsuarioId = (Guid?)null },
            // Outros
            new { Id = 75, CategoriaId = 15, Apelido = "presente", UsuarioId = (Guid?)null },
            new { Id = 76, CategoriaId = 15, Apelido = "doacao", UsuarioId = (Guid?)null },
            new { Id = 77, CategoriaId = 15, Apelido = "imposto", UsuarioId = (Guid?)null },
            new { Id = 78, CategoriaId = 15, Apelido = "multa", UsuarioId = (Guid?)null }
        );
    }
}