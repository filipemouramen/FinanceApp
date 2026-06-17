// ===== ENUMS =====
export type TipoTransacao = 'DESPESA' | 'RECEITA';
export type StatusTransacao = 'EFETIVADA' | 'PENDENTE' | 'VENCIDA' | 'CANCELADA';
export type TipoNotificacao =
  | 'ALERTA_ORCAMENTO_80'
  | 'ALERTA_ORCAMENTO_100'
  | 'META_ATINGIDA'
  | 'FATURA_FECHADA'
  | 'RECORRENCIA_VENCENDO'
  | 'DICA';

// ===== AUTH =====
export interface UsuarioResponse {
  id: number;
  nomeCompleto: string;
  email: string;
  telefoneWhatsApp?: string;
  fotoUrl?: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiraEm: string;
  usuario: UsuarioResponse;
}

export interface Resultado<T> {
  sucesso: boolean;
  dados?: T;
  mensagem?: string;
  erros: string[];
  statusCode?: number;
}

// ===== TRANSACOES =====
export interface TransacaoResponse {
  id: number;
  categoriaId: number;
  nomeCategoria: string;
  iconeCategoria: string;
  corCategoria: string;
  contaId?: number;
  nomeConta?: string;
  formaPagamentoId?: number;
  nomeFormaPagamento?: string;
  cartaoCreditoId?: number;
  nomeCartaoCredito?: string;
  transferenciasContaId?: number;
  parcelamentoId?: number;
  valor: number;
  tipo: TipoTransacao;
  descricao?: string;
  dataTransacao: string;
  origem: string;
  status: StatusTransacao;
  atrasada: boolean;
  observacoes?: string;
  recorrente: boolean;
  numeroParcela?: number;
  totalParcelas?: number;
  criadoEm: string;
}

export interface ListaPaginada<T> {
  itens: T[];
  totalItens: number;
  pagina: number;
  itensPorPagina: number;
  totalPaginas: number;
  temAnterior: boolean;
  temProximo: boolean;
}

// ===== CATEGORIAS =====
export interface CategoriaResponse {
  id: number;
  nome: string;
  icone: string;
  cor: string;
  tipo: TipoTransacao;
  padrao: boolean;
  editavel: boolean;
  totalTransacoes: number;
}

// ===== CONTAS =====
export interface ContaResponse {
  id: number;
  nome: string;
  tipoConta: string;
  banco?: string;
  cor: string;
  icone: string;
  saldoInicial: number;
  saldoAtual: number;
  principal: boolean;
  ativo: boolean;
  temCartaoVinculado: boolean;
  criadoEm: string;
}

export interface TransferenciaResponse {
  id: number;
  contaOrigemId: number;
  contaDestinoId: number;
  nomeContaOrigem: string;
  nomeContaDestino: string;
  valor: number;
  descricao?: string;
  dataTransferencia: string;
}

// ===== CARTOES =====
export interface CartaoCreditoResponse {
  id: number;
  nome: string;
  bandeira?: string;
  ultimosDigitos?: string;
  limiteTotal: number;
  limiteDisponivel: number;
  limiteUtilizado: number;
  percentualUtilizado: number;
  diaFechamento: number;
  diaVencimento: number;
  cor: string;
  contaId?: number;
  nomeConta?: string;
  ativo: boolean;
}

export interface FaturaCartaoResponse {
  id: number;
  cartaoCreditoId: number;
  nomeCartao: string;
  mesReferencia: number;
  anoReferencia: number;
  dataFechamento: string;
  dataVencimento?: string;
  dataPagamento?: string;
  valorTotal: number;
  valorPago: number;
  valorRestante: number;
  status: string;
  transacoes: TransacaoFaturaResponse[];
}

export interface TransacaoFaturaResponse {
  id: number;
  descricao?: string;
  nomeCategoria: string;
  corCategoria: string;
  valor: number;
  dataTransacao: string;
  numeroParcela?: number;
  totalParcelas?: number;
}

// ===== ORCAMENTOS =====
export interface OrcamentoResponse {
  id: number;
  categoriaId: number;
  nomeCategoria: string;
  iconeCategoria: string;
  corCategoria: string;
  valorLimite: number;
  totalGasto: number;
  valorRestante: number;
  percentualUsado: number;
  percentualAlerta: number;
  estourado: boolean;
  emAlerta: boolean;
  mes: number;
  ano: number;
}

// ===== METAS =====
export interface MetaResponse {
  id: number;
  titulo: string;
  valorAlvo: number;
  valorAtual: number;
  valorRestante: number;
  percentualConcluido: number;
  dataLimite?: string;
  diasRestantes?: number;
  icone?: string;
  cor?: string;
  concluida: boolean;
  criadoEm: string;
  ultimosLancamentos: LancamentoMetaResponse[];
}

export interface LancamentoMetaResponse {
  id: number;
  valor: number;
  observacoes?: string;
  criadoEm: string;
}

// ===== DASHBOARD =====
export interface DashboardResponse {
  resumo: ResumoFinanceiroResponse;
  gastosPorCategoria: GastoPorCategoriaResponse[];
  balancoUltimos6Meses: BalancoMensalResponse[];
  ultimasTransacoes: TransacaoRecenteResponse[];
  orcamentos: OrcamentoResumoResponse[];
  metas: MetaResumoResponse[];
  proximasFaturas: FaturaResumoResponse[];
  contas: ContaSaldoResponse[];
  totalTransferencias?: number;
}

export interface ResumoFinanceiroResponse {
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
  saldoTotalContas: number;
  totalTransacoesMes: number;
  mediaDiariaDespesas: number;
  maiorDespesaMes: number;
  categoriaMaisGasta?: string;
}

export interface GastoPorCategoriaResponse {
  categoriaId: number;
  nomeCategoria: string;
  iconeCategoria: string;
  corCategoria: string;
  valorTotal: number;
  percentual: number;
  quantidadeTransacoes: number;
}

export interface BalancoMensalResponse {
  ano: number;
  mes: number;
  nomeMes: string;
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}

export interface TransacaoRecenteResponse {
  id: number;
  descricao?: string;
  nomeCategoria: string;
  iconeCategoria: string;
  corCategoria: string;
  valor: number;
  tipo: TipoTransacao;
  status: StatusTransacao;
  origem: string;
  dataTransacao: string;
}

export interface OrcamentoResumoResponse {
  id: number;
  nomeCategoria: string;
  iconeCategoria: string;
  corCategoria: string;
  valorLimite: number;
  totalGasto: number;
  percentualUsado: number;
  estourado: boolean;
}

export interface MetaResumoResponse {
  id: number;
  titulo: string;
  valorAlvo: number;
  valorAtual: number;
  percentualConcluido: number;
  cor?: string;
  icone?: string;
}

export interface FaturaResumoResponse {
  id: number;
  nomeCartao: string;
  corCartao: string;
  valorTotal: number;
  valorPago: number;
  dataVencimento: string;
  status: string;
  diasParaVencimento: number;
}

export interface ContaSaldoResponse {
  id: number;
  nome: string;
  banco?: string;
  cor: string;
  icone: string;
  saldoAtual: number;
}

// ===== NOTIFICACOES =====
export interface NotificacaoResponse {
  id: number;
  titulo: string;
  mensagem: string;
  tipo: TipoNotificacao;
  lida: boolean;
  entidadeRelacionadaId?: number;
  criadoEm: string;
  tempoAtras: string;
}

export interface ListaNotificacoesResponse {
  totalNaoLidas: number;
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  itens: NotificacaoResponse[];
}

export interface ContadorNotificacoesResponse {
  count: number;
}

// ===== CONFIGURACOES =====
export interface ConfiguracaoResponse {
  moeda: string;
  diaInicioMes: number;
  whatsAppAtivado: boolean;
  notificacoesPush: boolean;
  alertasOrcamento: boolean;
  alertasFatura: boolean;
  modoEscuro: boolean;
  idioma: string;
}
