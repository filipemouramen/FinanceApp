import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import api from '../../api/client';
import { ContaResponse, Resultado } from '../../types';
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

export default function CriarCartaoScreen() {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const [contas, setContas] = useState<ContaResponse[]>([]);
  const [nome, setNome] = useState('');
  const [bandeira, setBandeira] = useState('');
  const [limiteTotalCentavos, setLimiteTotalCentavos] = useState(0);
  const [limiteTotalDisplay, setLimiteTotalDisplay] = useState('');
  const [diaFechamento, setDiaFechamento] = useState('');
  const [diaVencimento, setDiaVencimento] = useState('');
  const [contaId, setContaId] = useState<number | null>(null);
  const [cor, setCor] = useState('#FF4757');
  const [carregando, setCarregando] = useState(false);
  const styles = getStyles(colors);

  function handleLimiteTotalChange(text: string) {
    const digits = text.replace(/\D/g, '');
    const centavos = parseInt(digits || '0', 10);
    setLimiteTotalCentavos(centavos);
    setLimiteTotalDisplay(centavosParaDisplay(centavos));
  }

  useFocusEffect(
    useCallback(() => {
      api.get('/contas').then((r) => {
        if (r.data.sucesso) setContas(r.data.dados);
      }).catch(() => {});
    }, [])
  );

  async function handleSalvar() {
    if (!nome.trim()) { Alert.alert('Atenção', 'Informe o nome do cartão.'); return; }
    if (limiteTotalCentavos <= 0) { Alert.alert('Atenção', 'Informe um limite válido.'); return; }
    const fechamento = parseInt(diaFechamento);
    if (!fechamento || fechamento < 1 || fechamento > 30) { Alert.alert('Atenção', 'Dia de fechamento deve ser entre 1 e 30.'); return; }
    const vencimento = parseInt(diaVencimento);
    if (!vencimento || vencimento < 1 || vencimento > 30) { Alert.alert('Atenção', 'Dia de vencimento deve ser entre 1 e 30.'); return; }

    setCarregando(true);
    try {
      const res = await api.post('/cartoes', {
        nome: nome.trim(),
        bandeira: bandeira || null,
        limiteTotal: limiteTotalCentavos / 100,
        diaFechamento: fechamento,
        diaVencimento: vencimento,
        contaId: contaId || null,
        cor,
      });
      const resultado: Resultado<any> = res.data;
      if (resultado.sucesso) {
        Alert.alert('Sucesso', 'Cartão criado com sucesso!', [
          { text: 'OK', onPress: () => navigation.goBack() },
        ]);
      } else {
        Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao criar cartão.');
      }
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível criar o cartão.');
    } finally {
      setCarregando(false);
    }
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={24} color={colors.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitulo}>Novo Cartão</Text>
        <View style={{ width: 24 }} />
      </View>

      <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
        {/* Preview do cartão */}
        <View style={[styles.preview, { backgroundColor: cor }]}>
          <Text style={styles.previewNome}>{nome || 'Nome do Cartão'}</Text>
          <Text style={styles.previewBandeira}>{bandeira || 'Bandeira'}</Text>
          <Text style={styles.previewLimite}>
            Limite: {formatarMoeda(limiteTotalCentavos / 100)}
          </Text>
        </View>

        {/* Cor */}
        <View style={styles.corContainer}>
          {CORES_CARTAO.map((c) => (
            <TouchableOpacity
              key={c}
              style={[styles.corChip, { backgroundColor: c }, cor === c && styles.corChipSelecionado]}
              onPress={() => setCor(c)}
            />
          ))}
        </View>

        <View style={styles.form}>
          <Text style={styles.label}>Nome do cartão</Text>
          <TextInput style={styles.input} placeholder="Ex: Nubank Roxinho" placeholderTextColor={colors.textMuted}
            value={nome} onChangeText={setNome} />

          <Text style={styles.label}>Bandeira</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chips}>
            {BANDEIRAS.map((b) => (
              <TouchableOpacity key={b} style={[styles.chip, bandeira === b && styles.chipSelecionado]}
                onPress={() => setBandeira(b === bandeira ? '' : b)}>
                <Text style={[styles.chipTexto, bandeira === b && styles.chipTextoSelecionado]}>{b}</Text>
              </TouchableOpacity>
            ))}
          </ScrollView>

          <Text style={styles.label}>Limite total (R$)</Text>
          <TextInput style={styles.input} placeholder="0,00" placeholderTextColor={colors.textMuted}
            keyboardType="number-pad" value={limiteTotalDisplay} onChangeText={handleLimiteTotalChange} />

          <View style={styles.duploCampo}>
            <View style={styles.campoMeio}>
              <Text style={styles.label}>Dia de fechamento</Text>
              <TextInput style={styles.input} placeholder="1-30" placeholderTextColor={colors.textMuted}
                keyboardType="number-pad" maxLength={2} value={diaFechamento} onChangeText={setDiaFechamento} />
            </View>
            <View style={styles.campoMeio}>
              <Text style={styles.label}>Dia de vencimento</Text>
              <TextInput style={styles.input} placeholder="1-30" placeholderTextColor={colors.textMuted}
                keyboardType="number-pad" maxLength={2} value={diaVencimento} onChangeText={setDiaVencimento} />
            </View>
          </View>

          <Text style={styles.label}>Conta vinculada (opcional)</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chips}>
            {[{ id: null as number | null, nome: 'Nenhuma', cor: colors.textMuted }, ...contas.map((c) => ({ id: c.id as number | null, nome: c.nome, cor: c.cor }))].map((c) => (
              <TouchableOpacity key={c.id ?? -1}
                style={[styles.chip, contaId === c.id && styles.chipSelecionado]}
                onPress={() => setContaId(c.id)}>
                <Text style={[styles.chipTexto, contaId === c.id && styles.chipTextoSelecionado]}>{c.nome}</Text>
              </TouchableOpacity>
            ))}
          </ScrollView>

          <TouchableOpacity
            style={[styles.botao, carregando && styles.botaoDisabled]}
            onPress={handleSalvar}
            disabled={carregando}
          >
            <Text style={styles.botaoTexto}>{carregando ? 'Salvando...' : 'Salvar Cartão'}</Text>
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
  headerTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  scroll: { padding: Spacing.lg, paddingBottom: 80 },
  preview: {
    borderRadius: BorderRadius.xl,
    padding: Spacing.xl,
    marginBottom: Spacing.md,
    minHeight: 120,
    justifyContent: 'space-between',
  },
  previewNome: { fontSize: FontSize.xl, fontWeight: '700', color: '#fff' },
  previewBandeira: { fontSize: FontSize.sm, color: 'rgba(255,255,255,0.75)', marginTop: 4 },
  previewLimite: { fontSize: FontSize.md, color: 'rgba(255,255,255,0.9)', fontWeight: '600' },
  corContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 10,
    marginBottom: Spacing.md,
    justifyContent: 'center',
  },
  corChip: { width: 32, height: 32, borderRadius: 16 },
  corChipSelecionado: { borderWidth: 3, borderColor: colors.textPrimary },
  form: {
    backgroundColor: colors.surface,
    borderRadius: BorderRadius.lg,
    padding: Spacing.lg,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.06,
    shadowRadius: 8,
    elevation: 3,
  },
  label: {
    fontSize: FontSize.sm,
    fontWeight: '600',
    color: colors.textSecondary,
    marginBottom: Spacing.xs,
    marginTop: Spacing.md,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  input: {
    backgroundColor: colors.surfaceVariant,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: colors.border,
    padding: Spacing.md,
    fontSize: FontSize.md,
    color: colors.textPrimary,
  },
  duploCampo: { flexDirection: 'row', gap: Spacing.md },
  campoMeio: { flex: 1 },
  chips: { marginBottom: 4 },
  chip: {
    paddingHorizontal: Spacing.md,
    paddingVertical: 6,
    borderRadius: BorderRadius.full,
    borderWidth: 1.5,
    borderColor: colors.border,
    marginRight: Spacing.xs,
    backgroundColor: colors.surface,
  },
  chipSelecionado: { borderColor: colors.primary, backgroundColor: colors.primary + '15' },
  chipTexto: { fontSize: FontSize.sm, color: colors.textSecondary },
  chipTextoSelecionado: { color: colors.primary, fontWeight: '600' },
  botao: {
    backgroundColor: colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    alignItems: 'center',
    marginTop: Spacing.xl,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoTexto: { color: colors.textWhite, fontSize: FontSize.lg, fontWeight: '600' },
});
