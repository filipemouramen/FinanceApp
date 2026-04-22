import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  RefreshControl,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import api from '../../api/client';
import { TransacaoResponse, ListaPaginada, Resultado } from '../../types';
import { Colors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { formatarMoeda, formatarData } from '../../utils/formatters';

export default function TransacoesScreen() {
  const navigation = useNavigation<any>();
  const [transacoes, setTransacoes] = useState<TransacaoResponse[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [pagina, setPagina] = useState(1);
  const [temMais, setTemMais] = useState(true);

  useFocusEffect(
    useCallback(() => {
      carregarTransacoes(1, true);
    }, [])
  );

  async function carregarTransacoes(pag: number = 1, reset: boolean = false) {
    try {
      if (reset) setCarregando(true);
      const response = await api.get('/transacoes', {
        params: { pagina: pag, itensPorPagina: 20 },
      });
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

  function renderTransacao({ item }: { item: TransacaoResponse }) {
    const isDespesa = item.tipo === 'DESPESA';

    return (
      <View style={styles.transacaoCard}>
        <View style={styles.transacaoEsquerda}>
          <View style={[styles.transacaoIcone, { backgroundColor: item.corCategoria + '20' }]}>
            <View style={[styles.transacaoDot, { backgroundColor: item.corCategoria }]} />
          </View>
          <View style={styles.transacaoInfo}>
            <Text style={styles.transacaoDescricao} numberOfLines={1}>
              {item.descricao || item.nomeCategoria}
            </Text>
            <Text style={styles.transacaoCategoria}>
              {item.nomeCategoria}
              {item.totalParcelas ? ` • ${item.numeroParcela}/${item.totalParcelas}` : ''}
            </Text>
            <Text style={styles.transacaoData}>{formatarData(item.dataTransacao)}</Text>
          </View>
        </View>
        <View style={styles.transacaoDireita}>
          <Text style={[styles.transacaoValor, { color: isDespesa ? Colors.danger : Colors.success }]}>
            {isDespesa ? '- ' : '+ '}
            {formatarMoeda(item.valor)}
          </Text>
          {item.status !== 'EFETIVADA' && (
            <View style={[styles.badgeStatus, item.atrasada && styles.badgeAtrasada]}>
              <Text style={[styles.badgeStatusTexto, item.atrasada && styles.badgeAtrasadaTexto]}>
                {item.atrasada ? 'Atrasada' : item.status}
              </Text>
            </View>
          )}
        </View>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitulo}>Transações</Text>
        <TouchableOpacity
          style={styles.botaoAdicionar}
          onPress={() => navigation.navigate('CriarTransacao')}
        >
          <Ionicons name="add" size={24} color={Colors.textWhite} />
        </TouchableOpacity>
      </View>

      {/* Lista */}
      <FlatList
        data={transacoes}
        keyExtractor={(item) => item.id}
        renderItem={renderTransacao}
        refreshControl={
          <RefreshControl
            refreshing={carregando}
            onRefresh={() => carregarTransacoes(1, true)}
            colors={[Colors.primary]}
          />
        }
        onEndReached={carregarMais}
        onEndReachedThreshold={0.3}
        contentContainerStyle={styles.lista}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.vazio}>
              <Ionicons name="receipt-outline" size={64} color={Colors.textMuted} />
              <Text style={styles.vazioTexto}>Nenhuma transação encontrada</Text>
              <Text style={styles.vazioSub}>Toque no + para registrar</Text>
            </View>
          ) : null
        }
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: Spacing.lg,
    paddingTop: 60,
    paddingBottom: Spacing.md,
    backgroundColor: Colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: Colors.borderLight,
  },
  headerTitulo: {
    fontSize: FontSize.xxl,
    fontWeight: '700',
    color: Colors.textPrimary,
  },
  botaoAdicionar: {
    backgroundColor: Colors.primary,
    width: 44,
    height: 44,
    borderRadius: 22,
    justifyContent: 'center',
    alignItems: 'center',
  },
  lista: {
    padding: Spacing.md,
    paddingBottom: 100,
  },
  transacaoCard: {
    backgroundColor: Colors.surface,
    borderRadius: BorderRadius.lg,
    padding: Spacing.md,
    marginBottom: Spacing.sm,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 4,
    elevation: 2,
  },
  transacaoEsquerda: {
    flexDirection: 'row',
    alignItems: 'center',
    flex: 1,
  },
  transacaoIcone: {
    width: 40,
    height: 40,
    borderRadius: 10,
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: Spacing.md,
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
    color: Colors.textPrimary,
  },
  transacaoCategoria: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    marginTop: 2,
  },
  transacaoData: {
    fontSize: FontSize.xs,
    color: Colors.textMuted,
    marginTop: 2,
  },
  transacaoDireita: {
    alignItems: 'flex-end',
  },
  transacaoValor: {
    fontSize: FontSize.md,
    fontWeight: '700',
  },
  badgeStatus: {
    backgroundColor: Colors.warningLight,
    paddingHorizontal: 8,
    paddingVertical: 2,
    borderRadius: 4,
    marginTop: 4,
  },
  badgeAtrasada: {
    backgroundColor: Colors.dangerLight,
  },
  badgeStatusTexto: {
    fontSize: FontSize.xs,
    color: Colors.warning,
    fontWeight: '600',
  },
  badgeAtrasadaTexto: {
    color: Colors.danger,
  },
  vazio: {
    alignItems: 'center',
    marginTop: 100,
  },
  vazioTexto: {
    fontSize: FontSize.lg,
    color: Colors.textSecondary,
    marginTop: Spacing.md,
    fontWeight: '600',
  },
  vazioSub: {
    fontSize: FontSize.md,
    color: Colors.textMuted,
    marginTop: Spacing.xs,
  },
});