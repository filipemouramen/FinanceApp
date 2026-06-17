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
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { useTheme } from '../../theme/useTheme';

interface NotificacaoItem {
  id: number;
  titulo: string;
  mensagem: string;
  tipo: string;
  lida: boolean;
  tempoAtras: string;
  criadoEm: string;
}

interface ListaNotificacoesResponse {
  totalNaoLidas: number;
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  itens: NotificacaoItem[];
}

function iconeNotificacao(tipo: string): keyof typeof Ionicons.glyphMap {
  switch (tipo) {
    case 'ALERTA_ORCAMENTO_80': return 'warning-outline';
    case 'ALERTA_ORCAMENTO_100': return 'alert-circle-outline';
    case 'META_ATINGIDA': return 'trophy-outline';
    case 'FATURA_FECHADA': return 'receipt-outline';
    case 'RECORRENCIA_VENCENDO': return 'time-outline';
    default: return 'notifications-outline';
  }
}

function corNotificacao(tipo: string, colors: typeof LightColors): string {
  switch (tipo) {
    case 'ALERTA_ORCAMENTO_80': return colors.warning;
    case 'ALERTA_ORCAMENTO_100': return colors.danger;
    case 'META_ATINGIDA': return colors.success;
    case 'FATURA_FECHADA': return colors.info;
    default: return colors.primary;
  }
}

export default function NotificacoesScreen() {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const [notificacoes, setNotificacoes] = useState<NotificacaoItem[]>([]);
  const [totalNaoLidas, setTotalNaoLidas] = useState(0);
  const [carregando, setCarregando] = useState(true);

  const styles = getStyles(colors);

  useFocusEffect(
    useCallback(() => {
      carregarNotificacoes();
    }, [])
  );

  async function carregarNotificacoes() {
    try {
      setCarregando(true);
      const res = await api.get('/notificacoes');
      const resultado: { sucesso: boolean; dados: ListaNotificacoesResponse } = res.data;
      if (resultado.sucesso && resultado.dados) {
        setNotificacoes(resultado.dados.itens);
        setTotalNaoLidas(resultado.dados.totalNaoLidas);
      }
    } catch {
      console.log('Erro ao carregar notificações.');
    } finally {
      setCarregando(false);
    }
  }

  async function marcarLida(id: number) {
    try {
      await api.put(`/notificacoes/${id}/lida`);
      setNotificacoes((prev) =>
        prev.map((n) => (n.id === id ? { ...n, lida: true } : n))
      );
      setTotalNaoLidas((prev) => Math.max(0, prev - 1));
    } catch {
      console.log('Erro ao marcar notificação como lida.');
    }
  }

  async function marcarTodasLidas() {
    try {
      await api.put('/notificacoes/marcar-todas-lidas');
      setNotificacoes((prev) => prev.map((n) => ({ ...n, lida: true })));
      setTotalNaoLidas(0);
    } catch {
      Alert.alert('Erro', 'Não foi possível marcar as notificações como lidas.');
    }
  }

  async function excluirNotificacao(id: number) {
    try {
      await api.delete(`/notificacoes/${id}`);
      setNotificacoes((prev) => prev.filter((n) => n.id !== id));
    } catch {
      Alert.alert('Erro', 'Não foi possível excluir a notificação.');
    }
  }

  function handlePress(notif: NotificacaoItem) {
    if (!notif.lida) marcarLida(notif.id);
  }

  function handleLongPress(notif: NotificacaoItem) {
    Alert.alert(
      'Notificação',
      'O que deseja fazer?',
      [
        notif.lida ? null : { text: 'Marcar como lida', onPress: () => marcarLida(notif.id) },
        { text: 'Excluir', style: 'destructive', onPress: () => excluirNotificacao(notif.id) },
        { text: 'Cancelar', style: 'cancel' },
      ].filter(Boolean) as any
    );
  }

  function renderItem({ item }: { item: NotificacaoItem }) {
    const cor = corNotificacao(item.tipo, colors);
    const icone = iconeNotificacao(item.tipo);

    return (
      <TouchableOpacity
        style={[styles.item, !item.lida && styles.itemNaoLido]}
        onPress={() => handlePress(item)}
        onLongPress={() => handleLongPress(item)}
        activeOpacity={0.8}
      >
        <View style={[styles.iconContainer, { backgroundColor: cor + '20' }]}>
          <Ionicons name={icone} size={22} color={cor} />
        </View>
        <View style={styles.conteudo}>
          <View style={styles.topoItem}>
            <Text style={styles.titulo} numberOfLines={1}>{item.titulo}</Text>
            {!item.lida && <View style={styles.bolaNaoLida} />}
          </View>
          <Text style={styles.mensagem} numberOfLines={2}>{item.mensagem}</Text>
          <Text style={styles.tempo}>{item.tempoAtras}</Text>
        </View>
      </TouchableOpacity>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={24} color={colors.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitulo}>Notificações</Text>
        {totalNaoLidas > 0 ? (
          <TouchableOpacity onPress={marcarTodasLidas}>
            <Text style={styles.marcarTodas}>Ler todas</Text>
          </TouchableOpacity>
        ) : (
          <View style={{ width: 60 }} />
        )}
      </View>

      {totalNaoLidas > 0 && (
        <View style={styles.badgeBar}>
          <Text style={styles.badgeTexto}>{totalNaoLidas} não lida{totalNaoLidas > 1 ? 's' : ''}</Text>
        </View>
      )}

      <FlatList
        data={notificacoes}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderItem}
        refreshControl={
          <RefreshControl refreshing={carregando} onRefresh={carregarNotificacoes} colors={[colors.primary]} />
        }
        contentContainerStyle={styles.lista}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.vazio}>
              <Ionicons name="notifications-off-outline" size={56} color={colors.textMuted} />
              <Text style={styles.vazioTexto}>Nenhuma notificação</Text>
            </View>
          ) : null
        }
      />
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
  marcarTodas: { fontSize: FontSize.sm, color: colors.primary, fontWeight: '600' },
  badgeBar: {
    backgroundColor: colors.primary + '15',
    paddingHorizontal: Spacing.lg,
    paddingVertical: Spacing.xs,
  },
  badgeTexto: { fontSize: FontSize.sm, color: colors.primary, fontWeight: '600' },
  lista: { paddingVertical: Spacing.sm },
  item: {
    flexDirection: 'row',
    backgroundColor: colors.surface,
    paddingHorizontal: Spacing.lg,
    paddingVertical: Spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
    gap: Spacing.md,
    alignItems: 'flex-start',
  },
  itemNaoLido: { backgroundColor: colors.primary + '06' },
  iconContainer: {
    width: 44,
    height: 44,
    borderRadius: 22,
    justifyContent: 'center',
    alignItems: 'center',
  },
  conteudo: { flex: 1 },
  topoItem: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginBottom: 2 },
  titulo: { fontSize: FontSize.md, fontWeight: '700', color: colors.textPrimary, flex: 1 },
  bolaNaoLida: {
    width: 8, height: 8, borderRadius: 4, backgroundColor: colors.primary, marginLeft: 6,
  },
  mensagem: { fontSize: FontSize.sm, color: colors.textSecondary, lineHeight: 18, marginBottom: 4 },
  tempo: { fontSize: FontSize.xs, color: colors.textMuted },
  vazio: { alignItems: 'center', marginTop: 100 },
  vazioTexto: { fontSize: FontSize.md, color: colors.textMuted, marginTop: Spacing.md },
});
