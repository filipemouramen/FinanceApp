# Contrato: Contas Bancárias

**Base URL**: `/api/contas` | **Auth**: Bearer token obrigatório

---

## GET /api/contas

Lista contas do usuário (excluindo soft-deleted).

**Response 200**:
```json
[
  {
    "id": 1,
    "nome": "Nubank",
    "tipo": "CORRENTE",
    "banco": "Nubank",
    "saldoInicial": 0.00,
    "saldoAtual": 2800.00,
    "cor": "#8A05BE",
    "principal": true,
    "temCartaoVinculado": true,
    "criadaEm": "2026-01-01T00:00:00"
  }
]
```

---

## POST /api/contas

**Request Body**:
```json
{
  "nome": "Nubank",
  "tipo": "CORRENTE",
  "banco": "Nubank",
  "saldoInicial": 1000.00,
  "cor": "#8A05BE",
  "principal": false
}
```

**Behavior**: Se `principal = true`, desmarca a conta principal anterior.

**Response 201**: Conta criada com `saldoAtual = saldoInicial`.

---

## PUT /api/contas/{id}

Atualiza dados da conta. `SaldoInicial` pode ser alterado → recalcula `SaldoAtual`.

**Response 200**: Conta atualizada.

---

## DELETE /api/contas/{id}

Soft delete da conta.

**Bloqueado se**: Existir `CartaoCredito` com `IsDeleted = false` vinculado à conta.

**Behavior quando permitido**:
- `IsDeleted = true`, `DeletedAt = now`
- Transações vinculadas são preservadas (histórico)
- Conta deixa de aparecer em listagens

**Response 200**: `{ "mensagem": "Conta removida. Histórico de transações preservado." }`
**Response 422**: `{ "erro": "Não é possível remover a conta. Há 1 cartão de crédito vinculado ativo." }`
