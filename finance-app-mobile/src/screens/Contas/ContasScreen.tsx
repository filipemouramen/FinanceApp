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
import { ContaResponse, Resultado } from '../../types';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { useTheme } from '../../theme/useTheme';
import { formatarMoeda } from '../../utils/formatters';
import BancoIcone from '../../components/BancoIcone';

const TIPOS_CONTA = [
  { valor: 'CORRENTE', label: 'Corrente' },
  { valor: 'POUPANCA', label: 'Poupança' },
  { valor: 'CARTEIRA', label: 'Carteira' },
  { valor: 'INVESTIMENTO', label: 'Investimento' },
];

const CORES_CONTA = [
  '#6C63FF', '#8A05BE', '#FF6B6B', '#4ECDC4',
  '#45B7D1', '#27AE60', '#F39C12', '#E74C3C',
];

function centavosParaDisplay(centavos: number): string {
  if (centavos === 0) return '';
  const reais = Math.floor(centavos / 100);
  const cents = centavos % 100;
  const reaisStr = reais.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  return `${reaisStr},${cents.toString().padStart(2, '0')}`;
}

export default function ContasScreen() {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const [contas, setContas] = useState<ContaResponse[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [modalVisivel, setModalVisivel] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [contaEditando, setContaEditando] = useState<ContaResponse | null>(null);

  const [nome, setNome] = useState('');
  const [banco, setBanco] = useState('');
  const [tipoConta, setTipoConta] = useState('CORRENTE');
  const [saldoInicialCentavos, setSaldoInicialCentavos] = useState(0);
  const [saldoInicialDisplay, setSaldoInicialDisplay] = useState('');
  const [cor, setCor] = useState('#6C63FF');
  const [principal, setPrincipal] = useState(false);

  const styles = getStyles(colors);

  function handleSaldoInicialChange(text: string) {
    const digits = text.replace(/\D/g, '');
    const centavos = parseInt(digits || '0', 10);
    setSaldoInicialCentavos(centavos);
    setSaldoInicialDisplay(centavosParaDisplay(centavos));
  }

  useFocusEffect(
    useCallback(() => {
      carregarContas();
    }, [])
  );

  async function carregarContas() {
    try {
      setCarregando(true);
      const response = await api.get('/contas');
      const resultado: Resultado<ContaResponse[]> = response.data;
      if (resultado.sucesso && resultado.dados) setContas(resultado.dados);
    } catch {
      console.log('Erro ao carregar contas.');
    } finally {
      setCarregando(false);
    }
  }

  function abrirCriar() {
    setContaEditando(null);
    setNome('');
    setBanco('');
    setTipoConta('CORRENTE');
    setSaldoInicialCentavos(0);
    setSaldoInicialDisplay('');
    setCor('#6C63FF');
    setPrincipal(false);
    setModalVisivel(true);
  }

  function abrirEditar(conta: ContaResponse) {
    setContaEditando(conta);
    setNome(conta.nome);
    setBanco(conta.banco || '');
    setTipoConta(conta.tipoConta);
    setSaldoInicialCentavos(0);
    setSaldoInicialDisplay('');
    setCor(conta.cor);
    setPrincipal(conta.principal);
    setModalVisivel(true);
  }

  function fecharModal() {
    setModalVisivel(false);
    setContaEditando(null);
  }

  function opcoesConta(conta: ContaResponse) {
    Alert.alert(conta.nome, 'O que deseja fazer?', [
      { text: 'Editar', onPress: () => abrirEditar(conta) },
      {
        text: 'Excluir',
        style: 'destructive',
        onPress: () => excluirConta(conta),
      },
      { text: 'Cancelar', style: 'cancel' },
    ]);
  }

  async function salvarConta() {
    if (!nome.trim()) {
      Alert.alert('Atenção', 'Informe o nome da conta.');
      return;
    }

    setSalvando(true);
    try {
      if (contaEditando) {
        const response = await api.put(`/contas/${contaEditando.id}`, {
          nome: nome.trim(),
          banco: banco.trim() || null,
          cor,
          principal,
        });
        const resultado: Resultado<ContaResponse> = response.data;
        if (resultado.sucesso) {
          Alert.alert('Sucesso', 'Conta atualizada!');
          fecharModal();
          carregarContas();
        } else {
          Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao atualizar conta.');
        }
      } else {
        const response = await api.post('/contas', {
          nome: nome.trim(),
          banco: banco.trim() || null,
          tipoConta,
          saldoInicial: saldoInicialCentavos / 100,
          cor,
          icone: 'wallet',
          principal,
        });
        const resultado: Resultado<ContaResponse> = response.data;
        if (resultado.sucesso) {
          Alert.alert('Sucesso', resultado.mensagem || 'Conta criada!');
          fecharModal();
          carregarContas();
        } else {
          Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao criar conta.');
        }
      }
    } catch (error: any) {
      Alert.alert('Erro', error.response?.data?.erros?.[0] || 'Erro de conexão.');
    } finally {
      setSalvando(false);
    }
  }

  async function excluirConta(conta: ContaResponse) {
    if (conta.temCartaoVinculado) {
      Alert.alert(
        'Não é possível excluir',
        'Esta conta possui cartão de crédito vinculado.\nExclua o cartão primeiro.'
      );
      return;
    }

    Alert.alert(
      'Excluir conta',
      `Deseja excluir "${conta.nome}"?\n\nO histórico de transações será preservado.`,
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Excluir',
          style: 'destructive',
          onPress: async () => {
            try {
              const response = await api.delete(`/contas/${conta.id}`);
              const resultado: Resultado<boolean> = response.data;
              Alert.alert('', resultado.mensagem || 'Conta excluída.');
              carregarContas();
            } catch (error: any) {
              Alert.alert('Erro', error.response?.data?.erros?.[0] || 'Erro ao excluir.');
            }
          },
        },
      ]
    );
  }

  function saldoTotal(): number {
    return contas.reduce((acc, c) => acc + c.saldoAtual, 0);
  }

  function renderConta({ item }: { item: ContaResponse }) {
    return (
      <TouchableOpacity
        style={styles.contaCard}
        onPress={() => opcoesConta(item)}
        activeOpacity={0.7}
      >
        <View style={styles.contaEsquerda}>
          <BancoIcone banco={item.banco} nome={item.nome} corConta={item.cor} size={44} />
          <View style={styles.contaInfo}>
            <View style={styles.contaNomeLinha}>
              <Text style={styles.contaNome}>{item.nome}</Text>
              {item.principal && (
                <View style={styles.badgePrincipal}>
                  <Text style={styles.badgePrincipalTexto}>Principal</Text>
                </View>
              )}
            </View>
            {item.banco && <Text style={styles.contaBanco}>{item.banco}</Text>}
            <Text style={styles.contaTipo}>{item.tipoConta}</Text>
          </View>
        </View>
        <View style={styles.contaDireita}>
          <Text style={[styles.contaSaldo, { color: item.saldoAtual >= 0 ? colors.success : colors.danger }]}>
            {formatarMoeda(item.saldoAtual)}
          </Text>
          <Ionicons name="chevron-forward" size={16} color={colors.textMuted} style={{ marginTop: 2 }} />
        </View>
      </TouchableOpacity>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View>
          <Text style={styles.headerTitulo}>Minhas Contas</Text>
          <Text style={styles.headerSubtitulo}>
            Saldo total: {formatarMoeda(saldoTotal())}
          </Text>
        </View>
        <View style={styles.headerBotoes}>
          {contas.length >= 2 && (
            <TouchableOpacity
              style={styles.botaoTransferir}
              onPress={() => navigation.navigate('Transferencia')}
            >
              <Ionicons name="swap-horizontal" size={18} color={colors.primary} />
            </TouchableOpacity>
          )}
          <TouchableOpacity style={styles.botaoAdicionar} onPress={abrirCriar}>
            <Ionicons name="add" size={24} color={colors.textWhite} />
          </TouchableOpacity>
        </View>
      </View>

      <FlatList
        data={contas}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderConta}
        refreshControl={
          <RefreshControl refreshing={carregando} onRefresh={carregarContas} colors={[colors.primary]} />
        }
        contentContainerStyle={styles.lista}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.vazio}>
              <Ionicons name="wallet-outline" size={64} color={colors.textMuted} />
              <Text style={styles.vazioTexto}>Nenhuma conta cadastrada</Text>
              <Text style={styles.vazioSub}>Toque no + para adicionar sua primeira conta</Text>
            </View>
          ) : null
        }
      />

      <Modal visible={modalVisivel} animationType="slide" transparent>
        <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'} style={{ flex: 1 }}>
          <View style={styles.modalOverlay}>
            <View style={styles.modalContainer}>
              <ScrollView showsVerticalScrollIndicator={false} keyboardShouldPersistTaps="handled">
                <View style={styles.modalHeader}>
                  <Text style={styles.modalTitulo}>
                    {contaEditando ? 'Editar Conta' : 'Nova Conta'}
                  </Text>
                  <TouchableOpacity onPress={fecharModal}>
                    <Ionicons name="close" size={24} color={colors.textSecondary} />
                  </TouchableOpacity>
                </View>

                <Text style={styles.label}>Nome da conta *</Text>
                <TextInput
                  style={styles.input}
                  placeholder="Ex: Nubank, Itaú, Carteira"
                  placeholderTextColor={colors.textMuted}
                  value={nome}
                  onChangeText={setNome}
                  autoFocus={!contaEditando}
                />

                <Text style={styles.label}>Banco (opcional)</Text>
                <TextInput
                  style={styles.input}
                  placeholder="Ex: Nubank, Bradesco, Itaú..."
                  placeholderTextColor={colors.textMuted}
                  value={banco}
                  onChangeText={setBanco}
                />

                {!contaEditando && (
                  <>
                    <Text style={styles.label}>Tipo de conta</Text>
                    <View style={styles.tipoContainer}>
                      {TIPOS_CONTA.map((t) => (
                        <TouchableOpacity
                          key={t.valor}
                          style={[styles.tipoOpcao, tipoConta === t.valor && styles.tipoOpcaoAtivo]}
                          onPress={() => setTipoConta(t.valor)}
                        >
                          <Text style={[styles.tipoTexto, tipoConta === t.valor && styles.tipoTextoAtivo]}>
                            {t.label}
                          </Text>
                        </TouchableOpacity>
                      ))}
                    </View>

                    <Text style={styles.label}>Saldo inicial</Text>
                    <TextInput
                      style={styles.input}
                      placeholder="0,00"
                      placeholderTextColor={colors.textMuted}
                      keyboardType="number-pad"
                      value={saldoInicialDisplay}
                      onChangeText={handleSaldoInicialChange}
                    />
                  </>
                )}

                <Text style={styles.label}>Cor</Text>
                <View style={styles.coresContainer}>
                  {CORES_CONTA.map((c) => (
                    <TouchableOpacity
                      key={c}
                      style={[styles.corOpcao, { backgroundColor: c }, cor === c && styles.corOpcaoAtivo]}
                      onPress={() => setCor(c)}
                    >
                      {cor === c && <Ionicons name="checkmark" size={16} color="#FFF" />}
                    </TouchableOpacity>
                  ))}
                </View>

                <TouchableOpacity
                  style={styles.checkboxContainer}
                  onPress={() => setPrincipal(!principal)}
                >
                  <Ionicons
                    name={principal ? 'checkbox' : 'square-outline'}
                    size={24}
                    color={principal ? colors.primary : colors.textMuted}
                  />
                  <Text style={styles.checkboxTexto}>Definir como conta principal</Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={[styles.botaoSalvar, salvando && styles.botaoDisabled]}
                  onPress={salvarConta}
                  disabled={salvando}
                >
                  <Text style={styles.botaoSalvarTexto}>
                    {salvando
                      ? contaEditando ? 'Salvando...' : 'Criando...'
                      : contaEditando ? 'Salvar Alterações' : 'Criar Conta'}
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
  headerTitulo: { fontSize: FontSize.xxl, fontWeight: '700', color: colors.textPrimary },
  headerSubtitulo: { fontSize: FontSize.md, color: colors.textSecondary, marginTop: 2 },
  headerBotoes: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  botaoTransferir: {
    width: 40, height: 40, borderRadius: 20,
    borderWidth: 1.5, borderColor: colors.primary,
    justifyContent: 'center', alignItems: 'center',
    backgroundColor: colors.surface,
  },
  botaoAdicionar: {
    backgroundColor: colors.primary,
    width: 44, height: 44, borderRadius: 22,
    justifyContent: 'center', alignItems: 'center',
  },
  lista: { padding: Spacing.md, paddingBottom: 100 },
  contaCard: {
    backgroundColor: colors.surface,
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
  contaEsquerda: { flexDirection: 'row', alignItems: 'center', flex: 1 },
  contaInfo: { flex: 1, marginLeft: Spacing.md },
  contaNomeLinha: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  contaNome: { fontSize: FontSize.lg, fontWeight: '600', color: colors.textPrimary },
  badgePrincipal: {
    backgroundColor: colors.primaryLight,
    paddingHorizontal: 8, paddingVertical: 2, borderRadius: 4,
  },
  badgePrincipalTexto: { fontSize: FontSize.xs, color: colors.primary, fontWeight: '600' },
  contaBanco: { fontSize: FontSize.sm, color: colors.textSecondary, marginTop: 2 },
  contaTipo: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 1 },
  contaDireita: { alignItems: 'flex-end' },
  contaSaldo: { fontSize: FontSize.lg, fontWeight: '700' },
  vazio: { alignItems: 'center', marginTop: 100 },
  vazioTexto: { fontSize: FontSize.lg, color: colors.textSecondary, marginTop: Spacing.md, fontWeight: '600' },
  vazioSub: { fontSize: FontSize.md, color: colors.textMuted, marginTop: Spacing.xs },
  modalOverlay: {
    flex: 1, backgroundColor: colors.overlay, justifyContent: 'flex-end',
  },
  modalContainer: {
    backgroundColor: colors.surface,
    borderTopLeftRadius: BorderRadius.xl,
    borderTopRightRadius: BorderRadius.xl,
    padding: Spacing.lg,
    maxHeight: '92%',
  },
  modalHeader: {
    flexDirection: 'row', justifyContent: 'space-between',
    alignItems: 'center', marginBottom: Spacing.lg,
  },
  modalTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  label: {
    fontSize: FontSize.sm, fontWeight: '600', color: colors.textSecondary,
    marginBottom: Spacing.xs, marginTop: Spacing.md,
    textTransform: 'uppercase', letterSpacing: 0.5,
  },
  input: {
    backgroundColor: colors.surfaceVariant,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    fontSize: FontSize.lg,
    color: colors.textPrimary,
    borderWidth: 1,
    borderColor: colors.border,
  },
  tipoContainer: { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  tipoOpcao: {
    paddingHorizontal: 16, paddingVertical: 10,
    borderRadius: BorderRadius.sm, borderWidth: 1.5,
    borderColor: colors.border, backgroundColor: colors.surfaceVariant,
  },
  tipoOpcaoAtivo: { borderColor: colors.primary, backgroundColor: colors.primaryLight },
  tipoTexto: { fontSize: FontSize.md, color: colors.textSecondary, fontWeight: '500' },
  tipoTextoAtivo: { color: colors.primary, fontWeight: '600' },
  coresContainer: { flexDirection: 'row', gap: 12, flexWrap: 'wrap' },
  corOpcao: {
    width: 36, height: 36, borderRadius: 18,
    justifyContent: 'center', alignItems: 'center',
  },
  corOpcaoAtivo: {
    borderWidth: 3, borderColor: colors.textWhite,
    shadowColor: '#000', shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3, shadowRadius: 4, elevation: 4,
  },
  checkboxContainer: { flexDirection: 'row', alignItems: 'center', gap: 10, marginTop: Spacing.lg },
  checkboxTexto: { fontSize: FontSize.md, color: colors.textPrimary },
  botaoSalvar: {
    backgroundColor: colors.primary, borderRadius: BorderRadius.sm,
    padding: Spacing.md + 2, alignItems: 'center',
    marginTop: Spacing.lg, marginBottom: Spacing.md,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoSalvarTexto: { color: colors.textWhite, fontSize: FontSize.lg, fontWeight: '700' },
});
