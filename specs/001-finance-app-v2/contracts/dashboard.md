# Contrato: Dashboard

**Base URL**: `/api/dashboard` | **Auth**: Bearer token obrigatório

---

## GET /api/dashboard

Retorna dados consolidados do dashboard para um período mensal.

**Query Parameters**:
```
mes     int     DEFAULT mês atual   [1-12]
ano     int     DEFAULT ano atual   [>= 2020]
```

**Behavior**:
- Transações de transferência (`TransferenciaContaId IS NOT NULL`) são **excluídas** dos totais de receita e despesa.
- Apenas transações `EFETIVADA` entram nos totais.
- Orçamentos calculam `ValorUtilizado` dinamicamente via SUM de despesas EFETIVADAS.

**Response 200**:
```json
{
  "periodo": { "mes": 6, "ano": 2026, "descricao": "Junho 2026" },
  "resumo": {
    "totalReceitas": 5000.00,
    "totalDespesas": 3215.50,
    "saldoPeriodo": 1784.50,
    "totalTransferencias": 500.00,
    "quantidadeTransacoes": 23,
    "mediaDiaria": 107.18,
    "maiorDespesa": { "valor": 1200.00, "descricao": "Celular Samsung", "categoriaId": 7 },
    "categoriaComMaisGastos": { "id": 3, "nome": "Alimentação", "total": 890.00 }
  },
  "gastosPorCategoria": [
    {
      "categoria": { "id": 3, "nome": "Alimentação", "icone": "restaurant", "cor": "#FF6B35" },
      "valor": 890.00,
      "percentual": 27.7
    }
  ],
  "historico6Meses": [
    {
      "mes": 1, "ano": 2026, "descricao": "Jan",
      "receitas": 5000.00, "despesas": 3100.00, "saldo": 1900.00
    }
  ],
  "contas": [
    {
      "id": 1, "nome": "Nubank", "tipo": "CORRENTE",
      "saldoAtual": 2800.00, "cor": "#8A05BE", "principal": true
    }
  ],
  "orcamentos": [
    {
      "id": 5,
      "categoria": { "id": 3, "nome": "Alimentação", "icone": "restaurant" },
      "valorLimite": 1000.00,
      "valorUtilizado": 890.00,
      "percentualUtilizado": 89.0,
      "status": "ALERTA"
    }
  ],
  "metas": [
    {
      "id": 2,
      "titulo": "Viagem Europa",
      "valorAlvo": 10000.00,
      "valorAtual": 2400.00,
      "percentual": 24.0,
      "dataLimite": "2026-12-31",
      "expirada": false
    }
  ],
  "faturasCartao": [
    {
      "id": 8,
      "cartao": { "id": 1, "nome": "Nubank Roxinho" },
      "valorTotal": 1200.00,
      "status": "ABERTA",
      "dataVencimento": "2026-07-17",
      "diasParaVencer": 37
    }
  ],
  "transacoesRecentes": [
    {
      "id": 55,
      "descricao": "Celular Samsung",
      "valor": 400.00,
      "tipo": "DESPESA",
      "dataTransacao": "2026-06-10",
      "categoria": { "id": 7, "nome": "Eletrônicos", "icone": "phone-portrait", "cor": "#3498DB" }
    }
  ]
}
```

**Nota sobre `historico6Meses`**: Retorna sempre os 6 meses anteriores ao mês selecionado (inclusive o mês selecionado), independente do parâmetro `mes/ano`. Transferências excluídas.
