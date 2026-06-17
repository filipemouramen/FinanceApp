namespace FinanceApp.Domain.Enums;

public enum TipoTransacao
{
    DESPESA,
    RECEITA
}

public enum StatusTransacao
{
    EFETIVADA,
    PENDENTE,
    VENCIDA,
    CANCELADA
}

public enum OrigemTransacao
{
    APP,
    WHATSAPP
}

public enum TipoConta
{
    CORRENTE,
    POUPANCA,
    CARTEIRA,
    INVESTIMENTOS
}

public enum StatusFatura
{
    ABERTA,
    FECHADA,
    PAGA,
    PARCIAL
}

public enum FrequenciaRecorrencia
{
    DIARIO,
    SEMANAL,
    MENSAL,
    ANUAL
}

public enum FinalidadeCodigo
{
    CONFIRMAR_EMAIL,
    RESETAR_SENHA
}

public enum TipoNotificacao
{
    ALERTA_ORCAMENTO_80,
    ALERTA_ORCAMENTO_100,
    META_ATINGIDA,
    FATURA_FECHADA,
    RECORRENCIA_VENCENDO,
    DICA
}
