# Contrato: Metas de Economia

**Base URL**: `/api/metas` | **Auth**: Bearer token obrigatório

---

## GET /api/metas

Lista metas do usuário (excluindo soft-deleted).

**Response 200**:
```json
[
  {
    "id": 2,
    "titulo": "Viagem Europa",
    "descricao": "Férias em julho de 2027",
    "valorAlvo": 10000.00,
    "valorAtual": 2400.00,
    "percentual": 24.0,
    "dataLimite": "2026-12-31",
    "expirada": false,
    "atingida": false,
    "lancamentos": [
      { "id": 1, "valor": 800.00, "descricao": "Aporte janeiro", "dataAporte": "2026-01-31" },
      { "id": 2, "valor": 800.00, "descricao": "Aporte fevereiro", "dataAporte": "2026-02-28" }
    ]
  }
]
```

---

## POST /api/metas

**Request Body**:
```json
{
  "titulo": "Viagem Europa",
  "descricao": "Férias em julho de 2027",
  "valorAlvo": 10000.00,
  "dataLimite": "2026-12-31"
}
```

**Validações**: `valorAlvo > 0`, `dataLimite` opcional mas se informada deve ser futura.

**Response 201**: Meta criada com `valorAtual = 0`.

---

## PUT /api/metas/{id}

Atualiza dados da meta (exceto `valorAtual`, que só muda via aportes).

**Request Body**: `{ "titulo": "...", "valorAlvo": 12000.00, "dataLimite": "2027-06-30" }`

---

## DELETE /api/metas/{id}

Soft delete da meta e seus lançamentos.

---

## POST /api/metas/{id}/aportes

Registra um aporte na meta.

**Request Body**:
```json
{
  "valor": 800.00,
  "descricao": "Aporte junho",
  "dataAporte": "2026-06-30"
}
```

**Behavior**:
- Cria `LancamentoMeta` com o valor
- Atualiza `MetaEconomia.ValorAtual += valor`
- Se `ValorAtual >= ValorAlvo`: gera notificação `META_ATINGIDA`

**Response 201**:
```json
{
  "lancamentoId": 5,
  "novoValorAtual": 3200.00,
  "novoPercentual": 32.0,
  "metaAtingida": false
}
```

---

## DELETE /api/metas/{metaId}/aportes/{aporteId}

Remove um aporte e decrementa `ValorAtual` da meta.

**Response 200**: `{ "novoValorAtual": 2400.00, "novoPercentual": 24.0 }`
