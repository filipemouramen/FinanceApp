// ===== AUTH =====
export interface UsuarioResponse {
  id: string;
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
  id: string;
  categoriaId: number;
  nomeCategoria: string;
  iconeCategoria: string;
  corCategoria: string;
  contaId?: string;
  nomeConta?: string;
  formaPagamentoId?: number;
  nomeFormaPagamento?: string;
  cartaoCreditoId?: string;
  nomeCartaoCredito?: string;
  valor: number;
  tipo: string;
  descricao?: string;
  dataTransacao: string;
  origem: string;
  status: string;
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
  tipo: string;
  padrao: boolean;
  totalTransacoes: number;
}

// ===== CONTAS =====
export interface ContaResponse {
  id: string;
  nome: string;
  tipoConta: string;
  banco?: string;
  cor: string;
  icone: string;
  saldoInicial: number;
  saldoAtual: number;
  principal: boolean;
  ativo: boolean;
  criadoEm: string;
}

export interface TransferenciaResponse {
  id: string;
  contaOrigem: string;
  contaDestino: string;
  valor: number;
  descricao?: string;
  dataTransferencia: string;
}

// ===== CARTOES =====
export interface CartaoCreditoResponse {
  id: string;
  nome: string;
  bandeira?: string;
  ultimosDigitos?: string;
  limite: number;
  limiteDisponivel: number;
  limiteUtilizado: number;
  percentualUtilizado: number;
  diaFechamento: number;
  diaVencimento: number;
  cor: string;
  nomeConta?: string;
  ativo: boolean;
}

export interface FaturaCartaoResponse {
  id: string;
  cartaoCreditoId: string;
  nomeCartao: string;
  mesReferencia: number;
  anoReferencia: number;
  dataFechamento: string;
  dataVencimento: string;
  valorTotal: number;
  valorPago: number;
  valorRestante: number;
  status: string;
  transacoes: TransacaoFaturaResponse[];
}

export interface TransacaoFaturaResponse {
  id: string;
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
  id: string;
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
  id: string;
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
  id: string;
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
  id: string;
  descricao?: string;
  nomeCategoria: string;
  iconeCategoria: string;
  corCategoria: string;
  valor: number;
  tipo: string;
  origem: string;
  dataTransacao: string;
}

export interface OrcamentoResumoResponse {
  nomeCategoria: string;
  iconeCategoria: string;
  corCategoria: string;
  valorLimite: number;
  totalGasto: number;
  percentualUsado: number;
  estourado: boolean;
}

export interface MetaResumoResponse {
  id: string;
  titulo: string;
  valorAlvo: number;
  valorAtual: number;
  percentualConcluido: number;
  cor?: string;
  icone?: string;
}

export interface FaturaResumoResponse {
  id: string;
  nomeCartao: string;
  corCartao: string;
  valorTotal: number;
  valorPago: number;
  dataVencimento: string;
  status: string;
  diasParaVencimento: number;
}

export interface ContaSaldoResponse {
  id: string;
  nome: string;
  banco?: string;
  cor: string;
  icone: string;
  saldoAtual: number;
}

// ===== NOTIFICACOES =====
export interface NotificacaoResponse {
  id: string;
  titulo: string;
  mensagem: string;
  tipo: string;
  lida: boolean;
  entidadeRelacionadaId?: string;
  criadoEm: string;
  tempoAtras: string;
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