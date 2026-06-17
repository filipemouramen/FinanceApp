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
import { FaturaCartaoResponse, ContaResponse, Resultado } from '../../types';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { useTheme } from '../../theme/useTheme';
import { formatarMoeda, formatarData } from '../../utils/formatters';

const nomesMeses = ['', 'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];

function statusCor(status: string, colors: typeof LightColors): string {
  switch (status) {
    case 'PAGA': return colors.success;
    case 'FECHADA': return colors.warning;
    case 'VENCIDA': return colors.danger;
    default: return colors.primary;
  }
}

export default function FaturaDetalheScreen({ route }: any) {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const { cartaoId, nomeCartao } = route.params as { cartaoId: number; nomeCartao: string };
  const [faturas, setFaturas] = useState<FaturaCartaoResponse[]>([]);
  const [faturaSelecionada, setFaturaSelecionada] = useState<FaturaCartaoResponse | null>(null);
  const [contas, setContas] = useState<ContaResponse[]>([]);
  const [carregando, setCarregando] = useState(true);
  const styles = getStyles(colors);

  useFocusEffect(
    useCallback(() => {
      carregarFaturas();
      api.get('/contas').then((r) => { if (r.data.sucesso) setContas(r.data.dados); }).catch(() => {});
    }, [])
  );

  async function carregarFaturas() {
    try {
      setCarregando(true);
      const res = await api.get(`/cartoes/${cartaoId}/faturas`);
      const resultado: Resultado<FaturaCartaoResponse[]> = res.data;
      if (resultado.sucesso && resultado.dados) {
        setFaturas(resultado.dados);
        if (resultado.dados.length > 0 && !faturaSelecionada) {
          carregarDetalhesFatura(resultado.dados[0].id);
        }
      }
    } catch {
      console.log('Erro ao carregar faturas.');
    } finally {
      setCarregando(false);
    }
  }

  async function carregarDetalhesFatura(faturaId: number) {
    try {
      const res = await api.get(`/cartoes/${cartaoId}/faturas/${faturaId}`);
      const resultado: Resultado<FaturaCartaoResponse> = res.data;
      if (resultado.sucesso && resultado.dados) setFaturaSelecionada(resultado.dados);
    } catch {
      console.log('Erro ao carregar detalhes da fatura.');
    }
  }

  function confirmarPagamento(fatura: FaturaCartaoResponse) {
    if (contas.length === 0) {
      Alert.alert('Sem contas', 'Cadastre uma conta antes de pagar a fatura.');
      return;
    }

    const contaVinculada = contas[0];
    Alert.alert(
      'Pagar fatura',
      `Valor: ${formatarMoeda(fatura.valorTotal - fatura.valorPago)}\nConta: ${contaVinculada.nome}\n\nConfirmar pagamento?`,
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Pagar',
          onPress: async () => {
            try {
              const res = await api.post(`/cartoes/${cartaoId}/faturas/${fatura.id}/pagar`, {
                valor: fatura.valorTotal - fatura.valorPago,
                contaId: contaVinculada.id,
              });
              const resultado: Resultado<boolean> = res.data;
              Alert.alert('Sucesso', resultado.mensagem || 'Fatura paga!');
              carregarFaturas();
            } catch (e: any) {
              Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível pagar a fatura.');
            }
          },
        },
      ]
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={24} color={colors.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitulo}>{nomeCartao}</Text>
        <View style={{ width: 24 }} />
      </View>

      {/* Lista de faturas */}
      <FlatList
        data={faturas}
        horizontal
        showsHorizontalScrollIndicator={false}
        keyExtractor={(item) => item.id.toString()}
        style={styles.faturasLista}
        contentContainerStyle={styles.faturasContent}
        renderItem={({ item }) => (
          <TouchableOpacity
            style={[
              styles.faturaTab,
              faturaSelecionada?.id === item.id && styles.faturaTabSelecionada,
            ]}
            onPress={() => carregarDetalhesFatura(item.id)}
          >
            <Text style={[
              styles.faturaTabMes,
              faturaSelecionada?.id === item.id && styles.faturaTabMesSelecionado,
            ]}>
              {nomesMeses[item.mesReferencia]}/{item.anoReferencia}
            </Text>
            <Text style={[styles.faturaTabValor, { color: statusCor(item.status, colors) }]}>
              {formatarMoeda(item.valorTotal)}
            </Text>
            <View style={[styles.faturaTabBadge, { backgroundColor: statusCor(item.status, colors) + '20' }]}>
              <Text style={[styles.faturaTabBadgeTexto, { color: statusCor(item.status, colors) }]}>
                {item.status}
              </Text>
            </View>
          </TouchableOpacity>
        )}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.semFaturas}>
              <Text style={styles.semFaturasTexto}>Nenhuma fatura encontrada</Text>
            </View>
          ) : null
        }
      />

      {/* Detalhes da fatura selecionada */}
      {faturaSelecionada && (
        <View style={styles.detalheContainer}>
          <View style={styles.detalheHeader}>
            <View>
              <Text style={styles.detalheLabel}>Total da fatura</Text>
              <Text style={styles.detalheValorTotal}>{formatarMoeda(faturaSelecionada.valorTotal)}</Text>
            </View>
            {faturaSelecionada.status !== 'PAGA' && (
              <TouchableOpacity
                style={styles.botaoPagar}
                onPress={() => confirmarPagamento(faturaSelecionada)}
              >
                <Text style={styles.botaoPagarTexto}>Pagar fatura</Text>
              </TouchableOpacity>
            )}
          </View>

          <Text style={styles.transacoesLabel}>Transações</Text>
          <FlatList
            data={faturaSelecionada.transacoes}
            keyExtractor={(item) => item.id.toString()}
            refreshControl={
              <RefreshControl refreshing={carregando} onRefresh={carregarFaturas} colors={[colors.primary]} />
            }
            renderItem={({ item }) => (
              <View style={styles.transacaoItem}>
                <View style={[styles.transacaoCor, { backgroundColor: item.corCategoria }]} />
                <View style={styles.transacaoInfo}>
                  <Text style={styles.transacaoDescricao} numberOfLines={1}>
                    {item.descricao || item.nomeCategoria}
                    {item.totalParcelas ? ` (${item.numeroParcela}/${item.totalParcelas})` : ''}
                  </Text>
                  <Text style={styles.transacaoCategoria}>{item.nomeCategoria}</Text>
                  <Text style={styles.transacaoData}>{formatarData(item.dataTransacao)}</Text>
                </View>
                <Text style={styles.transacaoValor}>{formatarMoeda(item.valor)}</Text>
              </View>
            )}
            ListEmptyComponent={
              <View style={styles.semTransacoes}>
                <Text style={styles.semTransacoesTexto}>Nenhuma transação nesta fatura</Text>
              </View>
            }
            contentContainerStyle={styles.transacoesList}
          />
        </View>
      )}
    </View>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: Spacing.lg,
    paddingTop: 60,
    paddingBottom: Spacing.md,
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
  },
  headerTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  faturasLista: { maxHeight: 100, backgroundColor: colors.surface },
  faturasContent: { padding: Spacing.md, gap: Spacing.sm },
  faturaTab: {
    padding: Spacing.md,
    borderRadius: BorderRadius.md,
    borderWidth: 1.5,
    borderColor: colors.border,
    alignItems: 'center',
    minWidth: 100,
    backgroundColor: colors.background,
  },
  faturaTabSelecionada: { borderColor: colors.primary, backgroundColor: colors.primary + '08' },
  faturaTabMes: { fontSize: FontSize.sm, fontWeight: '600', color: colors.textSecondary },
  faturaTabMesSelecionado: { color: colors.primary },
  faturaTabValor: { fontSize: FontSize.md, fontWeight: '700', marginTop: 2 },
  faturaTabBadge: { paddingHorizontal: 6, paddingVertical: 2, borderRadius: 4, marginTop: 4 },
  faturaTabBadgeTexto: { fontSize: 9, fontWeight: '700' },
  semFaturas: { padding: Spacing.xl, alignItems: 'center' },
  semFaturasTexto: { color: colors.textMuted, fontSize: FontSize.md },
  detalheContainer: { flex: 1 },
  detalheHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: Spacing.lg,
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
  },
  detalheLabel: { fontSize: FontSize.xs, color: colors.textMuted, textTransform: 'uppercase' },
  detalheValorTotal: { fontSize: FontSize.xxl, fontWeight: '700', color: colors.textPrimary, marginTop: 2 },
  botaoPagar: {
    backgroundColor: colors.primary,
    paddingHorizontal: Spacing.lg,
    paddingVertical: Spacing.sm,
    borderRadius: BorderRadius.sm,
  },
  botaoPagarTexto: { color: colors.textWhite, fontWeight: '600', fontSize: FontSize.md },
  transacoesLabel: {
    paddingHorizontal: Spacing.lg,
    paddingTop: Spacing.md,
    paddingBottom: Spacing.sm,
    fontSize: FontSize.sm,
    fontWeight: '700',
    color: colors.textSecondary,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  transacoesList: { paddingHorizontal: Spacing.md, paddingBottom: 100 },
  transacaoItem: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.surface,
    borderRadius: BorderRadius.md,
    padding: Spacing.md,
    marginBottom: Spacing.sm,
    gap: Spacing.sm,
  },
  transacaoCor: { width: 4, height: 48, borderRadius: 2 },
  transacaoInfo: { flex: 1 },
  transacaoDescricao: { fontSize: FontSize.md, fontWeight: '600', color: colors.textPrimary },
  transacaoCategoria: { fontSize: FontSize.sm, color: colors.textSecondary, marginTop: 1 },
  transacaoData: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 1 },
  transacaoValor: { fontSize: FontSize.md, fontWeight: '700', color: colors.textPrimary },
  semTransacoes: { alignItems: 'center', padding: Spacing.xl },
  semTransacoesTexto: { color: colors.textMuted, fontSize: FontSize.md },
});
