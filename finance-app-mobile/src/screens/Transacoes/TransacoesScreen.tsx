import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  RefreshControl,
  Alert,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import api from '../../api/client';
import { TransacaoResponse, ListaPaginada, Resultado, StatusTransacao, CategoriaResponse } from '../../types';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { useTheme } from '../../theme/useTheme';
import { formatarMoeda, formatarData } from '../../utils/formatters';
import FiltroTransacaoModal, { FiltroAtivo } from './FiltroTransacaoModal';

function badgeColor(status: StatusTransacao, colors: typeof LightColors): { bg: string; text: string } {
  switch (status) {
    case 'EFETIVADA': return { bg: colors.successLight, text: colors.success };
    case 'VENCIDA':   return { bg: colors.dangerLight,  text: colors.danger };
    case 'CANCELADA': return { bg: '#E0E0E0',           text: '#888888' };
    default:          return { bg: colors.warningLight, text: colors.warning };
  }
}

function labelStatus(status: StatusTransacao): string {
  switch (status) {
    case 'EFETIVADA': return 'Efetivada';
    case 'PENDENTE':  return 'Pendente';
    case 'VENCIDA':   return 'Vencida';
    case 'CANCELADA': return 'Cancelada';
    default: return status;
  }
}

export default function TransacoesScreen() {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const [transacoes, setTransacoes] = useState<TransacaoResponse[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [pagina, setPagina] = useState(1);
  const [temMais, setTemMais] = useState(true);
  const [filtroModal, setFiltroModal] = useState(false);
  const [filtroAtivo, setFiltroAtivo] = useState<FiltroAtivo>({});
  const [categorias, setCategorias] = useState<CategoriaResponse[]>([]);
  const styles = getStyles(colors);

  const totalReceitas = transacoes.filter((t) => t.tipo === 'RECEITA').reduce((acc, t) => acc + t.valor, 0);
  const totalDespesas = transacoes.filter((t) => t.tipo === 'DESPESA').reduce((acc, t) => acc + t.valor, 0);

  useFocusEffect(
    useCallback(() => {
      carregarTransacoes(1, true);
      api.get('/categorias').then((r) => { if (r.data.sucesso) setCategorias(r.data.dados); }).catch(() => {});
    }, [filtroAtivo])
  );

  const filtrosAtivosCount = Object.values(filtroAtivo).filter((v) => v !== undefined && (Array.isArray(v) ? v.length > 0 : true)).length;

  async function carregarTransacoes(pag: number = 1, reset: boolean = false) {
    try {
      if (reset) setCarregando(true);
      const params: any = { pagina: pag, itensPorPagina: 20, ...filtroAtivo };
      if (filtroAtivo.categoriasIds?.length) params.categoriasIds = filtroAtivo.categoriasIds.join(',');
      const response = await api.get('/transacoes', { params });
      const resultado: Resultado<ListaPaginada<TransacaoResponse>> = response.data;

      if (resultado.sucesso && resultado.dados) {
        if (reset) {
          setTransacoes(resultado.dados.itens);
        } else {
          setTransacoes((prev) => [...prev, ...resultado.dados!.itens]);
        }
        setPagina(pag);
        setTemMais(resultado.dados.temProximo);
      }
    } catch (error) {
      console.log('Erro ao carregar transações:', error);
    } finally {
      setCarregando(false);
    }
  }

  function carregarMais() {
    if (temMais && !carregando) {
      carregarTransacoes(pagina + 1, false);
    }
  }

  async function marcarComoPago(item: TransacaoResponse) {
    Alert.alert(
      'Efetivar transação',
      `Confirmar pagamento de "${item.descricao || item.nomeCategoria}" (${formatarMoeda(item.valor)})?`,
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Efetivar',
          onPress: async () => {
            try {
              await api.patch(`/transacoes/${item.id}/status`, { status: 'EFETIVADA' });
              carregarTransacoes(1, true);
              Alert.alert('Efetivada!', `${item.descricao || item.nomeCategoria} marcada como paga.`);
            } catch {
              Alert.alert('Erro', 'Não foi possível marcar como pago.');
            }
          },
        },
      ]
    );
  }

  async function cancelarTransacao(item: TransacaoResponse) {
    Alert.alert(
      'Cancelar transação',
      `Deseja cancelar "${item.descricao || item.nomeCategoria}" (${formatarMoeda(item.valor)})?`,
      [
        { text: 'Não', style: 'cancel' },
        {
          text: 'Sim, cancelar',
          style: 'destructive',
          onPress: async () => {
            try {
              await api.delete(`/transacoes/${item.id}`);
              carregarTransacoes(1, true);
              Alert.alert('Cancelada', 'A transação foi cancelada com sucesso.');
            } catch {
              Alert.alert('Erro', 'Não foi possível cancelar a transação.');
            }
          },
        },
      ]
    );
  }

  function renderTransacao({ item }: { item: TransacaoResponse }) {
    const isTransferencia = item.transferenciasContaId != null;
    const isDespesa = item.tipo === 'DESPESA';
    const badge = badgeColor(item.status, colors);
    const podePagar = !isTransferencia && (item.status === 'PENDENTE' || item.status === 'VENCIDA');
    const podeCancelar = !isTransferencia && item.status !== 'CANCELADA';

    return (
      <View style={styles.transacaoCard}>
        <View style={styles.transacaoEsquerda}>
          <View style={[styles.transacaoIcone, { backgroundColor: isTransferencia ? '#607D8B20' : item.corCategoria + '20' }]}>
            {isTransferencia
              ? <Ionicons name="swap-horizontal" size={18} color="#607D8B" />
              : <View style={[styles.transacaoDot, { backgroundColor: item.corCategoria }]} />
            }
          </View>
          <View style={styles.transacaoInfo}>
            <Text style={styles.transacaoDescricao} numberOfLines={1}>
              {item.descricao || (isTransferencia ? 'Transferência' : item.nomeCategoria)}
            </Text>
            <Text style={styles.transacaoCategoria}>
              {isTransferencia ? 'Transferência entre contas' : item.nomeCategoria}
              {item.totalParcelas ? ` • ${item.numeroParcela}/${item.totalParcelas}` : ''}
            </Text>
            <Text style={styles.transacaoData}>{formatarData(item.dataTransacao)}</Text>
            {/* Status badge */}
            {item.status !== 'EFETIVADA' && (
              <View style={[styles.badgeStatus, { backgroundColor: badge.bg }]}>
                <Text style={[styles.badgeStatusTexto, { color: badge.text }]}>
                  {labelStatus(item.status)}
                </Text>
              </View>
            )}
          </View>
        </View>
        <View style={styles.transacaoDireita}>
          <Text style={[styles.transacaoValor, { color: isDespesa ? colors.danger : colors.success }]}>
            {isDespesa ? '- ' : '+ '}
            {formatarMoeda(item.valor)}
          </Text>
          <View style={styles.acoesContainer}>
            {item.status !== 'CANCELADA' && (
              <TouchableOpacity
                style={styles.botaoAcao}
                onPress={() => navigation.navigate('CriarTransacao', { transacaoParaEditar: item })}
              >
                <Ionicons name="pencil-outline" size={18} color={colors.textSecondary} />
              </TouchableOpacity>
            )}
            {podePagar && (
              <TouchableOpacity
                style={styles.botaoAcao}
                onPress={() => marcarComoPago(item)}
              >
                <Ionicons name="checkmark-circle-outline" size={20} color={colors.success} />
              </TouchableOpacity>
            )}
            {podeCancelar && (
              <TouchableOpacity
                style={styles.botaoAcao}
                onPress={() => cancelarTransacao(item)}
              >
                <Ionicons name="close-circle-outline" size={20} color={colors.danger} />
              </TouchableOpacity>
            )}
          </View>
        </View>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.headerTitulo}>Transações</Text>
        <View style={styles.headerAcoes}>
          <TouchableOpacity style={styles.botaoFiltro} onPress={() => setFiltroModal(true)}>
            <Ionicons name="filter" size={20} color={filtrosAtivosCount > 0 ? colors.primary : colors.textSecondary} />
            {filtrosAtivosCount > 0 && (
              <View style={styles.badgeFiltro}>
                <Text style={styles.badgeFiltroTexto}>{filtrosAtivosCount}</Text>
              </View>
            )}
          </TouchableOpacity>
          <TouchableOpacity
            style={styles.botaoAdicionar}
            onPress={() => navigation.navigate('CriarTransacao')}
          >
            <Ionicons name="add" size={24} color={colors.textWhite} />
          </TouchableOpacity>
        </View>
      </View>

      {filtrosAtivosCount > 0 && (
        <View style={styles.bannerFiltro}>
          <View style={styles.bannerItem}>
            <Text style={styles.bannerLabel}>Receitas</Text>
            <Text style={[styles.bannerValor, { color: colors.success }]}>{formatarMoeda(totalReceitas)}</Text>
          </View>
          <View style={styles.bannerItem}>
            <Text style={styles.bannerLabel}>Despesas</Text>
            <Text style={[styles.bannerValor, { color: colors.danger }]}>{formatarMoeda(totalDespesas)}</Text>
          </View>
          <View style={styles.bannerItem}>
            <Text style={styles.bannerLabel}>Saldo</Text>
            <Text style={[styles.bannerValor, { color: totalReceitas - totalDespesas >= 0 ? colors.success : colors.danger }]}>
              {formatarMoeda(totalReceitas - totalDespesas)}
            </Text>
          </View>
        </View>
      )}

      <FiltroTransacaoModal
        visivel={filtroModal}
        filtro={filtroAtivo}
        categorias={categorias}
        onAplicar={(f) => { setFiltroAtivo(f); }}
        onFechar={() => setFiltroModal(false)}
      />

      <FlatList
        data={transacoes}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderTransacao}
        refreshControl={
          <RefreshControl
            refreshing={carregando}
            onRefresh={() => carregarTransacoes(1, true)}
            colors={[colors.primary]}
          />
        }
        onEndReached={carregarMais}
        onEndReachedThreshold={0.3}
        contentContainerStyle={styles.lista}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.vazio}>
              <Ionicons name="receipt-outline" size={64} color={colors.textMuted} />
              <Text style={styles.vazioTexto}>Nenhuma transação encontrada</Text>
              <Text style={styles.vazioSub}>Toque no + para registrar</Text>
            </View>
          ) : null
        }
      />
    </View>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: Spacing.lg,
    paddingTop: 60,
    paddingBottom: Spacing.md,
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
  },
  headerTitulo: {
    fontSize: FontSize.xxl,
    fontWeight: '700',
    color: colors.textPrimary,
  },
  headerAcoes: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  botaoFiltro: {
    width: 40,
    height: 40,
    borderRadius: 20,
    borderWidth: 1.5,
    borderColor: colors.border,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: colors.surface,
  },
  badgeFiltro: {
    position: 'absolute',
    top: -2,
    right: -2,
    backgroundColor: colors.primary,
    width: 16,
    height: 16,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
  },
  badgeFiltroTexto: {
    fontSize: 9,
    color: colors.textWhite,
    fontWeight: '700',
  },
  botaoAdicionar: {
    backgroundColor: colors.primary,
    width: 44,
    height: 44,
    borderRadius: 22,
    justifyContent: 'center',
    alignItems: 'center',
  },
  bannerFiltro: {
    flexDirection: 'row',
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
    paddingVertical: Spacing.sm,
    paddingHorizontal: Spacing.lg,
    justifyContent: 'space-around',
  },
  bannerItem: {
    alignItems: 'center',
  },
  bannerLabel: {
    fontSize: FontSize.xs,
    color: colors.textMuted,
    textTransform: 'uppercase',
    letterSpacing: 0.3,
  },
  bannerValor: {
    fontSize: FontSize.md,
    fontWeight: '700',
    marginTop: 2,
  },
  lista: {
    padding: Spacing.md,
    paddingBottom: 100,
  },
  transacaoCard: {
    backgroundColor: colors.surface,
    borderRadius: BorderRadius.lg,
    padding: Spacing.md,
    marginBottom: Spacing.sm,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 4,
    elevation: 2,
  },
  transacaoEsquerda: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    flex: 1,
  },
  transacaoIcone: {
    width: 40,
    height: 40,
    borderRadius: 10,
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: Spacing.md,
    marginTop: 2,
  },
  transacaoDot: {
    width: 12,
    height: 12,
    borderRadius: 6,
  },
  transacaoInfo: {
    flex: 1,
  },
  transacaoDescricao: {
    fontSize: FontSize.md,
    fontWeight: '600',
    color: colors.textPrimary,
  },
  transacaoCategoria: {
    fontSize: FontSize.sm,
    color: colors.textSecondary,
    marginTop: 2,
  },
  transacaoData: {
    fontSize: FontSize.xs,
    color: colors.textMuted,
    marginTop: 2,
  },
  transacaoDireita: {
    alignItems: 'flex-end',
  },
  transacaoValor: {
    fontSize: FontSize.md,
    fontWeight: '700',
  },
  acoesContainer: {
    flexDirection: 'row',
    marginTop: 4,
  },
  botaoAcao: {
    padding: 4,
    marginLeft: 4,
  },
  badgeStatus: {
    alignSelf: 'flex-start',
    paddingHorizontal: 8,
    paddingVertical: 2,
    borderRadius: 4,
    marginTop: 4,
  },
  badgeStatusTexto: {
    fontSize: FontSize.xs,
    fontWeight: '600',
  },
  vazio: {
    alignItems: 'center',
    marginTop: 100,
  },
  vazioTexto: {
    fontSize: FontSize.lg,
    color: colors.textSecondary,
    marginTop: Spacing.md,
    fontWeight: '600',
  },
  vazioSub: {
    fontSize: FontSize.md,
    color: colors.textMuted,
    marginTop: Spacing.xs,
  },
});
