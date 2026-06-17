# Data Model: app-Finance v2.0

**Date**: 2026-06-11 | **Plan**: [plan.md](plan.md)

---

## Diagrama de Relacionamentos

```
Usuario (1) ──────────────────────────────────── (N) Transacao
Usuario (1) ──────────────────────────────────── (N) Conta
Usuario (1) ──────────────────────────────────── (N) CartaoCredito
Usuario (1) ──────────────────────────────────── (N) Orcamento
Usuario (1) ──────────────────────────────────── (N) MetaEconomia
Usuario (1) ──────────────────────────────────── (N) Notificacao
Usuario (1) ──────────────────────────────────── (N) TransferenciaConta
Usuario (1) ──────────────────────────────────── (1) ConfiguracaoUsuario
Usuario (1) ──────────────────────────────────── (N) TokenAtualizacao
Usuario (1) ──────────────────────────────────── (N) CodigoVerificacao

Conta      (1) ──────────────────────────────── (N) Transacao
Conta      (1) ──────────────────────────────── (N) CartaoCredito [conta pagamento]
Conta      (1) ──────────────────────────────── (N) TransferenciaConta [origem]
Conta      (1) ──────────────────────────────── (N) TransferenciaConta [destino]

CartaoCredito (1) ──────────────────────────── (N) FaturaCartao
CartaoCredito (1) ──────────────────────────── (N) Transacao [compras no cartão]

FaturaCartao (1) ──────────────────────────── (N) Transacao [parcelas da fatura]

Categoria  (1) ──────────────────────────────── (N) Transacao
Categoria  (1) ──────────────────────────────── (N) Orcamento

TransferenciaConta (1) ────────────────────── (2) Transacao [par origem+destino]

Parcelamento (1) ────────────────────────────── (N) Transacao [parcelas]

MetaEconomia (1) ───────────────────────────── (N) LancamentoMeta [aportes]
```

---

## Entidades

### Usuario
```
Id              int             PK, IDENTITY
NomeCompleto    string(200)     NOT NULL
Email           string(256)     NOT NULL, UNIQUE
PasswordHash    string          NOT NULL
TelefoneWhatsApp string(20)?    NULL
FotoUrl         string(500)?    NULL
Ativo           bool            DEFAULT true
CriadoEm       DateTime        DEFAULT GETUTCDATE()
```

**Validações**: Email único no sistema. NomeCompleto min 2 chars.

---

### Conta *(soft delete)*
```
Id              int             PK, IDENTITY
UsuarioId       int             FK → Usuario, RESTRICT
Nome            string(100)     NOT NULL
TipoConta       enum            CORRENTE|POUPANCA|CARTEIRA|INVESTIMENTOS
Banco           string(100)?    NULL
SaldoInicial    decimal(18,2)   NOT NULL, DEFAULT 0
SaldoAtual      decimal(18,2)   NOT NULL, DEFAULT 0  [calculado, atualizado via service]
Cor             string(7)       NOT NULL [hex color]
Principal       bool            DEFAULT false
IsDeleted       bool            DEFAULT false         [NOVO]
DeletedAt       DateTime?       NULL                  [NOVO]
CriadaEm       DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- `SaldoAtual = SaldoInicial + SUM(receitas EFETIVADAS) - SUM(despesas EFETIVADAS)`
- Apenas uma conta por usuário pode ter `Principal = true`
- Exclusão bloqueada se houver `CartaoCredito` com `IsDeleted = false` vinculado
- Soft delete: `IsDeleted = true`, `DeletedAt = now`

---

### CartaoCredito *(soft delete)*
```
Id                  int             PK, IDENTITY
UsuarioId           int             FK → Usuario, RESTRICT
ContaId             int             FK → Conta, RESTRICT [conta para débito das faturas]
Nome                string(100)     NOT NULL
Bandeira            string(50)?     NULL [Visa, Mastercard, etc.]
LimiteTotal         decimal(18,2)   NOT NULL, > 0
LimiteDisponivel    decimal(18,2)   NOT NULL [calculado = LimiteTotal - SUM(faturas não PAGA)]
DiaFechamento       int             NOT NULL [1-28]
DiaVencimento       int             NOT NULL [1-28]
IsDeleted           bool            DEFAULT false
DeletedAt           DateTime?       NULL
CriadoEm           DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- `LimiteDisponivel = LimiteTotal - SUM(FaturasCartao.ValorTotal WHERE Status != PAGA)`
- Exclusão bloqueada se existir `FaturaCartao` com `Status IN (ABERTA, FECHADA)`
- `DiaFechamento` e `DiaVencimento` entre 1 e 28 (evitar problemas com meses curtos)

---

### FaturaCartao
```
Id                  int             PK, IDENTITY
CartaoCreditoId     int             FK → CartaoCredito, RESTRICT
MesReferencia       int             NOT NULL [1-12]
AnoReferencia       int             NOT NULL [>= 2020]
ValorTotal          decimal(18,2)   NOT NULL DEFAULT 0 [soma das transações da fatura]
Status              enum            ABERTA|FECHADA|PAGA
DataVencimento      DateTime?       NULL [calculada: ano+mês+DiaVencimento do cartão]
DataPagamento       DateTime?       NULL [NOVO — preenchida ao marcar como PAGA]
CriadaEm           DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- Única por `(CartaoCreditoId, MesReferencia, AnoReferencia)`
- Auto-criada quando primeira transação do cartão é alocada em um mês
- `Status FECHADA` automático quando hoje >= DiaFechamento (verificado no carregamento)
- `Status PAGA` somente via ação manual do usuário
- Pagamento: cria transação DESPESA na `Conta` vinculada ao cartão + atualiza `LimiteDisponivel`
- Não pode ser paga duas vezes (`Status = PAGA` bloqueia)

---

### Transacao
```
Id                  int             PK, IDENTITY
UsuarioId           int             FK → Usuario, RESTRICT
CategoriaId         int             FK → Categoria, RESTRICT
ContaId             int?            FK → Conta, RESTRICT [null se apenas cartão]
CartaoCreditoId     int?            FK → CartaoCredito, RESTRICT [null se não for cartão]
FaturaCartaoId      int?            FK → FaturaCartao, RESTRICT [null se não for cartão]
ParcelamentoId      int?            FK → Parcelamento, SET NULL
TransferenciaContaId int?           FK → TransferenciaConta, SET NULL [NOVO]
Valor               decimal(18,2)   NOT NULL, > 0
Descricao           string(500)?    NULL
Tipo                enum            DESPESA|RECEITA
Status              enum            PENDENTE|EFETIVADA|VENCIDA|CANCELADA
OrigemTransacao     enum            APP|WHATSAPP
DataTransacao       DateTime        NOT NULL
NumeroParcela       int?            NULL [ex: 3 de 8]
FormaPagamentoId    int?            FK → FormaPagamento [apenas DESPESA]
CriadaEm           DateTime        DEFAULT GETUTCDATE()
AtualizadaEm       DateTime        DEFAULT GETUTCDATE()
```

**Máquina de estados**:
```
PENDENTE ──[data vencida, automático]──► VENCIDA
PENDENTE ──[usuário confirma pagamento]─► EFETIVADA → afeta SaldoAtual
VENCIDA  ──[usuário confirma pagamento]─► EFETIVADA → afeta SaldoAtual
EFETIVADA──[usuário cancela]───────────► (delete físico) + reverte SaldoAtual
```

**Regras**:
- Transações de transferência: `TransferenciaContaId IS NOT NULL`, excluídas dos totais de dashboard
- Transações parceladas: `ParcelamentoId IS NOT NULL` — não editáveis individualmente
- `CANCELADA` como status intermediário: o sistema exibe confirmação, depois deleta fisicamente
- Apenas transações `EFETIVADA` afetam `SaldoAtual` da conta

---

### Parcelamento
```
Id                  int             PK, IDENTITY
UsuarioId           int             FK → Usuario, RESTRICT
TotalParcelas       int             NOT NULL [2-210]
ValorTotal          decimal(18,2)   NOT NULL
ValorParcela        decimal(18,2)   NOT NULL [ValorTotal / TotalParcelas]
ParcelasPagas       int             NOT NULL DEFAULT 0
CriadoEm           DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- Cancelamento de parcelamento: cancela todas as `Transacao` vinculadas com status != EFETIVADA
- `ParcelasPagas` incrementa quando uma parcela muda para EFETIVADA

---

### TransferenciaConta
```
Id                  int             PK, IDENTITY
UsuarioId           int             FK → Usuario, CASCADE
ContaOrigemId       int             FK → Conta, RESTRICT
ContaDestinoId      int             FK → Conta, RESTRICT
Valor               decimal(18,2)   NOT NULL, > 0
DataTransferencia   DateTime        NOT NULL
Descricao           string(500)?    NULL
CriadaEm           DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- `ContaOrigemId != ContaDestinoId` (validação no serviço)
- Cria atomicamente: Transacao DESPESA em ContaOrigem + Transacao RECEITA em ContaDestino
- Cancelamento: cancela ambas as transações atomicamente

---

### Categoria
```
Id              int             PK, IDENTITY
Nome            string(100)     NOT NULL
Icone           string(50)      NOT NULL [nome do ícone Ionicons]
Cor             string(7)       NOT NULL [hex color]
Tipo            enum            DESPESA|RECEITA
Padrao          bool            DEFAULT false [categorias do sistema]
UsuarioId       int?            FK → Usuario [null = categoria padrão]
CriadaEm       DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- `Padrao = true` → não pode ser editada nem deletada
- Exclusão de categoria customizada bloqueada se houver transações vinculadas
- Categoria especial de sistema: `"Transferência"` com `Padrao = true`, usada por todas as transferências

---

### Orcamento
```
Id              int             PK, IDENTITY
UsuarioId       int             FK → Usuario, CASCADE
CategoriaId     int             FK → Categoria, RESTRICT
ValorLimite     decimal(18,2)   NOT NULL, > 0
AlertaEm        decimal(5,2)    NOT NULL DEFAULT 80 [percentual 0-100]
Mes             int             NOT NULL [1-12]
Ano             int             NOT NULL [>= 2020]
CriadoEm       DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- Único por `(UsuarioId, CategoriaId, Mes, Ano)`
- `ValorUtilizado` não é armazenado — calculado em tempo de consulta via `SUM` de despesas EFETIVADAS da categoria no período
- Notificação gerada quando `ValorUtilizado / ValorLimite >= AlertaEm / 100`

---

### MetaEconomia *(soft delete)*
```
Id              int             PK, IDENTITY
UsuarioId       int             FK → Usuario, CASCADE
Titulo          string(200)     NOT NULL
Descricao       string(500)?    NULL
ValorAlvo       decimal(18,2)   NOT NULL, > 0
ValorAtual      decimal(18,2)   NOT NULL DEFAULT 0
DataLimite      DateTime?       NULL
IsDeleted       bool            DEFAULT false
DeletedAt       DateTime?       NULL
CriadaEm       DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- `ValorAtual` aumenta via aportes manuais (`LancamentoMeta`)
- Notificação gerada quando `ValorAtual >= ValorAlvo`
- Meta expirada: `DataLimite < hoje AND ValorAtual < ValorAlvo` → destaque visual

---

### LancamentoMeta *(aportes)*
```
Id              int             PK, IDENTITY
MetaEconomiaId  int             FK → MetaEconomia, CASCADE
UsuarioId       int             FK → Usuario, CASCADE
Valor           decimal(18,2)   NOT NULL, > 0
Descricao       string(200)?    NULL
DataAporte      DateTime        NOT NULL
CriadoEm       DateTime        DEFAULT GETUTCDATE()
```

---

### Notificacao
```
Id              int             PK, IDENTITY
UsuarioId       int             FK → Usuario, CASCADE
Titulo          string(200)     NOT NULL
Mensagem        string(1000)    NOT NULL
Tipo            enum            ALERTA_ORCAMENTO_80|ALERTA_ORCAMENTO_100|META_ATINGIDA|FATURA_FECHADA
Lida            bool            DEFAULT false
CriadaEm       DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- Gerada automaticamente pelo serviço correspondente no momento do evento
- Sem TTL/expiração nesta versão — administração manual pelo usuário

---

### LogAuditoria *(imutável)*
```
Id              int             PK, IDENTITY
UsuarioId       int             NOT NULL [denormalizado, sem FK para performance]
Entidade        string(50)      NOT NULL [ex: "Transacao", "Conta"]
EntidadeId      int             NOT NULL
Operacao        string(50)      NOT NULL [CRIAR, EDITAR, CANCELAR, MUDAR_STATUS, DELETAR]
ValorAnterior   string(MAX)?    NULL [JSON com estado anterior]
ValorNovo       string(MAX)?    NULL [JSON com estado novo]
CriadoEm       DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- Registros nunca são deletados ou editados
- Preenchido via `AuditInterceptor` no EF Core para operações em `Transacao`

---

### ConfiguracaoUsuario
```
UsuarioId       int             PK, FK → Usuario, CASCADE
Moeda           string(3)       DEFAULT 'BRL'
Idioma          string(5)       DEFAULT 'pt-BR'
Tema            string(10)      DEFAULT 'light' [light|dark]
NotificacoesAtivas bool         DEFAULT true
DiaInicioMes    int             DEFAULT 1 [1-28]
```

---

### CodigoVerificacao
```
Id              int             PK, IDENTITY
UsuarioId       int             FK → Usuario, CASCADE
Codigo          string(6)       NOT NULL
Canal           string(10)      NOT NULL [EMAIL|WHATSAPP]  [NOVO]
Expira          DateTime        NOT NULL [now + 15min]
TentativasErradas int           DEFAULT 0                   [NOVO]
Usado           bool            DEFAULT false
CriadoEm       DateTime        DEFAULT GETUTCDATE()
```

**Regras**:
- `TentativasErradas >= 3` → código bloqueado (não pode ser usado mesmo dentro do prazo)
- `Expira < now` → código inválido
- `Usado = true` → código inválido
- Ao criar novo código, invalidar código anterior do mesmo usuário

---

### TokenAtualizacao
```
Id              int             PK, IDENTITY
UsuarioId       int             FK → Usuario, CASCADE
Token           string(MAX)     NOT NULL, UNIQUE
SubstituidoPor  string(MAX)?    NULL
Expirado        bool            DEFAULT false
Ativo           bool            DEFAULT true
CriadoEm       DateTime        DEFAULT GETUTCDATE()
ExpiraEm        DateTime        NOT NULL [now + 7 dias]
```

---

### FormaPagamento *(seeded, sem alterações)*
```
Id              int             PK, IDENTITY
Nome            string(50)      NOT NULL
Icone           string(50)      NOT NULL
```
*Seed: Dinheiro, Cartão de Crédito, Cartão de Débito, PIX, Boleto, Transferência*

---

## Entidades Fora de Escopo (mantidas, sem evolução nesta versão)

- `MensagemWhatsApp` — bot WhatsApp
- `ApelidoCategoria` — aliases do WhatsApp bot
- `Anexo` — comprovantes de pagamento

---

## Índices Recomendados

```sql
-- Performance de listagens frequentes
CREATE INDEX IX_Transacoes_UsuarioId_DataTransacao ON Transacoes (UsuarioId, DataTransacao DESC);
CREATE INDEX IX_Transacoes_CategoriaId_Status ON Transacoes (CategoriaId, Status);
CREATE INDEX IX_Transacoes_ContaId_Status ON Transacoes (ContaId, Status);
CREATE INDEX IX_Notificacoes_UsuarioId_Lida ON Notificacoes (UsuarioId, Lida);
CREATE INDEX IX_FaturasCartao_CartaoId_Mes_Ano ON FaturasCartao (CartaoCreditoId, MesReferencia, AnoReferencia);
CREATE UNIQUE INDEX UX_Orcamentos_Usuario_Cat_Mes_Ano ON Orcamentos (UsuarioId, CategoriaId, Mes, Ano);
```

---

## Resumo das Mudanças em Relação ao Schema Atual

| Entidade | Mudança de ID | Campos Novos | Comportamento Novo |
|---|---|---|---|
| Todas (21) | GUID → INT | — | — |
| Conta | ✓ | `IsDeleted`, `DeletedAt` | Soft delete |
| CartaoCredito | ✓ | `IsDeleted`, `DeletedAt` | Soft delete; bloquear exclusão |
| FaturaCartao | ✓ | `DataPagamento` | Ciclo ABERTA→FECHADA→PAGA |
| Transacao | ✓ | `TransferenciaContaId`, `FaturaCartaoId`, `AtualizadaEm` | Máquina de estados completa |
| CodigoVerificacao | ✓ | `Canal`, `TentativasErradas` | Reset via Email/WhatsApp |
| TransferenciaConta | ✓ | — | Implementação atômica real |
| MetaEconomia | ✓ | `IsDeleted`, `DeletedAt` | Soft delete |
| LogAuditoria | ✓ | `ValorAnterior`, `ValorNovo` | Preenchimento automático via interceptor |
