# Contrato: Transações

**Base URL**: `/api/transacoes` | **Auth**: Bearer token obrigatório em todos os endpoints

---

## GET /api/transacoes

Lista paginada de transações com filtros.

**Query Parameters**:
```
pagina          int     DEFAULT 1       Número da página
tamanhoPagina   int     DEFAULT 20      Itens por página (max 100)
mes             int?    [1-12]          Mês de referência
ano             int?    [>= 2020]       Ano de referência
dataInicio      date?   [YYYY-MM-DD]    Início do período
dataFim         date?   [YYYY-MM-DD]    Fim do período
categoriaId     int[]?                  IDs de categorias (múltiplos: ?categoriaId=1&categoriaId=3)
contaId         int?                    ID da conta
cartaoId        int?                    ID do cartão
tipo            string? [DESPESA|RECEITA] Tipo da transação
status          string? [PENDENTE|EFETIVADA|VENCIDA]  Status
busca           string?                 Busca textual na descrição (case-insensitive)
incluirTransferencias bool DEFAULT false Incluir transações de transferência
```

**Behavior**: 
- Antes de retornar, o sistema atualiza automaticamente transações `PENDENTE` com `DataTransacao < hoje` para `VENCIDA`.
- Retorna apenas transações do usuário autenticado.
- Se `mes` e `ano` são informados, equivale a `dataInicio = primeiro dia do mês` e `dataFim = último dia do mês`.
- Por padrão, `incluirTransferencias = false` (exclui do retorno transações de transferência).

**Response 200**:
```json
{
  "pagina": 1,
  "tamanhoPagina": 20,
  "totalItens": 87,
  "totalPaginas": 5,
  "totalReceitas": 5000.00,
  "totalDespesas": 3200.50,
  "saldoPeriodo": 1799.50,
  "itens": [
    {
      "id": 42,
      "descricao": "Mercado Extra",
      "valor": 189.90,
      "tipo": "DESPESA",
      "status": "EFETIVADA",
      "dataTransacao": "2026-06-05T00:00:00",
      "categoria": { "id": 3, "nome": "Alimentação", "icone": "restaurant", "cor": "#FF6B35" },
      "conta": { "id": 1, "nome": "Nubank", "cor": "#8A05BE" },
      "cartaoCredito": null,
      "parcelamento": null,
      "transferencia": null,
      "formaPagamento": { "id": 4, "nome": "PIX" },
      "criadaEm": "2026-06-05T14:23:00"
    }
  ]
}
```

---

## GET /api/transacoes/{id}

**Response 200**: Objeto transação completo (mesmo formato do item acima).
**Response 404**: `{ "erro": "Transação não encontrada" }`

---

## POST /api/transacoes

Cria uma nova transação (simples ou parcelada).

**Request Body**:
```json
{
  "valor": 1200.00,
  "descricao": "Celular Samsung",
  "tipo": "DESPESA",
  "status": "PENDENTE",
  "dataTransacao": "2026-06-10",
  "categoriaId": 5,
  "contaId": null,
  "cartaoCreditoId": 2,
  "formaPagamentoId": 2,
  "parcelas": 3
}
```

**Campos**:
- `valor`: obrigatório, > 0
- `tipo`: obrigatório, `DESPESA` ou `RECEITA`
- `status`: opcional, default `EFETIVADA` (se não fornecido, considera efetivada na data)
- `dataTransacao`: obrigatório
- `categoriaId`: obrigatório
- `contaId`: obrigatório se `cartaoCreditoId` for null
- `cartaoCreditoId`: obrigatório se `contaId` for null; não pode ser usado junto com `contaId`
- `parcelas`: opcional, default 1; se > 1, cria parcelamento
- `formaPagamentoId`: opcional (apenas para DESPESA)

**Behavior para parcelamento** (`parcelas > 1`):
- Cria entidade `Parcelamento`
- Cria N entidades `Transacao` com `DataTransacao` avançando 1 mês por parcela
- Se for cartão de crédito, aloca cada parcela na `FaturaCartao` do mês correspondente (respeitando dia de fechamento)
- Reduz `LimiteDisponivel` do cartão pelo valor total imediatamente

**Behavior para transação EFETIVADA**:
- Recalcula `SaldoAtual` da conta vinculada

**Response 201**:
```json
{
  "id": 55,
  "parcelamentoId": 12,
  "totalParcelas": 3,
  "valorParcela": 400.00,
  "transacoesCriadas": [55, 56, 57]
}
```

**Response 400**: Validações falhas.
**Response 422**: Regra de negócio violada (ex: limite insuficiente no cartão).

---

## PUT /api/transacoes/{id}

Edita uma transação existente. **Bloqueado para transações parceladas** (ParcelamentoId IS NOT NULL).

**Request Body** (campos opcionais — envia apenas o que muda):
```json
{
  "valor": 159.90,
  "descricao": "Mercado Extra (corrigido)",
  "categoriaId": 3,
  "contaId": 1,
  "dataTransacao": "2026-06-05",
  "formaPagamentoId": 4
}
```

**Behavior**:
- Se `valor` muda e transação é `EFETIVADA`: recalcula `SaldoAtual` com a diferença
- Se `contaId` muda e transação é `EFETIVADA`: recalcula `SaldoAtual` de ambas as contas
- Registra entrada em `LogAuditoria` com estado anterior e novo

**Response 200**: Transação atualizada.
**Response 400**: `{ "erro": "Transações parceladas não podem ser editadas individualmente" }`
**Response 403**: Tentativa de editar transação de outro usuário.

---

## PUT /api/transacoes/{id}/status

Altera o status de uma transação.

**Request Body**:
```json
{
  "status": "EFETIVADA"
}
```

**Transições permitidas**:
- `PENDENTE → EFETIVADA`: afeta `SaldoAtual`
- `VENCIDA → EFETIVADA`: afeta `SaldoAtual`

**Response 200**: `{ "id": 42, "novoStatus": "EFETIVADA", "saldoAtualizado": 850.00 }`
**Response 422**: `{ "erro": "Transição de status inválida: EFETIVADA → PENDENTE" }`

---

## DELETE /api/transacoes/{id}

Cancela e remove uma transação.

**Behavior**:
- Se `EFETIVADA`: reverte `SaldoAtual` da conta + registra em `LogAuditoria`
- Se `PENDENTE` ou `VENCIDA`: remove sem afetar saldo
- Se for transação de `Parcelamento`: veja endpoint abaixo
- Se for transação de `Transferência`: cancela ambas as transações do par atomicamente
- Remoção é física (hard delete) após reversão de efeitos

**Response 200**: `{ "mensagem": "Transação cancelada. Saldo revertido: +R$ 189,90", "saldoAtualizado": 850.00 }`
**Response 404**: Transação não encontrada.

---

## DELETE /api/transacoes/parcelamento/{parcelamentoId}

Cancela todas as parcelas pendentes/vencidas de um parcelamento.

**Behavior**:
- Cancela apenas parcelas com status `PENDENTE` ou `VENCIDA`
- Parcelas `EFETIVADA` permanecem no histórico
- Reverte saldo apenas das parcelas `EFETIVADA` que foram canceladas? Não — apenas PENDENTE/VENCIDA são canceladas, e essas não afetam saldo
- Se cartão de crédito: restaura `LimiteDisponivel` proporcionalmente às parcelas canceladas

**Response 200**:
```json
{
  "parcelamentoId": 12,
  "parcelasCanceladas": 5,
  "parcelasMantidasPagas": 3,
  "limiteRestaurado": 2000.00
}
```
