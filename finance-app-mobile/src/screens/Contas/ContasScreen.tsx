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
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect } from '@react-navigation/native';
import api from '../../api/client';
import { ContaResponse, Resultado } from '../../types';
import { Colors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { formatarMoeda } from '../../utils/formatters';

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

export default function ContasScreen() {
  const [contas, setContas] = useState<ContaResponse[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [modalVisivel, setModalVisivel] = useState(false);
  const [salvando, setSalvando] = useState(false);

  const [nome, setNome] = useState('');
  const [banco, setBanco] = useState('');
  const [tipoConta, setTipoConta] = useState('CORRENTE');
  const [saldoInicial, setSaldoInicial] = useState('');
  const [cor, setCor] = useState('#6C63FF');
  const [principal, setPrincipal] = useState(false);

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
      if (resultado.sucesso && resultado.dados) {
        setContas(resultado.dados);
      }
    } catch (error) {
      console.log('Erro ao carregar contas.', error);
    } finally {
      setCarregando(false);
    }
  }

  async function criarConta() {
    if (!nome.trim()) {
      Alert.alert('Atenção', 'Informe o nome da conta.');
      return;
    }

    setSalvando(true);
    try {
      const response = await api.post('/contas', {
        nome: nome.trim(),
        banco: banco.trim() || null,
        tipoConta,
        saldoInicial: parseFloat(saldoInicial) || 0,
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
    } catch (error: any) {
      Alert.alert('Erro', error.response?.data?.erros?.[0] || 'Erro de conexão.');
    } finally {
      setSalvando(false);
    }
  }

  async function excluirConta(conta: ContaResponse) {
    Alert.alert(
      'Excluir conta',
      `Deseja excluir a conta "${conta.nome}"?`,
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

  function fecharModal() {
    setModalVisivel(false);
    setNome('');
    setBanco('');
    setTipoConta('CORRENTE');
    setSaldoInicial('');
    setCor('#6C63FF');
    setPrincipal(false);
  }

  function saldoTotal(): number {
    return contas.reduce((acc, c) => acc + c.saldoAtual, 0);
  }

  function renderConta({ item }: { item: ContaResponse }) {
    return (
      <TouchableOpacity
        style={styles.contaCard}
        onLongPress={() => excluirConta(item)}
        activeOpacity={0.7}
      >
        <View style={styles.contaEsquerda}>
          <View style={[styles.contaIcone, { backgroundColor: item.cor + '20' }]}>
            <Ionicons name="wallet" size={22} color={item.cor} />
          </View>
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
        <Text style={[styles.contaSaldo, { color: item.saldoAtual >= 0 ? Colors.success : Colors.danger }]}>
          {formatarMoeda(item.saldoAtual)}
        </Text>
      </TouchableOpacity>
    );
  }

  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <View>
          <Text style={styles.headerTitulo}>Minhas Contas</Text>
          <Text style={styles.headerSubtitulo}>
            Saldo total: {formatarMoeda(saldoTotal())}
          </Text>
        </View>
        <TouchableOpacity
          style={styles.botaoAdicionar}
          onPress={() => setModalVisivel(true)}
        >
          <Ionicons name="add" size={24} color={Colors.textWhite} />
        </TouchableOpacity>
      </View>

      {/* Lista */}
      <FlatList
        data={contas}
        keyExtractor={(item) => item.id}
        renderItem={renderConta}
        refreshControl={
          <RefreshControl refreshing={carregando} onRefresh={carregarContas} colors={[Colors.primary]} />
        }
        contentContainerStyle={styles.lista}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.vazio}>
              <Ionicons name="wallet-outline" size={64} color={Colors.textMuted} />
              <Text style={styles.vazioTexto}>Nenhuma conta cadastrada</Text>
              <Text style={styles.vazioSub}>Toque no + para adicionar sua primeira conta</Text>
            </View>
          ) : null
        }
      />

      <Modal visible={modalVisivel} animationType="slide" transparent>
        <View style={styles.modalOverlay}>
          <View style={styles.modalContainer}>
            <ScrollView showsVerticalScrollIndicator={false}>
              <View style={styles.modalHeader}>
                <Text style={styles.modalTitulo}>Nova Conta</Text>
                <TouchableOpacity onPress={fecharModal}>
                  <Ionicons name="close" size={24} color={Colors.textSecondary} />
                </TouchableOpacity>
              </View>

              {/* Nome */}
              <Text style={styles.label}>Nome da conta *</Text>
              <TextInput
                style={styles.input}
                placeholder="Ex: Nubank, Itaú, Carteira"
                placeholderTextColor={Colors.textMuted}
                value={nome}
                onChangeText={setNome}
              />

              {/* Banco */}
              <Text style={styles.label}>Banco (opcional)</Text>
              <TextInput
                style={styles.input}
                placeholder="Ex: Nu Pagamentos, Banco Itaú"
                placeholderTextColor={Colors.textMuted}
                value={banco}
                onChangeText={setBanco}
              />

              {/* Tipo de Conta */}
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

              {/* Saldo Inicial */}
              <Text style={styles.label}>Saldo inicial</Text>
              <TextInput
                style={styles.input}
                placeholder="0,00"
                placeholderTextColor={Colors.textMuted}
                keyboardType="decimal-pad"
                value={saldoInicial}
                onChangeText={setSaldoInicial}
              />

              {/* Cor */}
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

              {/* Principal */}
              <TouchableOpacity
                style={styles.checkboxContainer}
                onPress={() => setPrincipal(!principal)}
              >
                <Ionicons
                  name={principal ? 'checkbox' : 'square-outline'}
                  size={24}
                  color={principal ? Colors.primary : Colors.textMuted}
                />
                <Text style={styles.checkboxTexto}>Definir como conta principal</Text>
              </TouchableOpacity>

              {/* Botão Salvar */}
              <TouchableOpacity
                style={[styles.botaoSalvar, salvando && styles.botaoDisabled]}
                onPress={criarConta}
                disabled={salvando}
              >
                <Text style={styles.botaoSalvarTexto}>
                  {salvando ? 'Salvando...' : 'Criar Conta'}
                </Text>
              </TouchableOpacity>
            </ScrollView>
          </View>
        </View>
      </Modal>
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
  headerSubtitulo: {
    fontSize: FontSize.md,
    color: Colors.textSecondary,
    marginTop: 2,
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
  contaCard: {
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
  contaEsquerda: {
    flexDirection: 'row',
    alignItems: 'center',
    flex: 1,
  },
  contaIcone: {
    width: 44,
    height: 44,
    borderRadius: 12,
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: Spacing.md,
  },
  contaInfo: {
    flex: 1,
  },
  contaNomeLinha: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  contaNome: {
    fontSize: FontSize.lg,
    fontWeight: '600',
    color: Colors.textPrimary,
  },
  badgePrincipal: {
    backgroundColor: Colors.primaryLight,
    paddingHorizontal: 8,
    paddingVertical: 2,
    borderRadius: 4,
  },
  badgePrincipalTexto: {
    fontSize: FontSize.xs,
    color: Colors.primary,
    fontWeight: '600',
  },
  contaBanco: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    marginTop: 2,
  },
  contaTipo: {
    fontSize: FontSize.xs,
    color: Colors.textMuted,
    marginTop: 1,
  },
  contaSaldo: {
    fontSize: FontSize.lg,
    fontWeight: '700',
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

  modalOverlay: {
    flex: 1,
    backgroundColor: Colors.overlay,
    justifyContent: 'flex-end',
  },
  modalContainer: {
    backgroundColor: Colors.surface,
    borderTopLeftRadius: BorderRadius.xl,
    borderTopRightRadius: BorderRadius.xl,
    padding: Spacing.lg,
    maxHeight: '90%',
  },
  modalHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: Spacing.lg,
  },
  modalTitulo: {
    fontSize: FontSize.xl,
    fontWeight: '700',
    color: Colors.textPrimary,
  },
  label: {
    fontSize: FontSize.sm,
    fontWeight: '600',
    color: Colors.textSecondary,
    marginBottom: Spacing.xs,
    marginTop: Spacing.md,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  input: {
    backgroundColor: Colors.surfaceVariant,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    fontSize: FontSize.lg,
    color: Colors.textPrimary,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  tipoContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  tipoOpcao: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: Colors.border,
    backgroundColor: Colors.surfaceVariant,
  },
  tipoOpcaoAtivo: {
    borderColor: Colors.primary,
    backgroundColor: Colors.primaryLight,
  },
  tipoTexto: {
    fontSize: FontSize.md,
    color: Colors.textSecondary,
    fontWeight: '500',
  },
  tipoTextoAtivo: {
    color: Colors.primary,
    fontWeight: '600',
  },
  coresContainer: {
    flexDirection: 'row',
    gap: 12,
    flexWrap: 'wrap',
  },
  corOpcao: {
    width: 36,
    height: 36,
    borderRadius: 18,
    justifyContent: 'center',
    alignItems: 'center',
  },
  corOpcaoAtivo: {
    borderWidth: 3,
    borderColor: Colors.textWhite,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 4,
    elevation: 4,
  },
  checkboxContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    marginTop: Spacing.lg,
  },
  checkboxTexto: {
    fontSize: FontSize.md,
    color: Colors.textPrimary,
  },
  botaoSalvar: {
    backgroundColor: Colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md + 2,
    alignItems: 'center',
    marginTop: Spacing.lg,
    marginBottom: Spacing.md,
  },
  botaoDisabled: {
    opacity: 0.6,
  },
  botaoSalvarTexto: {
    color: Colors.textWhite,
    fontSize: FontSize.lg,
    fontWeight: '700',
  },
});