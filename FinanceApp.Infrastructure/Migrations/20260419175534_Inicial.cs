using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "FinanceApp");

            migrationBuilder.CreateTable(
                name: "FormasPagamento",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormasPagamento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Acao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoEntidade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntidadeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Detalhes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnderecoIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Navegador = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TelefoneWhatsApp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "FinanceApp",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Icone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Padrao = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CodigosVerificacao",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Finalidade = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosVerificacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigosVerificacao_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesUsuario",
                schema: "FinanceApp",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Moeda = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DiaInicioMes = table.Column<int>(type: "int", nullable: false),
                    WhatsAppAtivado = table.Column<bool>(type: "bit", nullable: false),
                    NotificacoesPush = table.Column<bool>(type: "bit", nullable: false),
                    AlertasOrcamento = table.Column<bool>(type: "bit", nullable: false),
                    AlertasFatura = table.Column<bool>(type: "bit", nullable: false),
                    ModoEscuro = table.Column<bool>(type: "bit", nullable: false),
                    Idioma = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesUsuario", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_ConfiguracoesUsuario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contas",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoConta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Banco = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Cor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Icone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoAtual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    Principal = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetasEconomia",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ValorAlvo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorAtual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataLimite = table.Column<DateOnly>(type: "date", nullable: true),
                    Icone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Concluida = table.Column<bool>(type: "bit", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetasEconomia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetasEconomia_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Lida = table.Column<bool>(type: "bit", nullable: false),
                    EntidadeRelacionadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokensAtualizacao",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevogadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubstituidoPor = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensAtualizacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokensAtualizacao_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioClaims",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioClaims_Usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioLogins",
                schema: "FinanceApp",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UsuarioLogins_Usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                schema: "FinanceApp",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "FinanceApp",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioTokens",
                schema: "FinanceApp",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UsuarioTokens_Usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApelidosCategorias",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Apelido = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApelidosCategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApelidosCategorias_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalSchema: "FinanceApp",
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApelidosCategorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orcamentos",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    ValorLimite = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    PercentualAlerta = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orcamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orcamentos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalSchema: "FinanceApp",
                        principalTable: "Categorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orcamentos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartoesCredito",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Bandeira = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UltimosDigitos = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Limite = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LimiteDisponivel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiaFechamento = table.Column<int>(type: "int", nullable: false),
                    DiaVencimento = table.Column<int>(type: "int", nullable: false),
                    Cor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartoesCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartoesCredito_Contas_ContaId",
                        column: x => x.ContaId,
                        principalSchema: "FinanceApp",
                        principalTable: "Contas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CartoesCredito_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegrasRecorrencia",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    ContaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormaPagamentoId = table.Column<int>(type: "int", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Frequencia = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DiaMes = table.Column<int>(type: "int", nullable: true),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    UltimaGeracaoEm = table.Column<DateOnly>(type: "date", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegrasRecorrencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegrasRecorrencia_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalSchema: "FinanceApp",
                        principalTable: "Categorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegrasRecorrencia_Contas_ContaId",
                        column: x => x.ContaId,
                        principalSchema: "FinanceApp",
                        principalTable: "Contas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegrasRecorrencia_FormasPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalSchema: "FinanceApp",
                        principalTable: "FormasPagamento",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegrasRecorrencia_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferenciasContas",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContaOrigemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContaDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DataTransferencia = table.Column<DateOnly>(type: "date", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferenciasContas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferenciasContas_Contas_ContaDestinoId",
                        column: x => x.ContaDestinoId,
                        principalSchema: "FinanceApp",
                        principalTable: "Contas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransferenciasContas_Contas_ContaOrigemId",
                        column: x => x.ContaOrigemId,
                        principalSchema: "FinanceApp",
                        principalTable: "Contas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransferenciasContas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LancamentosMeta",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetaEconomiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentosMeta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentosMeta_MetasEconomia_MetaEconomiaId",
                        column: x => x.MetaEconomiaId,
                        principalSchema: "FinanceApp",
                        principalTable: "MetasEconomia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaturasCartao",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartaoCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MesReferencia = table.Column<int>(type: "int", nullable: false),
                    AnoReferencia = table.Column<int>(type: "int", nullable: false),
                    DataFechamento = table.Column<DateOnly>(type: "date", nullable: false),
                    DataVencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorPago = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturasCartao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaturasCartao_CartoesCredito_CartaoCreditoId",
                        column: x => x.CartaoCreditoId,
                        principalSchema: "FinanceApp",
                        principalTable: "CartoesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FaturasCartao_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Parcelamentos",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartaoCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorParcela = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalParcelas = table.Column<int>(type: "int", nullable: false),
                    ParcelasPagas = table.Column<int>(type: "int", nullable: false),
                    DataPrimeiraParcela = table.Column<DateOnly>(type: "date", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcelamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parcelamentos_CartoesCredito_CartaoCreditoId",
                        column: x => x.CartaoCreditoId,
                        principalSchema: "FinanceApp",
                        principalTable: "CartoesCredito",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Parcelamentos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalSchema: "FinanceApp",
                        principalTable: "Categorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Parcelamentos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transacoes",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    ContaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormaPagamentoId = table.Column<int>(type: "int", nullable: true),
                    CartaoCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FaturaCartaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DataTransacao = table.Column<DateOnly>(type: "date", nullable: false),
                    Origem = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Recorrente = table.Column<bool>(type: "bit", nullable: false),
                    RegraRecorrenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParcelamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumeroParcela = table.Column<int>(type: "int", nullable: true),
                    TotalParcelas = table.Column<int>(type: "int", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacoes_CartoesCredito_CartaoCreditoId",
                        column: x => x.CartaoCreditoId,
                        principalSchema: "FinanceApp",
                        principalTable: "CartoesCredito",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalSchema: "FinanceApp",
                        principalTable: "Categorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_Contas_ContaId",
                        column: x => x.ContaId,
                        principalSchema: "FinanceApp",
                        principalTable: "Contas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_FaturasCartao_FaturaCartaoId",
                        column: x => x.FaturaCartaoId,
                        principalSchema: "FinanceApp",
                        principalTable: "FaturasCartao",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_FormasPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalSchema: "FinanceApp",
                        principalTable: "FormasPagamento",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_Parcelamentos_ParcelamentoId",
                        column: x => x.ParcelamentoId,
                        principalSchema: "FinanceApp",
                        principalTable: "Parcelamentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_RegrasRecorrencia_RegraRecorrenciaId",
                        column: x => x.RegraRecorrenciaId,
                        principalSchema: "FinanceApp",
                        principalTable: "RegrasRecorrencia",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anexos",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeArquivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UrlArquivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoArquivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anexos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anexos_Transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalSchema: "FinanceApp",
                        principalTable: "Transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Anexos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MensagensWhatsApp",
                schema: "FinanceApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumeroTelefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MensagemOriginal = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CategoriaIdentificada = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ValorIdentificado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProcessadoComSucesso = table.Column<bool>(type: "bit", nullable: false),
                    TransacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MensagemErro = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagensWhatsApp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensagensWhatsApp_Transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalSchema: "FinanceApp",
                        principalTable: "Transacoes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MensagensWhatsApp_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "FinanceApp",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "FinanceApp",
                table: "Categorias",
                columns: new[] { "Id", "Ativo", "Cor", "CriadoEm", "Icone", "Nome", "Padrao", "Tipo", "UsuarioId" },
                values: new object[,]
                {
                    { 1, true, "#FF6B6B", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1082), "utensils", "Alimentacao", true, "DESPESA", null },
                    { 2, true, "#4ECDC4", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1097), "car", "Transporte", true, "DESPESA", null },
                    { 3, true, "#45B7D1", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1102), "home", "Moradia", true, "DESPESA", null },
                    { 4, true, "#96CEB4", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1108), "heart-pulse", "Saude", true, "DESPESA", null },
                    { 5, true, "#FFEAA7", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1112), "graduation-cap", "Educacao", true, "DESPESA", null },
                    { 6, true, "#DDA0DD", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1115), "gamepad-2", "Lazer", true, "DESPESA", null },
                    { 7, true, "#F0E68C", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1119), "shirt", "Vestuario", true, "DESPESA", null },
                    { 8, true, "#FFB347", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1123), "fuel", "Combustivel", true, "DESPESA", null },
                    { 9, true, "#87CEEB", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1127), "repeat", "Assinaturas", true, "DESPESA", null },
                    { 10, true, "#98D8C8", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1130), "shopping-cart", "Mercado", true, "DESPESA", null },
                    { 11, true, "#C9B1FF", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1132), "paw-print", "Pets", true, "DESPESA", null },
                    { 12, true, "#FFB6C1", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1135), "sparkles", "Beleza", true, "DESPESA", null },
                    { 13, true, "#FF4757", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1138), "credit-card", "Cartao de Credito", true, "DESPESA", null },
                    { 14, true, "#E17055", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1142), "trending-down", "Investimentos", true, "DESPESA", null },
                    { 15, true, "#B0BEC5", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1145), "ellipsis", "Outros", true, "DESPESA", null },
                    { 16, true, "#27AE60", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1152), "banknote", "Salario", true, "RECEITA", null },
                    { 17, true, "#2ECC71", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1155), "laptop", "Freelance", true, "RECEITA", null },
                    { 18, true, "#1ABC9C", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1159), "trending-up", "Rendimentos", true, "RECEITA", null },
                    { 19, true, "#3498DB", new DateTime(2026, 4, 19, 17, 55, 31, 971, DateTimeKind.Utc).AddTicks(1162), "plus-circle", "Outros Ganhos", true, "RECEITA", null }
                });

            migrationBuilder.InsertData(
                schema: "FinanceApp",
                table: "FormasPagamento",
                columns: new[] { "Id", "Icone", "Nome" },
                values: new object[,]
                {
                    { 1, "banknote", "Dinheiro" },
                    { 2, "credit-card", "Cartao de Credito" },
                    { 3, "credit-card", "Cartao de Debito" },
                    { 4, "qr-code", "PIX" },
                    { 5, "file-text", "Boleto" },
                    { 6, "arrow-right-left", "Transferencia" }
                });

            migrationBuilder.InsertData(
                schema: "FinanceApp",
                table: "ApelidosCategorias",
                columns: new[] { "Id", "Apelido", "CategoriaId", "UsuarioId" },
                values: new object[,]
                {
                    { 1, "comida", 1, null },
                    { 2, "almoco", 1, null },
                    { 3, "janta", 1, null },
                    { 4, "lanche", 1, null },
                    { 5, "ifood", 1, null },
                    { 6, "restaurante", 1, null },
                    { 7, "cafe", 1, null },
                    { 8, "padaria", 1, null },
                    { 9, "uber", 2, null },
                    { 10, "onibus", 2, null },
                    { 11, "99", 2, null },
                    { 12, "taxi", 2, null },
                    { 13, "passagem", 2, null },
                    { 14, "metro", 2, null },
                    { 15, "aluguel", 3, null },
                    { 16, "condominio", 3, null },
                    { 17, "luz", 3, null },
                    { 18, "agua", 3, null },
                    { 19, "internet", 3, null },
                    { 20, "energia", 3, null },
                    { 21, "gas", 3, null },
                    { 22, "iptu", 3, null },
                    { 23, "remedio", 4, null },
                    { 24, "farmacia", 4, null },
                    { 25, "medico", 4, null },
                    { 26, "consulta", 4, null },
                    { 27, "dentista", 4, null },
                    { 28, "exame", 4, null },
                    { 29, "curso", 5, null },
                    { 30, "livro", 5, null },
                    { 31, "faculdade", 5, null },
                    { 32, "escola", 5, null },
                    { 33, "cinema", 6, null },
                    { 34, "netflix", 6, null },
                    { 35, "spotify", 6, null },
                    { 36, "jogo", 6, null },
                    { 37, "bar", 6, null },
                    { 38, "festa", 6, null },
                    { 39, "viagem", 6, null },
                    { 40, "roupa", 7, null },
                    { 41, "calcado", 7, null },
                    { 42, "tenis", 7, null },
                    { 43, "shein", 7, null },
                    { 44, "gasolina", 8, null },
                    { 45, "etanol", 8, null },
                    { 46, "combustivel", 8, null },
                    { 47, "posto", 8, null },
                    { 48, "diesel", 8, null },
                    { 49, "assinatura", 9, null },
                    { 50, "mensalidade", 9, null },
                    { 51, "academia", 9, null },
                    { 52, "streaming", 9, null },
                    { 53, "mercado", 10, null },
                    { 54, "supermercado", 10, null },
                    { 55, "feira", 10, null },
                    { 56, "hortifruti", 10, null },
                    { 57, "acougue", 10, null },
                    { 58, "racao", 11, null },
                    { 59, "veterinario", 11, null },
                    { 60, "petshop", 11, null },
                    { 61, "cabelo", 12, null },
                    { 62, "unha", 12, null },
                    { 63, "salao", 12, null },
                    { 64, "barbearia", 12, null },
                    { 65, "fatura", 13, null },
                    { 66, "cartao", 13, null },
                    { 67, "acao", 14, null },
                    { 68, "acoes", 14, null },
                    { 69, "fii", 14, null },
                    { 70, "cdb", 14, null },
                    { 71, "tesouro", 14, null },
                    { 72, "cripto", 14, null },
                    { 73, "bitcoin", 14, null },
                    { 74, "poupanca", 14, null },
                    { 75, "presente", 15, null },
                    { 76, "doacao", 15, null },
                    { 77, "imposto", 15, null },
                    { 78, "multa", 15, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anexos_TransacaoId",
                schema: "FinanceApp",
                table: "Anexos",
                column: "TransacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anexos_UsuarioId",
                schema: "FinanceApp",
                table: "Anexos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ApelidosCategorias_Apelido_UsuarioId",
                schema: "FinanceApp",
                table: "ApelidosCategorias",
                columns: new[] { "Apelido", "UsuarioId" },
                unique: true,
                filter: "[UsuarioId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApelidosCategorias_CategoriaId",
                schema: "FinanceApp",
                table: "ApelidosCategorias",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ApelidosCategorias_UsuarioId",
                schema: "FinanceApp",
                table: "ApelidosCategorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CartoesCredito_ContaId",
                schema: "FinanceApp",
                table: "CartoesCredito",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_CartoesCredito_UsuarioId",
                schema: "FinanceApp",
                table: "CartoesCredito",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_UsuarioId",
                schema: "FinanceApp",
                table: "Categorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVerificacao_UsuarioId",
                schema: "FinanceApp",
                table: "CodigosVerificacao",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Contas_UsuarioId",
                schema: "FinanceApp",
                table: "Contas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturasCartao_CartaoCreditoId_MesReferencia_AnoReferencia",
                schema: "FinanceApp",
                table: "FaturasCartao",
                columns: new[] { "CartaoCreditoId", "MesReferencia", "AnoReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaturasCartao_UsuarioId",
                schema: "FinanceApp",
                table: "FaturasCartao",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosMeta_MetaEconomiaId",
                schema: "FinanceApp",
                table: "LancamentosMeta",
                column: "MetaEconomiaId");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_UsuarioId_CriadoEm",
                schema: "FinanceApp",
                table: "LogsAuditoria",
                columns: new[] { "UsuarioId", "CriadoEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_MensagensWhatsApp_NumeroTelefone",
                schema: "FinanceApp",
                table: "MensagensWhatsApp",
                column: "NumeroTelefone");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensWhatsApp_TransacaoId",
                schema: "FinanceApp",
                table: "MensagensWhatsApp",
                column: "TransacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensWhatsApp_UsuarioId",
                schema: "FinanceApp",
                table: "MensagensWhatsApp",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MetasEconomia_UsuarioId",
                schema: "FinanceApp",
                table: "MetasEconomia",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_UsuarioId_Lida_CriadoEm",
                schema: "FinanceApp",
                table: "Notificacoes",
                columns: new[] { "UsuarioId", "Lida", "CriadoEm" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Orcamentos_CategoriaId",
                schema: "FinanceApp",
                table: "Orcamentos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Orcamentos_UsuarioId_CategoriaId_Mes_Ano",
                schema: "FinanceApp",
                table: "Orcamentos",
                columns: new[] { "UsuarioId", "CategoriaId", "Mes", "Ano" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parcelamentos_CartaoCreditoId",
                schema: "FinanceApp",
                table: "Parcelamentos",
                column: "CartaoCreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_Parcelamentos_CategoriaId",
                schema: "FinanceApp",
                table: "Parcelamentos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Parcelamentos_UsuarioId",
                schema: "FinanceApp",
                table: "Parcelamentos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegrasRecorrencia_CategoriaId",
                schema: "FinanceApp",
                table: "RegrasRecorrencia",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegrasRecorrencia_ContaId",
                schema: "FinanceApp",
                table: "RegrasRecorrencia",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegrasRecorrencia_FormaPagamentoId",
                schema: "FinanceApp",
                table: "RegrasRecorrencia",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegrasRecorrencia_UsuarioId",
                schema: "FinanceApp",
                table: "RegrasRecorrencia",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                schema: "FinanceApp",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "FinanceApp",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TokensAtualizacao_Token",
                schema: "FinanceApp",
                table: "TokensAtualizacao",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_TokensAtualizacao_UsuarioId",
                schema: "FinanceApp",
                table: "TokensAtualizacao",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_CartaoCreditoId",
                schema: "FinanceApp",
                table: "Transacoes",
                column: "CartaoCreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_CategoriaId",
                schema: "FinanceApp",
                table: "Transacoes",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId",
                schema: "FinanceApp",
                table: "Transacoes",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_FaturaCartaoId",
                schema: "FinanceApp",
                table: "Transacoes",
                column: "FaturaCartaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_FormaPagamentoId",
                schema: "FinanceApp",
                table: "Transacoes",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ParcelamentoId",
                schema: "FinanceApp",
                table: "Transacoes",
                column: "ParcelamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_RegraRecorrenciaId",
                schema: "FinanceApp",
                table: "Transacoes",
                column: "RegraRecorrenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_UsuarioId",
                schema: "FinanceApp",
                table: "Transacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_UsuarioId_CategoriaId",
                schema: "FinanceApp",
                table: "Transacoes",
                columns: new[] { "UsuarioId", "CategoriaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_UsuarioId_ContaId",
                schema: "FinanceApp",
                table: "Transacoes",
                columns: new[] { "UsuarioId", "ContaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_UsuarioId_DataTransacao",
                schema: "FinanceApp",
                table: "Transacoes",
                columns: new[] { "UsuarioId", "DataTransacao" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasContas_ContaDestinoId",
                schema: "FinanceApp",
                table: "TransferenciasContas",
                column: "ContaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasContas_ContaOrigemId",
                schema: "FinanceApp",
                table: "TransferenciasContas",
                column: "ContaOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasContas_UsuarioId",
                schema: "FinanceApp",
                table: "TransferenciasContas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioClaims_UserId",
                schema: "FinanceApp",
                table: "UsuarioClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioLogins_UserId",
                schema: "FinanceApp",
                table: "UsuarioLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_RoleId",
                schema: "FinanceApp",
                table: "UsuarioRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "FinanceApp",
                table: "Usuarios",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TelefoneWhatsApp",
                schema: "FinanceApp",
                table: "Usuarios",
                column: "TelefoneWhatsApp",
                unique: true,
                filter: "[TelefoneWhatsApp] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "FinanceApp",
                table: "Usuarios",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anexos",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "ApelidosCategorias",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "CodigosVerificacao",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "ConfiguracoesUsuario",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "LancamentosMeta",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "LogsAuditoria",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "MensagensWhatsApp",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "Notificacoes",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "Orcamentos",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "TokensAtualizacao",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "TransferenciasContas",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "UsuarioClaims",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "UsuarioLogins",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "UsuarioRoles",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "UsuarioTokens",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "MetasEconomia",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "Transacoes",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "FaturasCartao",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "Parcelamentos",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "RegrasRecorrencia",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "CartoesCredito",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "Categorias",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "FormasPagamento",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "Contas",
                schema: "FinanceApp");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "FinanceApp");
        }
    }
}
