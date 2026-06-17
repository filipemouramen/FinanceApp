# Contrato: Notificações

**Base URL**: `/api/notificacoes` | **Auth**: Bearer token obrigatório

---

## GET /api/notificacoes

Lista todas as notificações do usuário, mais recentes primeiro.

**Query Parameters**:
```
pagina          int     DEFAULT 1
tamanhoPagina   int     DEFAULT 20
apenasNaoLidas  bool    DEFAULT false
```

**Response 200**:
```json
{
  "totalNaoLidas": 2,
  "pagina": 1,
  "totalItens": 15,
  "itens": [
    {
      "id": 10,
      "titulo": "Orçamento Alimentação: Alerta!",
      "mensagem": "Você atingiu 87% do limite de R$ 1.000,00 em Alimentação",
      "tipo": "ALERTA_ORCAMENTO_80",
      "lida": false,
      "criadaEm": "2026-06-09T18:45:00"
    },
    {
      "id": 9,
      "titulo": "Meta atingida! 🎉",
      "mensagem": "Parabéns! Você atingiu 100% da meta 'Reserva Emergência'",
      "tipo": "META_ATINGIDA",
      "lida": false,
      "criadaEm": "2026-06-08T10:12:00"
    }
  ]
}
```

---

## GET /api/notificacoes/nao-lidas/count

Retorna apenas o count para o badge.

**Response 200**: `{ "count": 2 }`

---

## PUT /api/notificacoes/{id}/marcar-lida

Marca uma notificação específica como lida.

**Response 200**: `{ "id": 10, "lida": true }`

---

## PUT /api/notificacoes/marcar-todas-lidas

Marca todas as notificações não lidas do usuário como lidas.

**Response 200**: `{ "quantidadeMarcadas": 2, "totalNaoLidas": 0 }`

---

## DELETE /api/notificacoes/{id}

Remove uma notificação.

**Response 200**: `{ "mensagem": "Notificação removida" }`

---

## Tipos de Notificação e seus Gatilhos

| Tipo | Gatilho | Mensagem |
|---|---|---|
| `ALERTA_ORCAMENTO_80` | Orçamento >= percentual de alerta (default 80%) | "Você atingiu {X}% do limite de {valor} em {categoria}" |
| `ALERTA_ORCAMENTO_100` | Orçamento >= 100% | "Limite de {valor} excedido em {categoria}" |
| `META_ATINGIDA` | `ValorAtual >= ValorAlvo` | "Parabéns! Você atingiu 100% da meta '{titulo}'" |
| `FATURA_FECHADA` | Fatura muda de ABERTA para FECHADA | "Fatura {cartao} de {mes}/{ano} foi fechada: R$ {valor}. Vencimento: {data}" |
