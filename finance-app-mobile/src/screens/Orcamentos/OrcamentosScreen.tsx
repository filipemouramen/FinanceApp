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
  ScrollView,
  KeyboardAvoidingView,
  Platform,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import api from '../../api/client';
import { CategoriaResponse, Resultado } from '../../types';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { useTheme } from '../../theme/useTheme';
import { formatarMoeda } from '../../utils/formatters';

interface OrcamentoResponse {
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
  status: string;
  mes: number;
  ano: number;
}

const NOMES_MESES = ['', 'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

export default function OrcamentosScreen() {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const hoje = new Date();
  const [mes, setMes] = useState(hoje.getMonth() + 1);
  const [ano, setAno] = useState(hoje.getFullYear());
  const [orcamentos, setOrcamentos] = useState<OrcamentoResponse[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [modalVisivel, setModalVisivel] = useState(false);
  const [categorias, setCategorias] = useState<CategoriaResponse[]>([]);
  const [salvando, setSalvando] = useState(false);

  const styles = getStyles(colors);

  // Form state
  const [categoriaId, setCategoriaId] = useState<number | null>(null);
  const [valorLimiteCentavos, setValorLimiteCentavos] = useState(0);
  const [valorLimiteDisplay, setValorLimiteDisplay] = useState('');
  const [percentualAlerta, setPercentualAlerta] = useState('80');

  function centavosParaDisplay(centavos: number): string {
    if (centavos === 0) return '';
    const reais = Math.floor(centavos / 100);
    const cents = centavos % 100;
    return `${reais.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.')},${cents.toString().padStart(2, '0')}`;
  }

  function handleValorLimiteChange(text: string) {
    const digits = text.replace(/\D/g, '');
    const centavos = parseInt(digits || '0', 10);
    setValorLimiteCentavos(centavos);
    setValorLimiteDisplay(centavosParaDisplay(centavos));
  }

  useFocusEffect(
    useCallback(() => {
      carregarOrcamentos(mes, ano);
    }, [mes, ano])
  );

  async function carregarOrcamentos(m: number, a: number) {
    try {
      setCarregando(true);
      const [orcRes, catRes] = await Promise.all([
        api.get(`/orcamentos/${m}/${a}`),
        api.get('/categorias'),
      ]);
      if (orcRes.data.sucesso) setOrcamentos(orcRes.data.dados);
      if (catRes.data.sucesso)
        setCategorias(catRes.data.dados.filter((c: CategoriaResponse) => c.tipo === 'DESPESA'));
    } catch {
      console.log('Erro ao carregar orçamentos.');
    } finally {
      setCarregando(false);
    }
  }

  function navegarMes(direcao: -1 | 1) {
    let novoMes = mes + direcao;
    let novoAno = ano;
    if (novoMes > 12) { novoMes = 1; novoAno++; }
    if (novoMes < 1) { novoMes = 12; novoAno--; }
    setMes(novoMes);
    setAno(novoAno);
  }

  async function handleSalvar() {
    if (!categoriaId) { Alert.alert('Atenção', 'Selecione uma categoria.'); return; }
    if (valorLimiteCentavos <= 0) { Alert.alert('Atenção', 'Informe um valor limite válido.'); return; }

    setSalvando(true);
    try {
      const res = await api.post('/orcamentos', {
        categoriaId,
        valorLimite: valorLimiteCentavos / 100,
        percentualAlerta: parseInt(percentualAlerta) || 80,
        mes,
        ano,
      });
      const resultado: Resultado<OrcamentoResponse> = res.data;
      if (resultado.sucesso) {
        setModalVisivel(false);
        resetForm();
        carregarOrcamentos(mes, ano);
      } else {
        Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao criar orçamento.');
      }
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível criar o orçamento.');
    } finally {
      setSalvando(false);
    }
  }

  function resetForm() {
    setCategoriaId(null);
    setValorLimiteCentavos(0);
    setValorLimiteDisplay('');
    setPercentualAlerta('80');
  }

  async function excluirOrcamento(id: number, nome: string) {
    Alert.alert(
      'Excluir orçamento',
      `Deseja excluir o orçamento de "${nome}"?`,
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Excluir',
          style: 'destructive',
          onPress: async () => {
            try {
              await api.delete(`/orcamentos/${id}`);
              carregarOrcamentos(mes, ano);
            } catch {
              Alert.alert('Erro', 'Não foi possível excluir o orçamento.');
            }
          },
        },
      ]
    );
  }

  function corStatus(orc: OrcamentoResponse) {
    if (orc.estourado) return colors.danger;
    if (orc.emAlerta) return colors.warning;
    return colors.success;
  }

  function renderOrcamento({ item }: { item: OrcamentoResponse }) {
    const cor = corStatus(item);
    const percent = Math.min(item.percentualUsado, 100);

    return (
      <TouchableOpacity
        style={styles.card}
        onLongPress={() => excluirOrcamento(item.id, item.nomeCategoria)}
        activeOpacity={0.8}
      >
        <View style={styles.cardTopo}>
          <View style={styles.cardEsq}>
            <View style={[styles.catDot, { backgroundColor: item.corCategoria }]} />
            <Text style={styles.catNome}>{item.nomeCategoria}</Text>
          </View>
          <View style={[styles.statusBadge, { backgroundColor: cor + '20' }]}>
            <Text style={[styles.statusTexto, { color: cor }]}>{item.status}</Text>
          </View>
        </View>

        <View style={styles.barraFundo}>
          <View style={[styles.barraPreenchida, { width: `${percent}%` as any, backgroundColor: cor }]} />
        </View>

        <View style={styles.valoresRow}>
          <Text style={styles.totalGasto}>{formatarMoeda(item.totalGasto)}</Text>
          <Text style={styles.slash}>/</Text>
          <Text style={styles.limite}>{formatarMoeda(item.valorLimite)}</Text>
          <Text style={[styles.percentual, { color: cor }]}>{item.percentualUsado.toFixed(0)}%</Text>
        </View>

        {item.estourado ? (
          <Text style={styles.alertaTexto}>⚠️ Orçamento estourado em {formatarMoeda(Math.abs(item.valorRestante))}</Text>
        ) : (
          <Text style={styles.restaTexto}>Resta {formatarMoeda(item.valorRestante)} (alerta em {item.percentualAlerta}%)</Text>
        )}
      </TouchableOpacity>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={24} color={colors.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitulo}>Orçamentos</Text>
        <TouchableOpacity style={styles.addBotao} onPress={() => setModalVisivel(true)}>
          <Ionicons name="add" size={22} color="#fff" />
        </TouchableOpacity>
      </View>

      {/* Navegador mês */}
      <View style={styles.mesNav}>
        <TouchableOpacity onPress={() => navegarMes(-1)} style={styles.mesBotao}>
          <Ionicons name="chevron-back" size={20} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.mesTitulo}>{NOMES_MESES[mes]} {ano}</Text>
        <TouchableOpacity onPress={() => navegarMes(1)} style={styles.mesBotao}>
          <Ionicons name="chevron-forward" size={20} color={colors.primary} />
        </TouchableOpacity>
      </View>

      <FlatList
        data={orcamentos}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderOrcamento}
        refreshControl={
          <RefreshControl refreshing={carregando} onRefresh={() => carregarOrcamentos(mes, ano)} colors={[colors.primary]} />
        }
        contentContainerStyle={styles.lista}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.vazio}>
              <Ionicons name="pie-chart-outline" size={56} color={colors.textMuted} />
              <Text style={styles.vazioTexto}>Nenhum orçamento para este mês</Text>
              <Text style={styles.vazioSub}>Toque em + para criar</Text>
            </View>
          ) : null
        }
      />

      {/* Modal criar orçamento */}
      <Modal visible={modalVisivel} animationType="slide" transparent>
        <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'} style={{ flex: 1 }}>
          <View style={styles.modalOverlay}>
            <ScrollView keyboardShouldPersistTaps="handled" showsVerticalScrollIndicator={false}>
              <View style={styles.modalContainer}>
                <View style={styles.modalHeader}>
                  <Text style={styles.modalTitulo}>Novo Orçamento</Text>
                  <TouchableOpacity onPress={() => { setModalVisivel(false); resetForm(); }}>
                    <Ionicons name="close" size={24} color={colors.textPrimary} />
                  </TouchableOpacity>
                </View>

                <Text style={styles.label}>Categoria (despesa)</Text>
                <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chips}>
                  {categorias.map((cat) => (
                    <TouchableOpacity
                      key={cat.id}
                      style={[styles.chip, categoriaId === cat.id && { borderColor: cat.cor, backgroundColor: cat.cor + '15' }]}
                      onPress={() => setCategoriaId(cat.id)}
                    >
                      <View style={[styles.chipDot, { backgroundColor: cat.cor }]} />
                      <Text style={[styles.chipTexto, categoriaId === cat.id && { color: cat.cor, fontWeight: '700' }]}>
                        {cat.nome}
                      </Text>
                    </TouchableOpacity>
                  ))}
                </ScrollView>

                <Text style={styles.label}>Valor limite (R$)</Text>
                <TextInput
                  style={styles.input}
                  placeholder="0,00"
                  placeholderTextColor={colors.textMuted}
                  keyboardType="number-pad"
                  value={valorLimiteDisplay}
                  onChangeText={handleValorLimiteChange}
                />

                <Text style={styles.label}>Alerta em (%) — padrão 80%</Text>
                <TextInput
                  style={styles.input}
                  placeholder="80"
                  placeholderTextColor={colors.textMuted}
                  keyboardType="number-pad"
                  maxLength={3}
                  value={percentualAlerta}
                  onChangeText={setPercentualAlerta}
                  returnKeyType="done"
                />

                <TouchableOpacity
                  style={[styles.botaoSalvar, salvando && styles.botaoDisabled]}
                  onPress={handleSalvar}
                  disabled={salvando}
                >
                  <Text style={styles.botaoSalvarTexto}>{salvando ? 'Salvando...' : 'Criar Orçamento'}</Text>
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
  mesNav: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    paddingVertical: Spacing.sm, backgroundColor: colors.surface,
    borderBottomWidth: 1, borderBottomColor: colors.borderLight,
  },
  mesBotao: { padding: Spacing.sm },
  mesTitulo: { fontSize: FontSize.md, fontWeight: '700', color: colors.textPrimary, minWidth: 180, textAlign: 'center' },
  lista: { padding: Spacing.md, paddingBottom: 100 },
  card: {
    backgroundColor: colors.surface, borderRadius: BorderRadius.lg, padding: Spacing.lg,
    marginBottom: Spacing.md,
    shadowColor: '#000', shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.06, shadowRadius: 6, elevation: 3,
  },
  cardTopo: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: Spacing.md },
  cardEsq: { flexDirection: 'row', alignItems: 'center', gap: 8, flex: 1 },
  catDot: { width: 10, height: 10, borderRadius: 5 },
  catNome: { fontSize: FontSize.md, fontWeight: '700', color: colors.textPrimary },
  statusBadge: { paddingHorizontal: 8, paddingVertical: 3, borderRadius: BorderRadius.sm },
  statusTexto: { fontSize: FontSize.xs, fontWeight: '700' },
  barraFundo: { height: 8, backgroundColor: colors.borderLight, borderRadius: 4, overflow: 'hidden', marginBottom: Spacing.sm },
  barraPreenchida: { height: '100%', borderRadius: 4 },
  valoresRow: { flexDirection: 'row', alignItems: 'center', gap: 4, marginBottom: 4 },
  totalGasto: { fontSize: FontSize.md, fontWeight: '700', color: colors.textPrimary },
  slash: { fontSize: FontSize.md, color: colors.textMuted },
  limite: { fontSize: FontSize.md, color: colors.textSecondary },
  percentual: { marginLeft: 'auto', fontSize: FontSize.md, fontWeight: '700' },
  alertaTexto: { fontSize: FontSize.sm, color: colors.danger, fontWeight: '600' },
  restaTexto: { fontSize: FontSize.xs, color: colors.textMuted },
  vazio: { alignItems: 'center', marginTop: 100 },
  vazioTexto: { fontSize: FontSize.lg, color: colors.textSecondary, marginTop: Spacing.md, fontWeight: '600' },
  vazioSub: { fontSize: FontSize.md, color: colors.textMuted, marginTop: Spacing.xs },
  // Modal
  modalOverlay: {
    flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end',
  },
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
  chips: { marginBottom: 4 },
  chip: {
    flexDirection: 'row', alignItems: 'center', paddingHorizontal: 12, paddingVertical: 8,
    borderRadius: BorderRadius.sm, borderWidth: 1.5, borderColor: colors.border,
    backgroundColor: colors.surface, marginRight: 8, gap: 6,
  },
  chipDot: { width: 8, height: 8, borderRadius: 4 },
  chipTexto: { fontSize: FontSize.sm, color: colors.textSecondary },
  botaoSalvar: {
    backgroundColor: colors.primary, borderRadius: BorderRadius.sm, padding: Spacing.md,
    alignItems: 'center', marginTop: Spacing.xl,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoSalvarTexto: { color: '#fff', fontSize: FontSize.lg, fontWeight: '700' },
});
