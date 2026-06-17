import React, { useState, useRef } from 'react';
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
import api from '../../api/client';
import { useTheme } from '../../theme/useTheme';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';

export default function VerificarCodigoScreen({ navigation, route }: any) {
  const { colors } = useTheme();
  const { email } = route.params as { email: string };
  const [digitos, setDigitos] = useState(['', '', '', '', '', '']);
  const [carregando, setCarregando] = useState(false);
  const refs = useRef<(TextInput | null)[]>([]);
  const styles = getStyles(colors);

  function handleDigito(valor: string, index: number) {
    const novos = [...digitos];
    novos[index] = valor.replace(/\D/, '');
    setDigitos(novos);
    if (valor && index < 5) refs.current[index + 1]?.focus();
  }

  function handleKeyPress(key: string, index: number) {
    if (key === 'Backspace' && !digitos[index] && index > 0) {
      refs.current[index - 1]?.focus();
    }
  }

  const codigoCompleto = digitos.join('');

  async function handleVerificar() {
    if (codigoCompleto.length < 6) {
      Alert.alert('Atenção', 'Digite os 6 dígitos do código.');
      return;
    }
    setCarregando(true);
    try {
      const res = await api.post('/auth/verificar-codigo', { email, codigo: codigoCompleto });
      if (res.data.sucesso) {
        navigation.navigate('NovaSenha', { email, codigo: codigoCompleto });
      } else {
        Alert.alert('Código inválido', res.data.mensagem || 'Código incorreto ou expirado.');
      }
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.mensagem || 'Não foi possível verificar o código.');
    } finally {
      setCarregando(false);
    }
  }

  async function handleReenviar() {
    try {
      await api.post('/auth/solicitar-reset-senha', { email });
      Alert.alert('Código enviado', 'Um novo código foi enviado para seu e-mail.');
      setDigitos(['', '', '', '', '', '']);
      refs.current[0]?.focus();
    } catch {
      Alert.alert('Erro', 'Não foi possível reenviar o código.');
    }
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
        <TouchableOpacity style={styles.voltar} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={24} color={colors.textPrimary} />
        </TouchableOpacity>

        <View style={styles.header}>
          <View style={styles.iconeContainer}>
            <Ionicons name="keypad-outline" size={40} color={colors.primary} />
          </View>
          <Text style={styles.titulo}>Verificação</Text>
          <Text style={styles.subtitulo}>
            Digite o código de 6 dígitos enviado para{'\n'}
            <Text style={styles.emailDestaque}>{email}</Text>
          </Text>
        </View>

        <View style={styles.form}>
          <View style={styles.codigoContainer}>
            {digitos.map((d, i) => (
              <TextInput
                key={i}
                ref={(r) => { refs.current[i] = r; }}
                style={[styles.digitoInput, d ? styles.digitoPreenchido : null]}
                value={d}
                onChangeText={(v) => handleDigito(v, i)}
                onKeyPress={({ nativeEvent }) => handleKeyPress(nativeEvent.key, i)}
                keyboardType="number-pad"
                maxLength={1}
                textAlign="center"
              />
            ))}
          </View>

          <TouchableOpacity
            style={[styles.botao, (carregando || codigoCompleto.length < 6) && styles.botaoDisabled]}
            onPress={handleVerificar}
            disabled={carregando || codigoCompleto.length < 6}
          >
            <Text style={styles.botaoTexto}>
              {carregando ? 'Verificando...' : 'Verificar código'}
            </Text>
          </TouchableOpacity>
        </View>

        <TouchableOpacity style={styles.linkContainer} onPress={handleReenviar}>
          <Text style={styles.linkTexto}>
            Não recebeu? <Text style={styles.linkDestaque}>Reenviar código</Text>
          </Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  scroll: { flexGrow: 1, padding: Spacing.lg },
  voltar: { marginTop: 20, marginBottom: Spacing.sm },
  header: { alignItems: 'center', marginBottom: Spacing.xl, marginTop: Spacing.lg },
  iconeContainer: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: colors.primary + '15',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: Spacing.md,
  },
  titulo: { fontSize: FontSize.xxl, fontWeight: '700', color: colors.textPrimary },
  subtitulo: {
    fontSize: FontSize.md,
    color: colors.textSecondary,
    textAlign: 'center',
    marginTop: Spacing.sm,
    lineHeight: 22,
  },
  emailDestaque: { color: colors.primary, fontWeight: '600' },
  form: {
    backgroundColor: colors.surface,
    borderRadius: BorderRadius.lg,
    padding: Spacing.lg,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.08,
    shadowRadius: 12,
    elevation: 4,
  },
  codigoContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: Spacing.md,
  },
  digitoInput: {
    width: 48,
    height: 56,
    borderRadius: BorderRadius.sm,
    borderWidth: 2,
    borderColor: colors.border,
    backgroundColor: colors.surfaceVariant,
    fontSize: FontSize.xxl,
    fontWeight: '700',
    color: colors.textPrimary,
  },
  digitoPreenchido: { borderColor: colors.primary },
  botao: {
    backgroundColor: colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    alignItems: 'center',
    marginTop: Spacing.sm,
  },
  botaoDisabled: { opacity: 0.5 },
  botaoTexto: { color: colors.textWhite, fontSize: FontSize.lg, fontWeight: '600' },
  linkContainer: { alignItems: 'center', marginTop: Spacing.lg },
  linkTexto: { fontSize: FontSize.md, color: colors.textSecondary },
  linkDestaque: { color: colors.primary, fontWeight: '600' },
});
