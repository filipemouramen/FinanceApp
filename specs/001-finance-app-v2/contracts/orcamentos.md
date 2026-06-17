# Contrato: Orçamentos

**Base URL**: `/api/orcamentos` | **Auth**: Bearer token obrigatório

---

## GET /api/orcamentos

Lista orçamentos do usuário com percentual utilizado calculado.

**Query Parameters**:
```
mes     int     DEFAULT mês atual
ano     int     DEFAULT ano atual
```

**Response 200**:
```json
[
  {
    "id": 5,
    "categoria": { "id": 3, "nome": "Alimentação", "icone": "restaurant", "cor": "#FF6B35" },
    "valorLimite": 1000.00,
    "valorUtilizado": 890.00,
    "percentualUtilizado": 89.0,
    "alertaEm": 80.0,
    "status": "ALERTA",
    "mes": 6,
    "ano": 2026
  }
]
```

`status`: `OK` (< alertaEm%), `ALERTA` (>= alertaEm% e < 100%), `EXCEDIDO` (>= 100%)

---

## POST /api/orcamentos

Cria novo orçamento.

**Request Body**:
```json
{
  "categoriaId": 3,
  "valorLimite": 1000.00,
  "alertaEm": 80.0,
  "mes": 6,
  "ano": 2026
}
```

**Validações**:
- `valorLimite > 0`
- `alertaEm` entre 1 e 100 (default 80)
- Apenas categorias do tipo `DESPESA`
- Único por `(UsuarioId, CategoriaId, Mes, Ano)`

**Response 201**: Orçamento criado.
**Response 422**: `{ "erro": "Já existe um orçamento para Alimentação em Junho/2026" }`

---

## PUT /api/orcamentos/{id}

**Request Body**: `{ "valorLimite": 1200.00, "alertaEm": 90.0 }`

**Response 200**: Orçamento atualizado.

---

## DELETE /api/orcamentos/{id}

Hard delete do orçamento.

**Response 200**: `{ "mensagem": "Orçamento removido" }`
