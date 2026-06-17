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
import { CartaoCreditoResponse, Resultado } from '../../types';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { useTheme } from '../../theme/useTheme';
import { formatarMoeda } from '../../utils/formatters';

const BANDEIRAS = ['Visa', 'Mastercard', 'Elo', 'Hipercard', 'American Express', 'Outro'];
const CORES_CARTAO = ['#FF4757', '#8A05BE', '#2C3E50', '#1ABC9C', '#F39C12', '#E74C3C', '#3498DB', '#27AE60'];

function centavosParaDisplay(centavos: number): string {
  if (centavos === 0) return '';
  const reais = Math.floor(centavos / 100);
  const cents = centavos % 100;
  const reaisStr = reais.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  return `${reaisStr},${cents.toString().padStart(2, '0')}`;
}

export default function CartoesScreen() {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const [cartoes, setCartoes] = useState<CartaoCreditoResponse[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [salvando, setSalvando] = useState(false);

  const [modalEditar, setModalEditar] = useState(false);
  const [cartaoEditando, setCartaoEditando] = useState<CartaoCreditoResponse | null>(null);
  const [editNome, setEditNome] = useState('');
  const [editBandeira, setEditBandeira] = useState('');
  const [editLimiteCentavos, setEditLimiteCentavos] = useState(0);
  const [editLimiteDisplay, setEditLimiteDisplay] = useState('');
  const [editDiaFechamento, setEditDiaFechamento] = useState('');
  const [editDiaVencimento, setEditDiaVencimento] = useState('');
  const [editCor, setEditCor] = useState('#FF4757');

  const styles = getStyles(colors);

  function handleEditLimiteChange(text: string) {
    const digits = text.replace(/\D/g, '');
    const centavos = parseInt(digits || '0', 10);
    setEditLimiteCentavos(centavos);
    setEditLimiteDisplay(centavosParaDisplay(centavos));
  }

  useFocusEffect(
    useCallback(() => {
      carregarCartoes();
    }, [])
  );

  async function carregarCartoes() {
    try {
      setCarregando(true);
      const response = await api.get('/cartoes');
      const resultado: Resultado<CartaoCreditoResponse[]> = response.data;
      if (resultado.sucesso && resultado.dados) setCartoes(resultado.dados);
    } catch {
      console.log('Erro ao carregar cartões.');
    } finally {
      setCarregando(false);
    }
  }

  function abrirEditar(cartao: CartaoCreditoResponse) {
    setCartaoEditando(cartao);
    setEditNome(cartao.nome);
    setEditBandeira(cartao.bandeira || '');
    const centavos = Math.round(cartao.limiteTotal * 100);
    setEditLimiteCentavos(centavos);
    setEditLimiteDisplay(centavosParaDisplay(centavos));
    setEditDiaFechamento(cartao.diaFechamento.toString());
    setEditDiaVencimento(cartao.diaVencimento.toString());
    setEditCor(cartao.cor);
    setModalEditar(true);
  }

  function fecharEditar() {
    setModalEditar(false);
    setCartaoEditando(null);
  }

  async function salvarEdicao() {
    if (!cartaoEditando) return;
    if (!editNome.trim()) { Alert.alert('Atenção', 'Informe o nome do cartão.'); return; }
    if (editLimiteCentavos <= 0) { Alert.alert('Atenção', 'Informe um limite válido.'); return; }
    const fechamento = parseInt(editDiaFechamento);
    if (!fechamento || fechamento < 1 || fechamento > 30) {
      Alert.alert('Atenção', 'Dia de fechamento deve ser entre 1 e 30.');
      return;
    }
    const vencimento = parseInt(editDiaVencimento);
    if (!vencimento || vencimento < 1 || vencimento > 30) {
      Alert.alert('Atenção', 'Dia de vencimento deve ser entre 1 e 30.');
      return;
    }

    setSalvando(true);
    try {
      const res = await api.put(`/cartoes/${cartaoEditando.id}`, {
        nome: editNome.trim(),
        bandeira: editBandeira || null,
        limiteTotal: editLimiteCentavos / 100,
        diaFechamento: fechamento,
        diaVencimento: vencimento,
        cor: editCor,
      });
      const resultado: Resultado<CartaoCreditoResponse> = res.data;
      if (resultado.sucesso) {
        Alert.alert('Sucesso', 'Cartão atualizado!');
        fecharEditar();
        carregarCartoes();
      } else {
        Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao atualizar cartão.');
      }
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível atualizar o cartão.');
    } finally {
      setSalvando(false);
    }
  }

  function opcoesCartao(cartao: CartaoCreditoResponse) {
    Alert.alert(cartao.nome, 'O que deseja fazer?', [
      { text: 'Editar', onPress: () => abrirEditar(cartao) },
      {
        text: 'Ver faturas',
        onPress: () => navigation.navigate('FaturaDetalhe', { cartaoId: cartao.id, nomeCartao: cartao.nome }),
      },
      {
        text: 'Excluir',
        style: 'destructive',
        onPress: () => excluirCartao(cartao),
      },
      { text: 'Cancelar', style: 'cancel' },
    ]);
  }

  async function excluirCartao(cartao: CartaoCreditoResponse) {
    Alert.alert(
      'Excluir cartão',
      `Deseja excluir "${cartao.nome}"?\n\nSó é possível excluir se não houver faturas abertas.`,
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Excluir',
          style: 'destructive',
          onPress: async () => {
            try {
              const res = await api.delete(`/cartoes/${cartao.id}`);
              const resultado: Resultado<boolean> = res.data;
              Alert.alert('', resultado.mensagem || 'Cartão excluído.');
              carregarCartoes();
            } catch (e: any) {
              Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível excluir.');
            }
          },
        },
      ]
    );
  }

  function barraLimite(percentual: number) {
    const cor = percentual >= 90 ? colors.danger : percentual >= 70 ? colors.warning : colors.success;
    return (
      <View style={styles.barraContainer}>
        <View style={[styles.barraPreenchida, { width: `${Math.min(percentual, 100)}%` as any, backgroundColor: cor }]} />
      </View>
    );
  }

  function renderCartao({ item }: { item: CartaoCreditoResponse }) {
    const percentual = item.percentualUtilizado ?? 0;
    return (
      <TouchableOpacity
        style={styles.cartaoCard}
        onPress={() => navigation.navigate('FaturaDetalhe', { cartaoId: item.id, nomeCartao: item.nome })}
        onLongPress={() => opcoesCartao(item)}
        activeOpacity={0.85}
      >
        <View style={[styles.cartaoHeader, { backgroundColor: item.cor }]}>
          <View>
            <Text style={styles.cartaoNome}>{item.nome}</Text>
            {item.bandeira && <Text style={styles.cartaoBandeira}>{item.bandeira}</Text>}
          </View>
          <TouchableOpacity onPress={() => opcoesCartao(item)} hitSlop={{ top: 10, bottom: 10, left: 10, right: 10 }}>
            <Ionicons name="ellipsis-horizontal" size={22} color="rgba(255,255,255,0.85)" />
          </TouchableOpacity>
        </View>

        <View style={styles.cartaoBody}>
          <View style={styles.limiteRow}>
            <View>
              <Text style={styles.limiteLabel}>Limite disponível</Text>
              <Text style={[styles.limiteValor, { color: colors.success }]}>
                {formatarMoeda(item.limiteDisponivel)}
              </Text>
            </View>
            <View style={{ alignItems: 'flex-end' }}>
              <Text style={styles.limiteLabel}>Limite total</Text>
              <Text style={styles.limiteTotalValor}>{formatarMoeda(item.limiteTotal)}</Text>
            </View>
          </View>

          {barraLimite(percentual)}
          <Text style={styles.percentualTexto}>
            {percentual.toFixed(0)}% utilizado ({formatarMoeda(item.limiteUtilizado ?? 0)} de {formatarMoeda(item.limiteTotal)})
          </Text>

          <View style={styles.infoRow}>
            <View style={styles.infoItem}>
              <Ionicons name="calendar-outline" size={14} color={colors.textMuted} />
              <Text style={styles.infoTexto}>Fecha dia {item.diaFechamento}</Text>
            </View>
            <View style={styles.infoItem}>
              <Ionicons name="time-outline" size={14} color={colors.textMuted} />
              <Text style={styles.infoTexto}>Vence dia {item.diaVencimento}</Text>
            </View>
          </View>
        </View>
      </TouchableOpacity>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.headerTitulo}>Cartões de Crédito</Text>
        <TouchableOpacity
          style={styles.botaoAdicionar}
          onPress={() => navigation.navigate('CriarCartao')}
        >
          <Ionicons name="add" size={24} color={colors.textWhite} />
        </TouchableOpacity>
      </View>

      <FlatList
        data={cartoes}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderCartao}
        refreshControl={
          <RefreshControl refreshing={carregando} onRefresh={carregarCartoes} colors={[colors.primary]} />
        }
        contentContainerStyle={styles.lista}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.vazio}>
              <Ionicons name="card-outline" size={64} color={colors.textMuted} />
              <Text style={styles.vazioTexto}>Nenhum cartão cadastrado</Text>
              <Text style={styles.vazioSub}>Toque em + para adicionar</Text>
            </View>
          ) : null
        }
      />

      {/* Modal editar cartão */}
      <Modal visible={modalEditar} animationType="slide" transparent>
        <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'} style={{ flex: 1 }}>
          <View style={styles.modalOverlay}>
            <View style={styles.modalContainer}>
              <ScrollView showsVerticalScrollIndicator={false} keyboardShouldPersistTaps="handled">
                <View style={styles.modalHeader}>
                  <Text style={styles.modalTitulo}>Editar Cartão</Text>
                  <TouchableOpacity onPress={fecharEditar}>
                    <Ionicons name="close" size={24} color={colors.textSecondary} />
                  </TouchableOpacity>
                </View>

                {/* Preview */}
                <View style={[styles.cartaoPreview, { backgroundColor: editCor }]}>
                  <Text style={styles.previewNome}>{editNome || 'Nome do cartão'}</Text>
                  <Text style={styles.previewBandeira}>{editBandeira || 'Bandeira'}</Text>
                  <Text style={styles.previewLimite}>
                    Limite: {formatarMoeda(editLimiteCentavos / 100)}
                  </Text>
                </View>

                <Text style={styles.label}>Nome do cartão</Text>
                <TextInput
                  style={styles.input}
                  placeholder="Ex: Nubank Roxinho"
                  placeholderTextColor={colors.textMuted}
                  value={editNome}
                  onChangeText={setEditNome}
                />

                <Text style={styles.label}>Bandeira</Text>
                <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chips}>
                  {BANDEIRAS.map((b) => (
                    <TouchableOpacity
                      key={b}
                      style={[styles.chip, editBandeira === b && styles.chipSelecionado]}
                      onPress={() => setEditBandeira(b === editBandeira ? '' : b)}
                    >
                      <Text style={[styles.chipTexto, editBandeira === b && styles.chipTextoSelecionado]}>{b}</Text>
                    </TouchableOpacity>
                  ))}
                </ScrollView>

                <Text style={styles.label}>Limite total (R$)</Text>
                <TextInput
                  style={styles.input}
                  placeholder="0,00"
                  placeholderTextColor={colors.textMuted}
                  keyboardType="number-pad"
                  value={editLimiteDisplay}
                  onChangeText={handleEditLimiteChange}
                />

                <View style={styles.duploCampo}>
                  <View style={styles.campoMeio}>
                    <Text style={styles.label}>Dia fechamento</Text>
                    <TextInput
                      style={styles.input}
                      placeholder="1-30"
                      placeholderTextColor={colors.textMuted}
                      keyboardType="number-pad"
                      maxLength={2}
                      value={editDiaFechamento}
                      onChangeText={setEditDiaFechamento}
                    />
                  </View>
                  <View style={styles.campoMeio}>
                    <Text style={styles.label}>Dia vencimento</Text>
                    <TextInput
                      style={styles.input}
                      placeholder="1-30"
                      placeholderTextColor={colors.textMuted}
                      keyboardType="number-pad"
                      maxLength={2}
                      value={editDiaVencimento}
                      onChangeText={setEditDiaVencimento}
                    />
                  </View>
                </View>

                <Text style={styles.label}>Cor</Text>
                <View style={styles.coresRow}>
                  {CORES_CARTAO.map((c) => (
                    <TouchableOpacity
                      key={c}
                      style={[styles.corChip, { backgroundColor: c }, editCor === c && styles.corChipSelecionado]}
                      onPress={() => setEditCor(c)}
                    />
                  ))}
                </View>

                <TouchableOpacity
                  style={[styles.botaoSalvar, salvando && styles.botaoDisabled]}
                  onPress={salvarEdicao}
                  disabled={salvando}
                >
                  <Text style={styles.botaoSalvarTexto}>
                    {salvando ? 'Salvando...' : 'Salvar Alterações'}
                  </Text>
                </TouchableOpacity>
              </ScrollView>
            </View>
          </View>
        </KeyboardAvoidingView>
      </Modal>
    </View>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  header: {
    flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center',
    paddingHorizontal: Spacing.lg, paddingTop: 60, paddingBottom: Spacing.md,
    backgroundColor: colors.surface, borderBottomWidth: 1, borderBottomColor: colors.borderLight,
  },
  headerTitulo: { fontSize: FontSize.xxl, fontWeight: '700', color: colors.textPrimary },
  botaoAdicionar: {
    backgroundColor: colors.primary, width: 44, height: 44,
    borderRadius: 22, justifyContent: 'center', alignItems: 'center',
  },
  lista: { padding: Spacing.md, paddingBottom: 100 },
  cartaoCard: {
    backgroundColor: colors.surface, borderRadius: BorderRadius.xl,
    marginBottom: Spacing.md, overflow: 'hidden',
    shadowColor: '#000', shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.1, shadowRadius: 8, elevation: 5,
  },
  cartaoHeader: {
    flexDirection: 'row', justifyContent: 'space-between',
    alignItems: 'center', padding: Spacing.lg, paddingBottom: Spacing.xl,
  },
  cartaoNome: { fontSize: FontSize.xl, fontWeight: '700', color: '#fff' },
  cartaoBandeira: { fontSize: FontSize.sm, color: 'rgba(255,255,255,0.75)', marginTop: 2 },
  cartaoBody: { padding: Spacing.lg },
  limiteRow: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: Spacing.sm },
  limiteLabel: { fontSize: FontSize.xs, color: colors.textMuted, marginBottom: 2 },
  limiteValor: { fontSize: FontSize.lg, fontWeight: '700' },
  limiteTotalValor: { fontSize: FontSize.lg, fontWeight: '600', color: colors.textPrimary },
  barraContainer: {
    height: 6, backgroundColor: colors.borderLight,
    borderRadius: 3, overflow: 'hidden', marginBottom: 4,
  },
  barraPreenchida: { height: '100%', borderRadius: 3 },
  percentualTexto: { fontSize: FontSize.xs, color: colors.textMuted, marginBottom: Spacing.sm },
  infoRow: { flexDirection: 'row', gap: Spacing.md },
  infoItem: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  infoTexto: { fontSize: FontSize.xs, color: colors.textMuted },
  vazio: { alignItems: 'center', marginTop: 100 },
  vazioTexto: { fontSize: FontSize.lg, color: colors.textSecondary, marginTop: Spacing.md, fontWeight: '600' },
  vazioSub: { fontSize: FontSize.md, color: colors.textMuted, marginTop: Spacing.xs },
  // Modal
  modalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' },
  modalContainer: {
    backgroundColor: colors.surface, borderTopLeftRadius: 20, borderTopRightRadius: 20,
    padding: Spacing.lg, maxHeight: '92%',
  },
  modalHeader: {
    flexDirection: 'row', justifyContent: 'space-between',
    alignItems: 'center', marginBottom: Spacing.lg,
  },
  modalTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  cartaoPreview: {
    borderRadius: BorderRadius.xl, padding: Spacing.xl,
    marginBottom: Spacing.md, minHeight: 110, justifyContent: 'space-between',
  },
  previewNome: { fontSize: FontSize.xl, fontWeight: '700', color: '#fff' },
  previewBandeira: { fontSize: FontSize.sm, color: 'rgba(255,255,255,0.75)', marginTop: 4 },
  previewLimite: { fontSize: FontSize.md, color: 'rgba(255,255,255,0.9)', fontWeight: '600' },
  label: {
    fontSize: FontSize.sm, fontWeight: '600', color: colors.textSecondary,
    marginBottom: Spacing.xs, marginTop: Spacing.md, textTransform: 'uppercase', letterSpacing: 0.5,
  },
  input: {
    backgroundColor: colors.surfaceVariant, borderRadius: BorderRadius.sm,
    borderWidth: 1.5, borderColor: colors.border,
    padding: Spacing.md, fontSize: FontSize.md, color: colors.textPrimary,
  },
  duploCampo: { flexDirection: 'row', gap: Spacing.md },
  campoMeio: { flex: 1 },
  chips: { marginBottom: 4 },
  chip: {
    paddingHorizontal: Spacing.md, paddingVertical: 6,
    borderRadius: BorderRadius.full, borderWidth: 1.5, borderColor: colors.border,
    marginRight: Spacing.xs, backgroundColor: colors.surface,
  },
  chipSelecionado: { borderColor: colors.primary, backgroundColor: colors.primary + '15' },
  chipTexto: { fontSize: FontSize.sm, color: colors.textSecondary },
  chipTextoSelecionado: { color: colors.primary, fontWeight: '600' },
  coresRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 10, marginTop: 4 },
  corChip: { width: 32, height: 32, borderRadius: 16 },
  corChipSelecionado: { borderWidth: 3, borderColor: colors.textPrimary },
  botaoSalvar: {
    backgroundColor: colors.primary, borderRadius: BorderRadius.sm,
    padding: Spacing.md, alignItems: 'center', marginTop: Spacing.xl, marginBottom: Spacing.md,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoSalvarTexto: { color: '#fff', fontSize: FontSize.lg, fontWeight: '700' },
});
