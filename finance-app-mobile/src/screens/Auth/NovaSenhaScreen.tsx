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

export default function NovaSenhaScreen({ navigation, route }: any) {
  const { colors } = useTheme();
  const { email, codigo } = route.params as { email: string; codigo: string };
  const [novaSenha, setNovaSenha] = useState('');
  const [confirmarSenha, setConfirmarSenha] = useState('');
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const styles = getStyles(colors);

  async function handleRedefinir() {
    if (!novaSenha || !confirmarSenha) {
      Alert.alert('Atenção', 'Preencha os dois campos de senha.');
      return;
    }
    if (novaSenha.length < 6) {
      Alert.alert('Atenção', 'A senha deve ter no mínimo 6 caracteres.');
      return;
    }
    if (novaSenha !== confirmarSenha) {
      Alert.alert('Atenção', 'As senhas não coincidem.');
      return;
    }

    setCarregando(true);
    try {
      const res = await api.post('/auth/resetar-senha', { email, codigo, novaSenha });
      if (res.data.sucesso) {
        Alert.alert('Sucesso', 'Senha redefinida! Faça login com a nova senha.', [
          { text: 'OK', onPress: () => navigation.navigate('Login') },
        ]);
      } else {
        Alert.alert('Erro', res.data.mensagem || 'Não foi possível redefinir a senha.');
      }
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.mensagem || 'Não foi possível redefinir a senha.');
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
            <Ionicons name="shield-checkmark-outline" size={40} color={colors.primary} />
          </View>
          <Text style={styles.titulo}>Nova senha</Text>
          <Text style={styles.subtitulo}>Crie uma senha forte com pelo menos 6 caracteres.</Text>
        </View>

        <View style={styles.form}>
          <Text style={styles.label}>Nova senha</Text>
          <View style={styles.inputContainer}>
            <Ionicons name="lock-closed-outline" size={20} color={colors.textMuted} style={styles.inputIcone} />
            <TextInput
              style={styles.inputComIcone}
              placeholder="Mínimo 6 caracteres"
              placeholderTextColor={colors.textMuted}
              secureTextEntry={!mostrarSenha}
              value={novaSenha}
              onChangeText={setNovaSenha}
            />
            <TouchableOpacity onPress={() => setMostrarSenha(!mostrarSenha)} style={styles.inputIconeDireita}>
              <Ionicons
                name={mostrarSenha ? 'eye-off-outline' : 'eye-outline'}
                size={20}
                color={colors.textMuted}
              />
            </TouchableOpacity>
          </View>

          <Text style={styles.label}>Confirmar senha</Text>
          <View style={[
            styles.inputContainer,
            confirmarSenha.length > 0 && novaSenha !== confirmarSenha && styles.inputErro,
          ]}>
            <Ionicons name="lock-closed-outline" size={20} color={colors.textMuted} style={styles.inputIcone} />
            <TextInput
              style={styles.inputComIcone}
              placeholder="Repita a senha"
              placeholderTextColor={colors.textMuted}
              secureTextEntry={!mostrarSenha}
              value={confirmarSenha}
              onChangeText={setConfirmarSenha}
            />
          </View>
          {confirmarSenha.length > 0 && novaSenha !== confirmarSenha && (
            <Text style={styles.textoErro}>As senhas não coincidem</Text>
          )}

          <TouchableOpacity
            style={[styles.botao, carregando && styles.botaoDisabled]}
            onPress={handleRedefinir}
            disabled={carregando}
          >
            <Text style={styles.botaoTexto}>
              {carregando ? 'Salvando...' : 'Redefinir senha'}
            </Text>
          </TouchableOpacity>
        </View>
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
  inputErro: { borderColor: colors.danger },
  inputIcone: { paddingLeft: Spacing.md },
  inputIconeDireita: { paddingRight: Spacing.md, paddingVertical: Spacing.md },
  inputComIcone: {
    flex: 1,
    padding: Spacing.md,
    fontSize: FontSize.lg,
    color: colors.textPrimary,
  },
  textoErro: { fontSize: FontSize.xs, color: colors.danger, marginTop: 4 },
  botao: {
    backgroundColor: colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    alignItems: 'center',
    marginTop: Spacing.lg,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoTexto: { color: colors.textWhite, fontSize: FontSize.lg, fontWeight: '600' },
});
