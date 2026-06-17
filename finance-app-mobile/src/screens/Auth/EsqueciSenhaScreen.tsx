import React, { useState } from 'react';
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

export default function EsqueciSenhaScreen({ navigation }: any) {
  const { colors } = useTheme();
  const [email, setEmail] = useState('');
  const [carregando, setCarregando] = useState(false);
  const styles = getStyles(colors);

  async function handleEnviar() {
    if (!email.trim()) {
      Alert.alert('Atenção', 'Informe seu e-mail.');
      return;
    }

    setCarregando(true);
    try {
      await api.post('/auth/solicitar-reset-senha', { email: email.trim() });
      navigation.navigate('VerificarCodigo', { email: email.trim() });
    } catch {
      Alert.alert('Erro', 'Não foi possível enviar o código. Tente novamente.');
    } finally {
      setCarregando(false);
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
            <Ionicons name="lock-open-outline" size={40} color={colors.primary} />
          </View>
          <Text style={styles.titulo}>Esqueceu a senha?</Text>
          <Text style={styles.subtitulo}>
            Informe seu e-mail e enviaremos um código de verificação.
          </Text>
        </View>

        <View style={styles.form}>
          <Text style={styles.label}>E-mail</Text>
          <View style={styles.inputContainer}>
            <Ionicons name="mail-outline" size={20} color={colors.textMuted} style={styles.inputIcone} />
            <TextInput
              style={styles.inputComIcone}
              placeholder="seu@email.com"
              placeholderTextColor={colors.textMuted}
              keyboardType="email-address"
              autoCapitalize="none"
              value={email}
              onChangeText={setEmail}
            />
          </View>

          <TouchableOpacity
            style={[styles.botao, carregando && styles.botaoDisabled]}
            onPress={handleEnviar}
            disabled={carregando}
          >
            <Text style={styles.botaoTexto}>
              {carregando ? 'Enviando...' : 'Enviar código'}
            </Text>
          </TouchableOpacity>
        </View>

        <TouchableOpacity style={styles.linkContainer} onPress={() => navigation.goBack()}>
          <Text style={styles.linkTexto}>
            Lembrou a senha? <Text style={styles.linkDestaque}>Voltar ao login</Text>
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
  label: {
    fontSize: FontSize.sm,
    fontWeight: '600',
    color: colors.textSecondary,
    marginBottom: Spacing.xs,
    marginTop: Spacing.md,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  inputContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.surfaceVariant,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: colors.border,
  },
  inputIcone: { paddingLeft: Spacing.md },
  inputComIcone: {
    flex: 1,
    padding: Spacing.md,
    fontSize: FontSize.lg,
    color: colors.textPrimary,
  },
  botao: {
    backgroundColor: colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    alignItems: 'center',
    marginTop: Spacing.lg,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoTexto: { color: colors.textWhite, fontSize: FontSize.lg, fontWeight: '600' },
  linkContainer: { alignItems: 'center', marginTop: Spacing.lg },
  linkTexto: { fontSize: FontSize.md, color: colors.textSecondary },
  linkDestaque: { color: colors.primary, fontWeight: '600' },
});
