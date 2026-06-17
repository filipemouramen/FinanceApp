import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TextInput,
  TouchableOpacity,
  ScrollView,
  Alert,
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

function centavosParaDisplay(centavos: number): string {
  if (centavos === 0) return '';
  const reais = Math.floor(centavos / 100);
  const cents = centavos % 100;
  const reaisStr = reais.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  return `${reaisStr},${cents.toString().padStart(2, '0')}`;
}

export default function TransferenciaScreen() {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const [contas, setContas] = useState<ContaResponse[]>([]);
  const [contaOrigemId, setContaOrigemId] = useState<number | null>(null);
  const [contaDestinoId, setContaDestinoId] = useState<number | null>(null);
  const [valorCentavos, setValorCentavos] = useState(0);
  const [valorDisplay, setValorDisplay] = useState('');
  const [descricao, setDescricao] = useState('');
  const [carregando, setCarregando] = useState(false);
  const styles = getStyles(colors);

  function handleValorChange(text: string) {
    const digits = text.replace(/\D/g, '');
    const centavos = parseInt(digits || '0', 10);
    setValorCentavos(centavos);
    setValorDisplay(centavosParaDisplay(centavos));
  }

  useFocusEffect(
    useCallback(() => {
      api.get('/contas').then((r) => {
        if (r.data.sucesso) setContas(r.data.dados);
      }).catch(() => {});
    }, [])
  );

  const contaOrigem = contas.find((c) => c.id === contaOrigemId);
  const contaDestino = contas.find((c) => c.id === contaDestinoId);

  async function handleTransferir() {
    if (!contaOrigemId || !contaDestinoId) {
      Alert.alert('Atenção', 'Selecione a conta de origem e destino.');
      return;
    }
    if (contaOrigemId === contaDestinoId) {
      Alert.alert('Atenção', 'Origem e destino não podem ser a mesma conta.');
      return;
    }
    if (valorCentavos <= 0) {
      Alert.alert('Atenção', 'Informe um valor válido.');
      return;
    }

    setCarregando(true);
    try {
      const response = await api.post('/transferencias', {
        contaOrigemId,
        contaDestinoId,
        valor: valorCentavos / 100,
        descricao: descricao.trim() || null,
        dataTransferencia: new Date().toISOString().split('T')[0],
      });

      const resultado: Resultado<any> = response.data;
      if (resultado.sucesso && resultado.dados) {
        const { contaOrigem: co, contaDestino: cd } = resultado.dados;
        Alert.alert(
          'Transferência realizada!',
          `${co.nome}: ${formatarMoeda(co.novoSaldo)}\n${cd.nome}: ${formatarMoeda(cd.novoSaldo)}`,
          [{ text: 'OK', onPress: () => navigation.goBack() }]
        );
      } else {
        Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao realizar a transferência.');
      }
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível realizar a transferência.');
    } finally {
      setCarregando(false);
    }
  }

  function renderSeletorConta(
    label: string,
    selecionadoId: number | null,
    excluirId: number | null,
    onSelect: (id: number) => void
  ) {
    return (
      <View style={styles.seletorContainer}>
        <Text style={styles.label}>{label}</Text>
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.contasScroll}>
          {contas
            .filter((c) => c.id !== excluirId)
            .map((conta) => (
              <TouchableOpacity
                key={conta.id}
                style={[
                  styles.contaChip,
                  selecionadoId === conta.id && styles.contaChipSelecionado,
                  { borderColor: conta.cor },
                ]}
                onPress={() => onSelect(conta.id)}
              >
                <View style={[styles.contaDot, { backgroundColor: conta.cor }]} />
                <View>
                  <Text style={[
                    styles.contaChipNome,
                    selecionadoId === conta.id && styles.contaChipNomeSelecionado,
                  ]}>
                    {conta.nome}
                  </Text>
                  <Text style={styles.contaChipSaldo}>{formatarMoeda(conta.saldoAtual)}</Text>
                </View>
              </TouchableOpacity>
            ))}
        </ScrollView>
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <View style={styles.header}>
        <TouchableOpacity style={styles.voltarBtn} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={24} color={colors.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitulo}>Transferência</Text>
        <View style={{ width: 40 }} />
      </View>

      <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
        {renderSeletorConta('Conta de Origem', contaOrigemId, contaDestinoId, setContaOrigemId)}

        {contaOrigemId && contaDestinoId && (
          <View style={styles.setaContainer}>
            <Ionicons name="arrow-down" size={28} color={colors.primary} />
          </View>
        )}

        {renderSeletorConta('Conta de Destino', contaDestinoId, contaOrigemId, setContaDestinoId)}

        <View style={styles.form}>
          <Text style={styles.label}>Valor (R$)</Text>
          <View style={styles.inputContainer}>
            <Text style={styles.moedaPrefix}>R$</Text>
            <TextInput
              style={styles.inputValor}
              placeholder="0,00"
              placeholderTextColor={colors.textMuted}
              keyboardType="number-pad"
              value={valorDisplay}
              onChangeText={handleValorChange}
            />
          </View>

          <Text style={styles.label}>Descrição (opcional)</Text>
          <TextInput
            style={styles.inputDescricao}
            placeholder="Ex: Transferência para poupança"
            placeholderTextColor={colors.textMuted}
            value={descricao}
            onChangeText={setDescricao}
          />

          {contaOrigem && contaDestino && valorCentavos > 0 ? (
            <View style={styles.resumo}>
              <Text style={styles.resumoTitulo}>Resumo</Text>
              <Text style={styles.resumoLinha}>
                <Text style={styles.resumoLabel}>De: </Text>
                <Text>{contaOrigem.nome}</Text>
              </Text>
              <Text style={styles.resumoLinha}>
                <Text style={styles.resumoLabel}>Para: </Text>
                <Text>{contaDestino.nome}</Text>
              </Text>
              <Text style={styles.resumoLinha}>
                <Text style={styles.resumoLabel}>Valor: </Text>
                <Text style={styles.resumoValor}>
                  {formatarMoeda(valorCentavos / 100)}
                </Text>
              </Text>
            </View>
          ) : null}

          <TouchableOpacity
            style={[styles.botao, carregando && styles.botaoDisabled]}
            onPress={handleTransferir}
            disabled={carregando}
          >
            <Ionicons name="swap-horizontal" size={20} color={colors.textWhite} style={{ marginRight: 8 }} />
            <Text style={styles.botaoTexto}>
              {carregando ? 'Realizando...' : 'Realizar Transferência'}
            </Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
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
  voltarBtn: { width: 40, height: 40, justifyContent: 'center' },
  headerTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  scroll: { padding: Spacing.lg, paddingBottom: 80 },
  seletorContainer: { marginBottom: Spacing.md },
  label: {
    fontSize: FontSize.sm,
    fontWeight: '600',
    color: colors.textSecondary,
    marginBottom: Spacing.xs,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  contasScroll: { marginHorizontal: -Spacing.lg, paddingHorizontal: Spacing.lg },
  contaChip: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.surface,
    borderRadius: BorderRadius.lg,
    padding: Spacing.md,
    marginRight: Spacing.sm,
    borderWidth: 2,
    borderColor: colors.border,
    minWidth: 140,
    gap: 8,
  },
  contaChipSelecionado: { backgroundColor: colors.primary + '10' },
  contaDot: { width: 10, height: 10, borderRadius: 5 },
  contaChipNome: { fontSize: FontSize.md, fontWeight: '600', color: colors.textPrimary },
  contaChipNomeSelecionado: { color: colors.primary },
  contaChipSaldo: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 2 },
  setaContainer: { alignItems: 'center', marginVertical: Spacing.xs },
  form: {
    backgroundColor: colors.surface,
    borderRadius: BorderRadius.lg,
    padding: Spacing.lg,
    marginTop: Spacing.md,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.06,
    shadowRadius: 8,
    elevation: 3,
  },
  inputContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.surfaceVariant,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: colors.border,
    paddingHorizontal: Spacing.md,
    marginBottom: Spacing.md,
  },
  moedaPrefix: { fontSize: FontSize.lg, color: colors.textSecondary, marginRight: 4 },
  inputValor: { flex: 1, padding: Spacing.md, fontSize: FontSize.xxl, fontWeight: '700', color: colors.textPrimary },
  inputDescricao: {
    backgroundColor: colors.surfaceVariant,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: colors.border,
    padding: Spacing.md,
    fontSize: FontSize.md,
    color: colors.textPrimary,
    marginBottom: Spacing.md,
  },
  resumo: {
    backgroundColor: colors.primary + '08',
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    marginBottom: Spacing.md,
    borderLeftWidth: 3,
    borderLeftColor: colors.primary,
  },
  resumoTitulo: { fontSize: FontSize.sm, fontWeight: '700', color: colors.primary, marginBottom: 6 },
  resumoLinha: { fontSize: FontSize.md, color: colors.textSecondary, marginBottom: 2 },
  resumoLabel: { fontWeight: '600', color: colors.textPrimary },
  resumoValor: { fontWeight: '700', color: colors.primary, fontSize: FontSize.lg },
  botao: {
    backgroundColor: colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
  botaoDisabled: { opacity: 0.6 },
  botaoTexto: { color: colors.textWhite, fontSize: FontSize.lg, fontWeight: '600' },
});
