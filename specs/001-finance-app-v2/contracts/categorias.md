# Contrato: Categorias

**Base URL**: `/api/categorias` | **Auth**: Bearer token obrigatório

---

## GET /api/categorias

Lista todas as categorias disponíveis para o usuário: padrões do sistema + customizadas do usuário.

**Query Parameters**:
```
tipo    string?     [DESPESA|RECEITA]   Filtra por tipo
```

**Response 200**:
```json
[
  {
    "id": 1,
    "nome": "Alimentação",
    "icone": "restaurant",
    "cor": "#FF6B35",
    "tipo": "DESPESA",
    "padrao": true,
    "editavel": false
  },
  {
    "id": 22,
    "nome": "Freelance Design",
    "icone": "brush",
    "cor": "#9B59B6",
    "tipo": "RECEITA",
    "padrao": false,
    "editavel": true
  }
]
```

---

## POST /api/categorias

Cria nova categoria customizada para o usuário.

**Request Body**:
```json
{
  "nome": "Freelance Design",
  "icone": "brush",
  "cor": "#9B59B6",
  "tipo": "RECEITA"
}
```

**Validações**:
- `nome` max 100 chars, não pode duplicar nome de outra categoria do mesmo usuário + padrão
- `icone` deve ser um nome de ícone Ionicons válido
- `cor` formato `#RRGGBB`
- `tipo`: DESPESA ou RECEITA

**Response 201**: Categoria criada com `padrao = false`, `UsuarioId = usuário autenticado`.

---

## PUT /api/categorias/{id}

Atualiza categoria customizada. **Bloqueado para categorias padrão** (`padrao = true`).

**Request Body**: `{ "nome": "...", "icone": "...", "cor": "..." }` *(tipo não pode mudar)*

**Response 400**: `{ "erro": "Categorias padrão do sistema não podem ser editadas" }`

---

## DELETE /api/categorias/{id}

Remove categoria customizada. **Bloqueado se**:
- `padrao = true` (categoria do sistema)
- Existir alguma transação vinculada (`CategoriaId = id`)

**Response 422**: `{ "erro": "Esta categoria possui 12 transação(ões) vinculada(s) e não pode ser removida" }`
**Response 400**: `{ "erro": "Categorias padrão do sistema não podem ser removidas" }`
