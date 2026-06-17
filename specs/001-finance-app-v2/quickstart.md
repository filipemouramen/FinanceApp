# Quickstart: Guia de Validação End-to-End

**Date**: 2026-06-11 | **Plan**: [plan.md](plan.md)

Este guia descreve cenários de validação manual que provam o funcionamento correto das funcionalidades implementadas. Use o Swagger (`/swagger`) para endpoints da API e o app mobile para validação visual.

---

## Pré-requisitos

1. Backend rodando em `https://localhost:7137`
2. Swagger disponível em `https://localhost:7137/swagger`
3. App mobile conectado via `EXPO_PUBLIC_API_URL` no `.env`
4. Banco de dados com migration `v2` aplicada (INT IDs, soft delete, audit)

---

## Cenário 1: Fundação — Migração e Saldo

**Objetivo**: Confirmar que IDs são INT e saldo é sempre consistente.

### Passos:
1. Registrar usuário via `POST /api/auth/registrar`
   - Verificar: `id` no response é um número inteiro (ex: `1`), não GUID
2. Criar conta com saldo inicial R$1.000 via `POST /api/contas`
3. Criar despesa EFETIVADA R$300 via `POST /api/transacoes`
   - Verificar via `GET /api/contas`: `saldoAtual = 700.00`
4. Deletar a transação via `DELETE /api/transacoes/{id}`
   - Verificar via `GET /api/contas`: `saldoAtual = 1000.00` (saldo revertido)
5. Criar despesa PENDENTE R$200
   - Verificar via `GET /api/contas`: `saldoAtual = 1000.00` (PENDENTE não afeta saldo)
6. Marcar transação PENDENTE como EFETIVADA via `PUT /api/transacoes/{id}/status`
   - Verificar via `GET /api/contas`: `saldoAtual = 800.00`

**Critério de sucesso**: Saldo sempre correto após cada operação.

---

## Cenário 2: Máquina de Estados — PENDENTE → VENCIDA

**Objetivo**: Confirmar que transações com data passada aparecem como VENCIDA.

### Passos:
1. Criar transação PENDENTE com `dataTransacao = data de ontem`
2. Chamar `GET /api/transacoes` sem filtros
   - Verificar: a transação aparece com `status = "VENCIDA"` (atualização automática)
3. Marcar a transação como EFETIVADA via `PUT /api/transacoes/{id}/status`
   - Verificar: saldo da conta reduz pelo valor da transação
4. No app mobile: badge visual vermelho em transações vencidas deve aparecer

---

## Cenário 3: Edição de Transação

**Objetivo**: Confirmar que transações comuns podem ser editadas e saldo é ajustado.

### Passos:
1. Criar despesa EFETIVADA de R$100 na categoria "Alimentação" → saldo = R$900
2. Editar via `PUT /api/transacoes/{id}`: alterar valor para R$80
   - Verificar: `saldoAtual = 920.00` (diferença +R$20 revertida)
3. Editar categoria para "Transporte"
   - Verificar: categoria atualizada, saldo inalterado
4. Criar transação com parcelamento (3x) → tentar editar via `PUT /api/transacoes/{idParcela}`
   - Verificar: `400 Bad Request` com mensagem explicando que parceladas não podem ser editadas

---

## Cenário 4: Parcelamento e Cancelamento

**Objetivo**: Confirmar lógica de parcelas e cancelamento parcial.

### Passos:
1. Criar despesa parcelada R$900 em 3x no cartão de crédito
   - Verificar: 3 transações criadas (meses M, M+1, M+2)
   - Verificar: `limiteDisponivel` do cartão reduziu R$900
2. Marcar parcela 1 como EFETIVADA
3. Cancelar o parcelamento via `DELETE /api/transacoes/parcelamento/{parcelamentoId}`
   - Verificar: apenas parcelas 2 e 3 (PENDENTE) são canceladas
   - Verificar: parcela 1 (EFETIVADA) permanece no histórico
   - Verificar: `limiteDisponivel` restaurado em R$600 (parcelas 2 e 3)

---

## Cenário 5: Transferência entre Contas

**Objetivo**: Confirmar atomicidade e que totais não mudam no dashboard.

### Passos:
1. Criar conta A (R$2.000) e conta B (R$500) → total = R$2.500
2. Transferir R$800 de A para B via `POST /api/transferencias`
   - Verificar: conta A = R$1.200, conta B = R$1.300, total = R$2.500 (inalterado)
3. Chamar `GET /api/dashboard`
   - Verificar: `totalReceitas` e `totalDespesas` não incluem os R$800 da transferência
4. Chamar `GET /api/transacoes?incluirTransferencias=false` (default)
   - Verificar: transações de transferência não aparecem na lista

---

## Cenário 6: Cartão de Crédito e Ciclo de Fatura

**Objetivo**: Confirmar limite, alocação em fatura e pagamento.

### Passos (cartão com fechamento dia 10, vencimento dia 17, limite R$5.000):
1. Criar cartão via `POST /api/cartoes` → `limiteDisponivel = 5000.00`
2. Criar compra parcelada R$1.200 em 3x na data 05/06/2026 (antes do fechamento)
   - Verificar: `limiteDisponivel = 3800.00` imediatamente
   - Verificar: `GET /api/cartoes/{id}/faturas` mostra fatura Jun/2026 = R$400, Jul = R$400, Ago = R$400
3. Criar compra R$500 na data 12/06/2026 (após dia de fechamento)
   - Verificar: alocada na fatura Jul/2026 (não em Jun/2026)
4. Pagar fatura de Junho (R$400) via `POST /api/cartoes/{id}/faturas/{id}/pagar`
   - Verificar: conta corrente vinculada decrementou R$400
   - Verificar: `limiteDisponivel = 4200.00` (+R$400 restaurado)
   - Verificar: tentar pagar de novo → `422 Esta fatura já foi paga`

---

## Cenário 7: Orçamentos — Alerta e Notificação

**Objetivo**: Confirmar que alertas de orçamento geram notificações.

### Passos:
1. Criar orçamento R$100 para "Alimentação" no mês atual
2. Criar despesa EFETIVADA R$85 em "Alimentação"
   - Verificar: `GET /api/orcamentos` → `percentualUtilizado = 85.0`, `status = "ALERTA"`
   - Verificar: `GET /api/notificacoes/nao-lidas/count` → `count = 1`
   - Verificar: notificação gerada do tipo `ALERTA_ORCAMENTO_80`
3. Criar mais R$20 em "Alimentação" → total R$105
   - Verificar: `status = "EXCEDIDO"`, segunda notificação `ALERTA_ORCAMENTO_100`

---

## Cenário 8: Reset de Senha

**Objetivo**: Confirmar fluxo completo de recuperação.

### Passos:
1. `POST /api/auth/solicitar-reset-senha` com email válido e `canal: "EMAIL"`
   - Verificar: `200 OK`, código armazenado em `CodigoVerificacao` com `Expira = now + 15min`
2. `POST /api/auth/verificar-codigo` com código correto
   - Verificar: `200 OK` com `tokenTemporario`
3. `POST /api/auth/verificar-codigo` com código incorreto (antes do expirar)
   - Verificar: `400`, `tentativasRestantes = 2`
4. `POST /api/auth/resetar-senha` com `tokenTemporario` e nova senha
   - Verificar: `200 OK`
5. `POST /api/auth/login` com a nova senha
   - Verificar: `200 OK`, login bem-sucedido

---

## Cenário 9: Dark Mode

**Objetivo**: Confirmar persistência e cobertura total.

### Passos:
1. Abrir o app → tema claro (default)
2. Ir para Configurações → ativar dark mode
   - Verificar imediatamente: todas as telas mudam para fundo escuro
3. Fechar completamente o app → reabrir
   - Verificar: dark mode ainda ativo (persiste via AsyncStorage)
4. Navegar por todas as telas: Home, Transações, Contas, Config, Cartões, Metas, Orçamentos
   - Verificar: nenhuma tela com fundo branco no dark mode

---

## Checklist de Aceitação Final

```
[ ] IDs são inteiros em todos os endpoints (não GUIDs)
[ ] SaldoAtual sempre consistente após criar/editar/cancelar transação
[ ] Transações PENDENTE com data passada aparecem como VENCIDA automaticamente
[ ] Transferências não afetam totais de receita/despesa no dashboard
[ ] Cartão de crédito: limite reduz na compra, restaura no pagamento de fatura
[ ] Fatura já paga não pode ser paga novamente
[ ] Conta com cartão ativo não pode ser excluída
[ ] Categoria com transações vinculadas não pode ser excluída
[ ] Categoria padrão não pode ser editada nem excluída
[ ] Reset de senha funciona via email
[ ] Notificações geradas em orçamento 80% e 100%
[ ] Dark mode persiste entre sessões e cobre todas as telas
[ ] LogAuditoria preenchido para operações sensíveis de transação
```
