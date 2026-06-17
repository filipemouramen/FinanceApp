# Contrato: Transferências entre Contas

**Base URL**: `/api/transferencias` | **Auth**: Bearer token obrigatório

---

## POST /api/transferencias

Cria uma transferência entre duas contas do usuário.

**Request Body**:
```json
{
  "contaOrigemId": 1,
  "contaDestinoId": 3,
  "valor": 500.00,
  "dataTransferencia": "2026-06-10",
  "descricao": "Reserva para poupança"
}
```

**Validações**:
- `contaOrigemId != contaDestinoId`
- `valor > 0`
- Ambas as contas pertencem ao usuário autenticado
- Ambas as contas têm `IsDeleted = false`

**Behavior** (atômico):
1. Cria `TransferenciaConta`
2. Cria `Transacao` DESPESA na conta origem: `TransferenciaContaId = id criado`, `CategoriaId = [id fixo de "Transferência"]`, `Status = EFETIVADA`
3. Cria `Transacao` RECEITA na conta destino: mesmos vínculos
4. Recalcula `SaldoAtual` de ambas as contas
5. Em caso de falha: rollback completo

**Response 201**:
```json
{
  "id": 15,
  "contaOrigem": { "id": 1, "nome": "Nubank", "novoSaldo": 1500.00 },
  "contaDestino": { "id": 3, "nome": "Poupança", "novoSaldo": 1200.00 },
  "valor": 500.00,
  "dataTransferencia": "2026-06-10",
  "transacaoOrigemId": 101,
  "transacaoDestinoId": 102
}
```

**Response 400**: Validações falhas (mesma conta, valor inválido).
**Response 404**: Conta não encontrada ou não pertence ao usuário.

---

## GET /api/transferencias

Lista histórico de transferências do usuário.

**Query Parameters**:
```
pagina          int     DEFAULT 1
tamanhoPagina   int     DEFAULT 20
mes             int?
ano             int?
```

**Response 200**:
```json
{
  "pagina": 1,
  "totalItens": 8,
  "itens": [
    {
      "id": 15,
      "valor": 500.00,
      "dataTransferencia": "2026-06-10",
      "descricao": "Reserva para poupança",
      "contaOrigem": { "id": 1, "nome": "Nubank", "cor": "#8A05BE" },
      "contaDestino": { "id": 3, "nome": "Poupança", "cor": "#27AE60" }
    }
  ]
}
```

---

## DELETE /api/transferencias/{id}

Cancela uma transferência (atomicamente cancela ambas as transações vinculadas).

**Behavior**:
1. Localiza `TransferenciaConta` pelo id
2. Localiza as duas `Transacao` vinculadas via `TransferenciaContaId`
3. Reverte `SaldoAtual` de ambas as contas (ambas eram EFETIVADA)
4. Remove fisicamente ambas as transações
5. Remove `TransferenciaConta`

**Response 200**:
```json
{
  "mensagem": "Transferência cancelada. Saldos revertidos.",
  "contaOrigem": { "id": 1, "nome": "Nubank", "saldoRestaurado": 2000.00 },
  "contaDestino": { "id": 3, "nome": "Poupança", "saldoRestaurado": 700.00 }
}
```
