# Contrato: Cartões de Crédito e Faturas

**Base URL**: `/api/cartoes` | **Auth**: Bearer token obrigatório

---

## GET /api/cartoes

Lista cartões de crédito do usuário (excluindo soft-deleted).

**Response 200**:
```json
[
  {
    "id": 1,
    "nome": "Nubank Roxinho",
    "bandeira": "Mastercard",
    "limiteTotal": 5000.00,
    "limiteDisponivel": 3800.00,
    "limiteUtilizado": 1200.00,
    "diaFechamento": 10,
    "diaVencimento": 17,
    "conta": { "id": 2, "nome": "Conta Corrente Nubank", "cor": "#8A05BE" },
    "faturaAtual": {
      "id": 8,
      "mesReferencia": 6,
      "anoReferencia": 2026,
      "valorTotal": 1200.00,
      "status": "ABERTA",
      "dataVencimento": "2026-07-17"
    }
  }
]
```

---

## POST /api/cartoes

Cria novo cartão de crédito.

**Request Body**:
```json
{
  "nome": "Nubank Roxinho",
  "bandeira": "Mastercard",
  "limiteTotal": 5000.00,
  "diaFechamento": 10,
  "diaVencimento": 17,
  "contaId": 2
}
```

**Validações**:
- `limiteTotal > 0`
- `diaFechamento` entre 1 e 28
- `diaVencimento` entre 1 e 28
- `contaId` deve pertencer ao usuário

**Response 201**: Cartão criado com `limiteDisponivel = limiteTotal`.

---

## PUT /api/cartoes/{id}

Atualiza dados do cartão. Limite e conta vinculada podem ser alterados.

**Response 200**: Cartão atualizado.
**Response 422**: `{ "erro": "Não é possível reduzir o limite abaixo do valor já utilizado" }`

---

## DELETE /api/cartoes/{id}

Soft delete do cartão.

**Bloqueado se**: Existir `FaturaCartao` com `Status IN (ABERTA, FECHADA)`.

**Response 200**: `{ "mensagem": "Cartão removido com sucesso" }`
**Response 422**: `{ "erro": "Não é possível remover o cartão. Existem faturas em aberto: R$ 1.200,00" }`

---

## GET /api/cartoes/{id}/faturas

Lista faturas do cartão, mais recente primeiro.

**Query Parameters**:
```
pagina      int     DEFAULT 1
quantidade  int     DEFAULT 6   [últimas N faturas]
```

**Response 200**:
```json
[
  {
    "id": 8,
    "mesReferencia": 6,
    "anoReferencia": 2026,
    "valorTotal": 1200.00,
    "status": "ABERTA",
    "dataVencimento": "2026-07-17",
    "dataFechamento": "2026-07-10",
    "dataPagamento": null,
    "quantidadeTransacoes": 4
  },
  {
    "id": 7,
    "mesReferencia": 5,
    "anoReferencia": 2026,
    "valorTotal": 850.00,
    "status": "PAGA",
    "dataVencimento": "2026-06-17",
    "dataFechamento": "2026-06-10",
    "dataPagamento": "2026-06-15T10:30:00",
    "quantidadeTransacoes": 3
  }
]
```

---

## GET /api/cartoes/{cartaoId}/faturas/{faturaId}

Detalhe de uma fatura com todas as transações.

**Response 200**:
```json
{
  "id": 8,
  "mesReferencia": 6,
  "anoReferencia": 2026,
  "valorTotal": 1200.00,
  "status": "ABERTA",
  "dataVencimento": "2026-07-17",
  "transacoes": [
    {
      "id": 55,
      "descricao": "Celular Samsung - Parcela 1/3",
      "valor": 400.00,
      "dataTransacao": "2026-06-10",
      "categoria": { "id": 7, "nome": "Eletrônicos", "icone": "phone-portrait", "cor": "#3498DB" },
      "status": "EFETIVADA"
    }
  ]
}
```

---

## POST /api/cartoes/{cartaoId}/faturas/{faturaId}/pagar

Paga uma fatura.

**Request Body**:
```json
{
  "dataPagemento": "2026-07-15"
}
```

**Behavior**:
1. Verifica `Status != PAGA` (idempotência)
2. Cria transação `DESPESA` na conta vinculada ao cartão com `Valor = FaturaCartao.ValorTotal`
3. Atualiza `FaturaCartao.Status = PAGA` e `DataPagamento`
4. Recalcula `CartaoCredito.LimiteDisponivel += FaturaCartao.ValorTotal`
5. Recalcula `Conta.SaldoAtual -= FaturaCartao.ValorTotal`
6. Toda a operação é atômica

**Response 200**:
```json
{
  "mensagem": "Fatura paga com sucesso",
  "valorPago": 1200.00,
  "novoLimiteDisponivel": 5000.00,
  "novoSaldoConta": 3800.00
}
```
**Response 422**: `{ "erro": "Esta fatura já foi paga em 15/07/2026" }`
**Response 422**: `{ "erro": "Saldo insuficiente na conta para pagar a fatura" }` *(avisar, não bloquear — saldo negativo permitido)*
