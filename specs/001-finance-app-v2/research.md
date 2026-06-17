# Research: app-Finance v2.0

**Date**: 2026-06-11 | **Plan**: [plan.md](plan.md)

---

## Decision 1: Estratégia de Migração GUID → INT no EF Core

**Decision**: Criar uma nova migration completa que substitui os tipos de ID. Como todos os dados existentes são de teste (confirmado pelo PO), a abordagem é: DROP + CREATE com nova estrutura INT.

**Rationale**: Migração incremental GUID→INT em banco com dados reais é extremamente complexa (requer tabelas temporárias, update de FKs). Como os dados são de teste, a abordagem mais segura e simples é:
1. Remover a migration inicial (`20260419175534_Inicial`)
2. Atualizar todas as entidades de `Guid` para `int`
3. Criar nova migration única `Inicial_v2` com a estrutura INT
4. Executar `DROP DATABASE` (dev) ou `UPDATE-DATABASE 0` + `DROP TABLE` em staging
5. Executar `Update-Database` com a nova migration

**Alternatives considered**:
- Migração em produção com script SQL: descartada — dados são de teste, complexidade desnecessária
- Manter GUID em tabelas-chave e migrar gradualmente: descartada — gera inconsistência permanente na base

**Impact**: Todos os tokens JWT existentes ficam inválidos (o `sub` claim contém o GUID do usuário). Usuários precisam fazer login novamente. Aceitável em ambiente de desenvolvimento.

---

## Decision 2: Implementação de Soft Delete no EF Core

**Decision**: Interface `ISoftDeletable` + Global Query Filter + Override de `SaveChangesAsync` para interceptar deletes.

**Rationale**: O EF Core oferece suporte nativo a Global Query Filters que automaticamente adicionam `WHERE IsDeleted = 0` em todas as queries para entidades que implementam a interface. O override de `SaveChangesAsync` intercepta chamadas `EntityState.Deleted` e as converte em updates de `IsDeleted = true`.

**Entidades com Soft Delete**: `Conta`, `CartaoCredito`. Orçamentos e Metas usam hard delete (sem valor histórico).

**Implementação**:
```
// Interface
interface ISoftDeletable {
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

// No DbContext.OnModelCreating:
modelBuilder.Entity<Conta>().HasQueryFilter(e => !e.IsDeleted);
modelBuilder.Entity<CartaoCredito>().HasQueryFilter(e => !e.IsDeleted);

// No DbContext.SaveChangesAsync:
// Interceptar EntityState.Deleted para ISoftDeletable → converter para Modified + IsDeleted=true
```

**Alternatives considered**:
- Soft delete via coluna `DataExclusao` nullable: funcionalmente equivalente, preferimos `IsDeleted` bool para legibilidade das queries
- Biblioteca Auditable (ex: Audit.NET): overhead desnecessário para o escopo atual

---

## Decision 3: Cálculo de SaldoAtual — Campo Armazenado vs. Derivado

**Decision**: `SaldoAtual` permanece como campo armazenado, mas é **recalculado** a cada operação de transação via `ContaService.RecalcularSaldo()`.

**Rationale**: Calcular `SaldoAtual` com `SUM()` de todas as transações em cada consulta é ineficiente para contas com histórico longo. Manter como campo armazenado é mais performático, desde que haja um mecanismo confiável de recálculo.

**Protocolo de recálculo**:
- Toda operação no `TransacaoService` que afeta uma conta (criar, editar valor/conta, mudar status para/de EFETIVADA, cancelar) chama `ContaService.RecalcularSaldo(contaId)` ao final, dentro da mesma transação de banco de dados.
- `RecalcularSaldo(contaId)`: `SELECT SaldoInicial + SUM(CASE tipo WHEN RECEITA THEN valor ELSE -valor END) WHERE ContaId = X AND Status = EFETIVADA`

**Alternatives considered**:
- Saldo sempre derivado (sem campo): mais consistente, mas lento para contas antigas
- Event sourcing: complexidade excessiva para o escopo atual

---

## Decision 4: Atomicidade de Operações Financeiras

**Decision**: Todas as operações que afetam múltiplas entidades (saldo + transação, transferência, pagamento de fatura) usam uma única **transação de banco de dados** via `IDbContextTransaction` / `using var transaction = await _context.BeginTransactionAsync()`.

**Rationale**: Operações parcialmente salvas em finanças são inaceitáveis. O EF Core permite agrupar múltiplas operações em uma transação explícita. Em caso de qualquer exceção, o `catch` chama `transaction.RollbackAsync()`.

**Pattern**:
```
await using var transaction = await _context.Database.BeginTransactionAsync();
try {
    // operações
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync();
    throw;
}
```

---

## Decision 5: Ciclo de Fatura do Cartão de Crédito

**Decision**: A alocação de uma compra na fatura correta segue a regra do **dia de fechamento**:
- Se `DataTransacao.Day < DiaFechamento`: a compra vai para a fatura do mês de `DataTransacao`.
- Se `DataTransacao.Day >= DiaFechamento`: a compra vai para a fatura do mês seguinte.

**Exemplo**: Compra dia 10, fechamento dia 10 → fatura do próximo mês (dia de fechamento é inclusivo no próximo ciclo).

**Fatura auto-gerada**: Quando uma transação de cartão é criada, se a `FaturaCartao` para `(CartaoId, Mes, Ano)` não existir, ela é criada automaticamente com status `ABERTA`.

**Fechamento automático de fatura**: Verificado no carregamento das faturas — se hoje >= DiaFechamento e a fatura está ABERTA, ela é marcada como FECHADA.

**Alternatives considered**:
- Fechamento via job/cron: fora de escopo (sem infraestrutura de background jobs)
- Fechamento manual pelo usuário: menos amigável

---

## Decision 6: Transferências entre Contas

**Decision**: Uma transferência cria atomicamente **duas transações** vinculadas por `TransferenciaContaId`. As transações têm categoria especial de sistema `"Transferência"` com `Id` fixo (seeded).

**Filtro no dashboard**: O `DashboardService` exclui transações onde `TransferenciaContaId IS NOT NULL` ao calcular totais de receita/despesa.

**Exclusão de transferência**: Se o usuário tentar cancelar uma transação de transferência, ambas as transações são canceladas atomicamente.

---

## Decision 7: Token JWT — Configuração de Expiração

**Decision**:
- Access Token: **15 minutos** (curto para segurança)
- Refresh Token: **7 dias** (renovado a cada uso — rotação)
- Código de verificação (reset de senha): **15 minutos**, máximo 3 tentativas

**Rationale**: Tokens curtos limitam a janela de exploração se capturados. Refresh com rotação invalida tokens roubados na próxima renovação. O cliente Axios já tem interceptor de refresh implementado — apenas a configuração do tempo está faltando.

**Race condition no refresh**: O cliente mobile deve implementar `isRefreshing` flag + fila de requests pendentes para evitar múltiplos refreshes simultâneos.

---

## Decision 8: Notificações In-App (sem Push Externo)

**Decision**: Notificações são geradas e armazenadas na tabela `Notificacoes` (entidade já existe) no momento do evento de negócio. O app busca notificações via polling simples na abertura ou navegação entre telas.

**Eventos que geram notificação**:
1. Orçamento atingindo 80% do limite (após cada transação de despesa na categoria)
2. Orçamento ultrapassando 100% do limite
3. Meta de economia atingindo 100%
4. Fatura de cartão com status FECHADA (aguardando pagamento)

**Badge**: O `GET /api/notificacoes/nao-lidas/count` retorna o número para exibir no ícone.

**Alternatives considered**:
- FCM/APNs (push externo): descartado nesta versão — sem infraestrutura configurada
- WebSocket para tempo real: complexidade excessiva para o escopo

---

## Decision 9: Exportação PDF

**Decision**: Geração de PDF **server-side** usando a biblioteca **QuestPDF** (open-source, MIT, nativa .NET) ou **iText7 Community** como alternativa.

**Rationale**: QuestPDF oferece API fluente, sem dependências externas pesadas, com suporte a tabelas e formatação adequada para extratos financeiros.

**Fluxo**:
1. Frontend: usuário seleciona período (máx 3 meses) → POST para `/api/exportacao/pdf`
2. Backend: gera PDF em memória → retorna como `application/pdf` com `Content-Disposition: attachment`
3. Frontend: recebe bytes → usa API nativa do Expo para salvar/compartilhar

**Alternatives considered**:
- Geração client-side: limitações de memória no mobile
- Jasper/Crystal Reports: desnecessariamente complexos para um extrato simples

---

## Decision 10: Dark Mode no React Native/Expo

**Decision**: `ThemeContext` com `React.createContext` armazena o tema atual (`'light' | 'dark'`). Todos os componentes consomem via `useTheme()` hook. Persistência via `AsyncStorage`.

**Paleta dark**:
- Background principal: `#121212`
- Surface (cards): `#1E1E1E`
- Textos primários: `#FFFFFF`
- Textos secundários: `#AAAAAA`
- Primary accent: `#7C73FF` (versão mais clara do roxo para contraste)
- Success: `#2ECC71`
- Danger: `#E74C3C`
- Warning: `#F39C12`

**Alternatives considered**:
- `useColorScheme()` do React Native (segue o sistema): menos controle, o usuário do app pode querer diferente do sistema
- Biblioteca de theming (ex: React Native Paper): overhead desnecessário

---

## Decision 11: Variável de Ambiente para URL da API

**Decision**: Usar `EXPO_PUBLIC_API_URL` no arquivo `.env` (Expo suporta natively com prefixo `EXPO_PUBLIC_`).

**config.ts** passa a ler `process.env.EXPO_PUBLIC_API_URL` com fallback para `http://10.0.2.2:7137` (emulador Android).

**Arquivo `.env.example`** criado na raiz do projeto mobile com instruções.

**Alternatives considered**:
- `app.config.js` com `extra`: mais complexo sem ganho de segurança para URL pública
- Manter detecção por plataforma: problemático em redes Wi-Fi variadas

---

## Decision 12: Cascade Delete — Remoção de Comportamentos Perigosos

**Decision**: Remover todas as configurações `OnDelete(DeleteBehavior.Cascade)` para entidades financeiras. Substituir por `OnDelete(DeleteBehavior.Restrict)`.

**Entidades protegidas**: `Conta → Transacao`, `Usuario → Transacao`, `Categoria → Transacao`, `CartaoCredito → FaturaCartao`.

**Razão**: O cascade delete atual pode apagar todo o histórico financeiro de um usuário ao deletar uma conta. Com `Restrict`, o sistema bloqueia a exclusão e exige tratamento explícito (soft delete da conta, que preserva as transações).

**Exceptions**: `Usuario → TokenAtualizacao`, `Usuario → Notificacao`, `Usuario → ConfiguracaoUsuario` → mantêm Cascade (dados não-financeiros, sem valor histórico).
