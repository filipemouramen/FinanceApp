import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  RefreshControl,
  Alert,
  Modal,
  TextInput,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import api from '../../api/client';
import { Resultado } from '../../types';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { useTheme } from '../../theme/useTheme';
import { formatarMoeda } from '../../utils/formatters';

interface LancamentoMetaResponse {
  id: number;
  valor: number;
  observacoes?: string;
  criadoEm: string;
}

interface MetaResponse {
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

const CORES = ['#7C73FF', '#1ABC9C', '#F39C12', '#E74C3C', '#3498DB', '#27AE60', '#8A05BE', '#FF4757'];

export default function MetasScreen() {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const [metas, setMetas] = useState<MetaResponse[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [metaSelecionada, setMetaSelecionada] = useState<MetaResponse | null>(null);

  const [modalNovaMeta, setModalNovaMeta] = useState(false);
  const [modalAporte, setModalAporte] = useState(false);

  const [titulo, setTitulo] = useState('');
  const [valorAlvoCentavos, setValorAlvoCentavos] = useState(0);
  const [valorAlvoDisplay, setValorAlvoDisplay] = useState('');
  const [dataLimite, setDataLimite] = useState('');
  const [corSelecionada, setCorSelecionada] = useState(CORES[0]);
  const [salvando, setSalvando] = useState(false);

  const [valorAporteCentavos, setValorAporteCentavos] = useState(0);
  const [valorAporteDisplay, setValorAporteDisplay] = useState('');
  const [obsAporte, setObsAporte] = useState('');

  const styles = getStyles(colors);

  function centavosParaDisplay(centavos: number): string {
    if (centavos === 0) return '';
    const reais = Math.floor(centavos / 100);
    const cents = centavos % 100;
    return `${reais.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.')},${cents.toString().padStart(2, '0')}`;
  }

  function handleValorAlvoChange(text: string) {
    const digits = text.replace(/\D/g, '');
    const centavos = parseInt(digits || '0', 10);
    setValorAlvoCentavos(centavos);
    setValorAlvoDisplay(centavosParaDisplay(centavos));
  }

  function handleValorAporteChange(text: string) {
    const digits = text.replace(/\D/g, '');
    const centavos = parseInt(digits || '0', 10);
    setValorAporteCentavos(centavos);
    setValorAporteDisplay(centavosParaDisplay(centavos));
  }

  useFocusEffect(
    useCallback(() => {
      carregarMetas();
    }, [])
  );

  async function carregarMetas() {
    try {
      setCarregando(true);
      const res = await api.get('/metas');
      const resultado: Resultado<MetaResponse[]> = res.data;
      if (resultado.sucesso && resultado.dados) setMetas(resultado.dados);
    } catch {
      console.log('Erro ao carregar metas.');
    } finally {
      setCarregando(false);
    }
  }

  async function handleCriarMeta() {
    if (!titulo.trim()) { Alert.alert('Atenção', 'Informe o título da meta.'); return; }
    if (valorAlvoCentavos <= 0) { Alert.alert('Atenção', 'Informe um valor alvo válido.'); return; }

    setSalvando(true);
    try {
      const body: any = { titulo: titulo.trim(), valorAlvo: valorAlvoCentavos / 100, cor: corSelecionada };
      if (dataLimite.length === 10) {
        const [d, m, a] = dataLimite.split('/');
        if (d && m && a) body.dataLimite = `${a}-${m}-${d}`;
      }
      const res = await api.post('/metas', body);
      const resultado: Resultado<MetaResponse> = res.data;
      if (resultado.sucesso) {
        setModalNovaMeta(false);
        resetFormMeta();
        carregarMetas();
      } else {
        Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao criar meta.');
      }
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível criar a meta.');
    } finally {
      setSalvando(false);
    }
  }

  async function handleAporte() {
    if (!metaSelecionada) return;
    if (valorAporteCentavos <= 0) { Alert.alert('Atenção', 'Informe um valor de aporte válido.'); return; }

    setSalvando(true);
    try {
      const res = await api.post(`/metas/${metaSelecionada.id}/lancamentos`, {
        valor: valorAporteCentavos / 100,
        observacoes: obsAporte.trim() || null,
      });
      const resultado: Resultado<MetaResponse> = res.data;
      if (resultado.sucesso) {
        Alert.alert('Sucesso', resultado.mensagem || 'Aporte registrado!');
        setModalAporte(false);
        setValorAporteCentavos(0);
        setValorAporteDisplay('');
        setObsAporte('');
        setMetaSelecionada(null);
        carregarMetas();
      } else {
        Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao registrar aporte.');
      }
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível registrar o aporte.');
    } finally {
      setSalvando(false);
    }
  }

  async function excluirMeta(meta: MetaResponse) {
    Alert.alert(
      'Excluir meta',
      `Deseja excluir a meta "${meta.titulo}"?`,
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Excluir',
          style: 'destructive',
          onPress: async () => {
            try {
              await api.delete(`/metas/${meta.id}`);
              carregarMetas();
            } catch {
              Alert.alert('Erro', 'Não foi possível excluir a meta.');
            }
          },
        },
      ]
    );
  }

  function resetFormMeta() {
    setTitulo('');
    setValorAlvoCentavos(0);
    setValorAlvoDisplay('');
    setDataLimite('');
    setCorSelecionada(CORES[0]);
  }

  function abrirAporte(meta: MetaResponse) {
    setMetaSelecionada(meta);
    setModalAporte(true);
  }

  function renderMeta({ item }: { item: MetaResponse }) {
    const percent = Math.min(item.percentualConcluido, 100);
    const cor = item.cor || colors.primary;
    const expirada = item.diasRestantes !== null && item.diasRestantes !== undefined && item.diasRestantes < 0;

    return (
      <TouchableOpacity
        style={[styles.card, item.concluida && styles.cardConcluido]}
        onPress={() => !item.concluida && abrirAporte(item)}
        onLongPress={() => excluirMeta(item)}
        activeOpacity={0.85}
      >
        <View style={styles.cardTopo}>
          <View style={[styles.iconBg, { backgroundColor: cor + '25' }]}>
            <Ionicons name={(item.icone as any) || 'trophy-outline'} size={20} color={cor} />
          </View>
          <View style={styles.cardInfo}>
            <Text style={styles.metaTitulo} numberOfLines={1}>{item.titulo}</Text>
            {item.concluida ? (
              <Text style={[styles.metaStatus, { color: colors.success }]}>✓ Meta concluída!</Text>
            ) : expirada ? (
              <Text style={[styles.metaStatus, { color: colors.danger }]}>Prazo expirado</Text>
            ) : item.diasRestantes !== null && item.diasRestantes !== undefined ? (
              <Text style={styles.metaStatus}>{item.diasRestantes} dia{item.diasRestantes !== 1 ? 's' : ''} restante{item.diasRestantes !== 1 ? 's' : ''}</Text>
            ) : null}
          </View>
          {!item.concluida && (
            <TouchableOpacity style={[styles.aporteBotao, { backgroundColor: cor }]} onPress={() => abrirAporte(item)}>
              <Ionicons name="add" size={18} color="#fff" />
            </TouchableOpacity>
          )}
        </View>

        <View style={styles.barraFundo}>
          <View style={[styles.barraPreenchida, { width: `${percent}%` as any, backgroundColor: cor }]} />
        </View>

        <View style={styles.valoresRow}>
          <Text style={styles.valorAtual}>{formatarMoeda(item.valorAtual)}</Text>
          <Text style={styles.slash}>/</Text>
          <Text style={styles.valorAlvo}>{formatarMoeda(item.valorAlvo)}</Text>
          <Text style={[styles.percentual, { color: cor }]}>{percent.toFixed(0)}%</Text>
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
        <Text style={styles.headerTitulo}>Metas de Economia</Text>
        <TouchableOpacity style={styles.addBotao} onPress={() => setModalNovaMeta(true)}>
          <Ionicons name="add" size={22} color="#fff" />
        </TouchableOpacity>
      </View>

      <FlatList
        data={metas}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderMeta}
        refreshControl={
          <RefreshControl refreshing={carregando} onRefresh={carregarMetas} colors={[colors.primary]} />
        }
        contentContainerStyle={styles.lista}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.vazio}>
              <Ionicons name="trophy-outline" size={56} color={colors.textMuted} />
              <Text style={styles.vazioTexto}>Nenhuma meta criada</Text>
              <Text style={styles.vazioSub}>Toque em + para criar sua primeira meta</Text>
            </View>
          ) : null
        }
      />

      {/* Modal nova meta */}
      <Modal visible={modalNovaMeta} animationType="slide" transparent>
        <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'} style={{ flex: 1 }}>
          <View style={styles.modalOverlay}>
            <ScrollView keyboardShouldPersistTaps="handled" showsVerticalScrollIndicator={false}>
              <View style={styles.modalContainer}>
                <View style={styles.modalHeader}>
                  <Text style={styles.modalTitulo}>Nova Meta</Text>
                  <TouchableOpacity onPress={() => { setModalNovaMeta(false); resetFormMeta(); }}>
                    <Ionicons name="close" size={24} color={colors.textPrimary} />
                  </TouchableOpacity>
                </View>

                <Text style={styles.label}>Título</Text>
                <TextInput style={styles.input} placeholder="Ex: Viagem, Reserva de emergência..." placeholderTextColor={colors.textMuted} value={titulo} onChangeText={setTitulo} returnKeyType="next" />

                <Text style={styles.label}>Valor alvo (R$)</Text>
                <TextInput style={styles.input} placeholder="0,00" placeholderTextColor={colors.textMuted} keyboardType="number-pad" value={valorAlvoDisplay} onChangeText={handleValorAlvoChange} />

                <Text style={styles.label}>Prazo (DD/MM/AAAA) — opcional</Text>
                <TextInput style={styles.input} placeholder="31/12/2026" placeholderTextColor={colors.textMuted} keyboardType="numeric" maxLength={10} value={dataLimite} onChangeText={setDataLimite} returnKeyType="done" />

                <Text style={styles.label}>Cor</Text>
                <View style={styles.coresRow}>
                  {CORES.map((c) => (
                    <TouchableOpacity
                      key={c}
                      style={[styles.corChip, { backgroundColor: c }, corSelecionada === c && styles.corChipSelecionado]}
                      onPress={() => setCorSelecionada(c)}
                    />
                  ))}
                </View>

                <TouchableOpacity
                  style={[styles.botaoSalvar, salvando && styles.botaoDisabled]}
                  onPress={handleCriarMeta}
                  disabled={salvando}
                >
                  <Text style={styles.botaoSalvarTexto}>{salvando ? 'Criando...' : 'Criar Meta'}</Text>
                </TouchableOpacity>
              </View>
            </ScrollView>
          </View>
        </KeyboardAvoidingView>
      </Modal>

      {/* Modal aporte */}
      <Modal visible={modalAporte} animationType="slide" transparent>
        <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'} style={{ flex: 1 }}>
          <View style={styles.modalOverlay}>
            <ScrollView keyboardShouldPersistTaps="handled" showsVerticalScrollIndicator={false}>
              <View style={styles.modalContainer}>
                <View style={styles.modalHeader}>
                  <Text style={styles.modalTitulo}>Registrar Aporte</Text>
                  <TouchableOpacity onPress={() => { setModalAporte(false); setValorAporteCentavos(0); setValorAporteDisplay(''); setObsAporte(''); setMetaSelecionada(null); }}>
                    <Ionicons name="close" size={24} color={colors.textPrimary} />
                  </TouchableOpacity>
                </View>

                {metaSelecionada && (
                  <View style={styles.metaResumo}>
                    <Text style={styles.metaResumoTitulo}>{metaSelecionada.titulo}</Text>
                    <Text style={styles.metaResumoDetalhe}>
                      Faltam {formatarMoeda(metaSelecionada.valorRestante)} para completar ({metaSelecionada.percentualConcluido.toFixed(0)}% concluído)
                    </Text>
                  </View>
                )}

                <Text style={styles.label}>Valor do aporte (R$)</Text>
                <TextInput
                  style={styles.input}
                  placeholder="0,00"
                  placeholderTextColor={colors.textMuted}
                  keyboardType="number-pad"
                  value={valorAporteDisplay}
                  onChangeText={handleValorAporteChange}
                  autoFocus
                />

                <Text style={styles.label}>Observações (opcional)</Text>
                <TextInput
                  style={styles.input}
                  placeholder="Ex: Bônus de março..."
                  placeholderTextColor={colors.textMuted}
                  value={obsAporte}
                  onChangeText={setObsAporte}
                  returnKeyType="done"
                />

                <TouchableOpacity
                  style={[styles.botaoSalvar, salvando && styles.botaoDisabled]}
                  onPress={handleAporte}
                  disabled={salvando}
                >
                  <Text style={styles.botaoSalvarTexto}>{salvando ? 'Registrando...' : 'Confirmar Aporte'}</Text>
                </TouchableOpacity>
              </View>
            </ScrollView>
          </View>
        </KeyboardAvoidingView>
      </Modal>
    </View>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  header: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    paddingHorizontal: Spacing.lg, paddingTop: 60, paddingBottom: Spacing.md,
    backgroundColor: colors.surface, borderBottomWidth: 1, borderBottomColor: colors.borderLight,
  },
  headerTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  addBotao: {
    backgroundColor: colors.primary, width: 38, height: 38, borderRadius: 19,
    justifyContent: 'center', alignItems: 'center',
  },
  lista: { padding: Spacing.md, paddingBottom: 100 },
  card: {
    backgroundColor: colors.surface, borderRadius: BorderRadius.lg, padding: Spacing.lg,
    marginBottom: Spacing.md,
    shadowColor: '#000', shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.06, shadowRadius: 6, elevation: 3,
  },
  cardConcluido: { opacity: 0.7 },
  cardTopo: { flexDirection: 'row', alignItems: 'center', gap: Spacing.sm, marginBottom: Spacing.md },
  iconBg: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  cardInfo: { flex: 1 },
  metaTitulo: { fontSize: FontSize.md, fontWeight: '700', color: colors.textPrimary },
  metaStatus: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 2 },
  aporteBotao: { width: 32, height: 32, borderRadius: 16, justifyContent: 'center', alignItems: 'center' },
  barraFundo: { height: 8, backgroundColor: colors.borderLight, borderRadius: 4, overflow: 'hidden', marginBottom: Spacing.sm },
  barraPreenchida: { height: '100%', borderRadius: 4 },
  valoresRow: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  valorAtual: { fontSize: FontSize.md, fontWeight: '700', color: colors.textPrimary },
  slash: { fontSize: FontSize.md, color: colors.textMuted },
  valorAlvo: { fontSize: FontSize.md, color: colors.textSecondary },
  percentual: { marginLeft: 'auto', fontSize: FontSize.md, fontWeight: '700' },
  vazio: { alignItems: 'center', marginTop: 100 },
  vazioTexto: { fontSize: FontSize.lg, color: colors.textSecondary, marginTop: Spacing.md, fontWeight: '600' },
  vazioSub: { fontSize: FontSize.md, color: colors.textMuted, marginTop: Spacing.xs },
  modalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' },
  modalContainer: {
    backgroundColor: colors.surface, borderTopLeftRadius: 20, borderTopRightRadius: 20,
    padding: Spacing.xl, paddingBottom: 40,
  },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: Spacing.lg },
  modalTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  label: { fontSize: FontSize.sm, fontWeight: '600', color: colors.textSecondary, marginBottom: Spacing.xs, marginTop: Spacing.md, textTransform: 'uppercase', letterSpacing: 0.5 },
  input: {
    backgroundColor: colors.surfaceVariant, borderRadius: BorderRadius.sm, borderWidth: 1.5,
    borderColor: colors.border, padding: Spacing.md, fontSize: FontSize.md, color: colors.textPrimary,
  },
  coresRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 10, marginTop: 4 },
  corChip: { width: 32, height: 32, borderRadius: 16 },
  corChipSelecionado: { borderWidth: 3, borderColor: colors.textPrimary },
  metaResumo: {
    backgroundColor: colors.primaryLight, borderRadius: BorderRadius.md, padding: Spacing.md, marginBottom: Spacing.md,
  },
  metaResumoTitulo: { fontSize: FontSize.md, fontWeight: '700', color: colors.primary },
  metaResumoDetalhe: { fontSize: FontSize.sm, color: colors.primary, opacity: 0.8, marginTop: 2 },
  botaoSalvar: {
    backgroundColor: colors.primary, borderRadius: BorderRadius.sm, padding: Spacing.md,
    alignItems: 'center', marginTop: Spacing.xl,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoSalvarTexto: { color: '#fff', fontSize: FontSize.lg, fontWeight: '700' },
});
