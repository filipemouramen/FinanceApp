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
import { useAuth } from '../../contexts/AuthContext';
import { useTheme } from '../../theme/useTheme';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';

export default function LoginScreen({ navigation }: any) {
  const { login } = useAuth();
  const { colors } = useTheme();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [carregando, setCarregando] = useState(false);
  const [mostrarSenha, setMostrarSenha] = useState(false);

  const styles = getStyles(colors);

  async function handleLogin() {
    if (!email.trim() || !senha.trim()) {
      Alert.alert('Atenção', 'Preencha e-mail e a senha para continuar!');
      return;
    }

    setCarregando(true);
    const resultado = await login(email.trim(), senha);
    setCarregando(false);

    if (!resultado.sucesso) {
      Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao fazer login ❌');
    }
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
        <View style={styles.header}>
          <Text style={styles.titulo}>FinanceApp</Text>
          <Text style={styles.subtitulo}>Controle as suas finanças!</Text>
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

          <Text style={styles.label}>Senha</Text>
          <View style={styles.inputContainer}>
            <Ionicons name="lock-closed-outline" size={20} color={colors.textMuted} style={styles.inputIcone} />
            <TextInput
              style={styles.inputComIcone}
              placeholder="Sua senha"
              placeholderTextColor={colors.textMuted}
              secureTextEntry={!mostrarSenha}
              value={senha}
              onChangeText={setSenha}
            />
            <TouchableOpacity onPress={() => setMostrarSenha(!mostrarSenha)} style={styles.inputIconeDireita}>
              <Ionicons
                name={mostrarSenha ? 'eye-off-outline' : 'eye-outline'}
                size={20}
                color={colors.textMuted}
              />
            </TouchableOpacity>
          </View>

          <TouchableOpacity
            style={[styles.botao, carregando && styles.botaoDisabled]}
            onPress={handleLogin}
            disabled={carregando}
          >
            <Text style={styles.botaoTexto}>
              {carregando ? 'Entrando...' : 'Entrar'}
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={styles.esqueciContainer}
            onPress={() => navigation.navigate('EsqueciSenha')}
          >
            <Text style={styles.esqueciTexto}>Esqueci minha senha</Text>
          </TouchableOpacity>
        </View>

        <TouchableOpacity
          style={styles.linkContainer}
          onPress={() => navigation.navigate('Registro')}
        >
          <Text style={styles.linkTexto}>
            Não tem conta? <Text style={styles.linkDestaque}>Criar conta</Text>
          </Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scroll: {
    flexGrow: 1,
    justifyContent: 'center',
    padding: Spacing.lg,
  },
  header: {
    alignItems: 'center',
    marginBottom: Spacing.xl,
  },
  titulo: {
    fontSize: FontSize.title,
    fontWeight: '700',
    color: colors.primary,
  },
  subtitulo: {
    fontSize: FontSize.md,
    color: colors.textSecondary,
    marginTop: Spacing.xs,
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
  inputIcone: {
    paddingLeft: Spacing.md,
  },
  inputIconeDireita: {
    paddingRight: Spacing.md,
    paddingVertical: Spacing.md,
  },
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
  botaoDisabled: {
    opacity: 0.6,
  },
  botaoTexto: {
    color: colors.textWhite,
    fontSize: FontSize.lg,
    fontWeight: '600',
  },
  linkContainer: {
    alignItems: 'center',
    marginTop: Spacing.lg,
  },
  linkTexto: {
    fontSize: FontSize.md,
    color: colors.textSecondary,
  },
  linkDestaque: {
    color: colors.primary,
    fontWeight: '600',
  },
  esqueciContainer: {
    alignItems: 'center',
    marginTop: Spacing.md,
    paddingVertical: Spacing.sm,
  },
  esqueciTexto: {
    fontSize: FontSize.md,
    color: colors.primary,
    fontWeight: '500',
  },
});
