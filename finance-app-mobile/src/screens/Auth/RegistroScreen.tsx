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
import { Colors, Spacing, FontSize, BorderRadius } from '../../theme/colors';

function formatarTelefone(valor: string): string {
  const numeros = valor.replace(/\D/g, '');

  if (numeros.length <= 2) return `(${numeros}`;
  if (numeros.length <= 7) return `(${numeros.slice(0, 2)}) ${numeros.slice(2)}`;
  return `(${numeros.slice(0, 2)}) ${numeros.slice(2, 7)}-${numeros.slice(7, 11)}`;
}

function limparTelefone(valor: string): string {
  return valor.replace(/\D/g, '');
}

export default function RegistroScreen({ navigation }: any) {
  const { registrar } = useAuth();
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [confirmarSenha, setConfirmarSenha] = useState('');
  const [telefone, setTelefone] = useState('');
  const [carregando, setCarregando] = useState(false);
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [mostrarConfirmar, setMostrarConfirmar] = useState(false);

  function handleTelefoneChange(texto: string) {
    const numeros = texto.replace(/\D/g, '');
    if (numeros.length <= 11) {
      setTelefone(formatarTelefone(numeros));
    }
  }

  async function handleRegistro() {
    if (!nome.trim() || !email.trim() || !senha || !confirmarSenha) {
      Alert.alert('Atenção', 'Preencha todos os campos obrigatórios.');
      return;
    }

    if (senha !== confirmarSenha) {
      Alert.alert('Atenção', 'As senhas não conferem.');
      return;
    }

    if (senha.length < 6) {
      Alert.alert('Atenção', 'A senha deve ter no mínimo 6 caracteres.');
      return;
    }

    const telefoneLimpo = limparTelefone(telefone);

    setCarregando(true);
    const resultado = await registrar(
      nome.trim(),
      email.trim(),
      senha,
      confirmarSenha,
      telefoneLimpo || undefined
    );
    setCarregando(false);

    if (!resultado.sucesso) {
      Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao criar conta.');
    }
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
        <View style={styles.header}>
          <TouchableOpacity onPress={() => navigation.goBack()} style={styles.voltarBotao}>
            <Ionicons name="arrow-back" size={24} color={Colors.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.titulo}>Criar conta</Text>
          <Text style={styles.subtitulo}>Comece a controlar suas finanças!</Text>
        </View>

        <View style={styles.form}>
          {/* Nome */}
          <Text style={styles.label}>Nome completo *</Text>
          <View style={styles.inputContainer}>
            <Ionicons name="person-outline" size={20} color={Colors.textMuted} style={styles.inputIcone} />
            <TextInput
              style={styles.inputComIcone}
              placeholder="Seu nome completo"
              placeholderTextColor={Colors.textMuted}
              value={nome}
              onChangeText={setNome}
            />
          </View>

          {/* Email */}
          <Text style={styles.label}>E-mail *</Text>
          <View style={styles.inputContainer}>
            <Ionicons name="mail-outline" size={20} color={Colors.textMuted} style={styles.inputIcone} />
            <TextInput
              style={styles.inputComIcone}
              placeholder="seu@email.com"
              placeholderTextColor={Colors.textMuted}
              keyboardType="email-address"
              autoCapitalize="none"
              autoCorrect={false}
              value={email}
              onChangeText={setEmail}
            />
          </View>

          {/* WhatsApp */}
          <Text style={styles.label}>WhatsApp (opcional)</Text>
          <View style={styles.inputContainer}>
            <Ionicons name="logo-whatsapp" size={20} color="#25D366" style={styles.inputIcone} />
            <TextInput
              style={styles.inputComIcone}
              placeholder="(00) 00000-0000"
              placeholderTextColor={Colors.textMuted}
              keyboardType="phone-pad"
              value={telefone}
              onChangeText={handleTelefoneChange}
              maxLength={15}
            />
          </View>

          {/* Senha */}
          <Text style={styles.label}>Senha *</Text>
          <View style={styles.inputContainer}>
            <Ionicons name="lock-closed-outline" size={20} color={Colors.textMuted} style={styles.inputIcone} />
            <TextInput
              style={styles.inputComIcone}
              placeholder="Mínimo 6 caracteres"
              placeholderTextColor={Colors.textMuted}
              secureTextEntry={!mostrarSenha}
              value={senha}
              onChangeText={setSenha}
            />
            <TouchableOpacity onPress={() => setMostrarSenha(!mostrarSenha)} style={styles.inputIconeDireita}>
              <Ionicons
                name={mostrarSenha ? 'eye-off-outline' : 'eye-outline'}
                size={20}
                color={Colors.textMuted}
              />
            </TouchableOpacity>
          </View>

          {/* Confirmar Senha */}
          <Text style={styles.label}>Confirmar senha *</Text>
          <View style={styles.inputContainer}>
            <Ionicons name="lock-closed-outline" size={20} color={Colors.textMuted} style={styles.inputIcone} />
            <TextInput
              style={styles.inputComIcone}
              placeholder="Repita a senha"
              placeholderTextColor={Colors.textMuted}
              secureTextEntry={!mostrarConfirmar}
              value={confirmarSenha}
              onChangeText={setConfirmarSenha}
            />
            <TouchableOpacity onPress={() => setMostrarConfirmar(!mostrarConfirmar)} style={styles.inputIconeDireita}>
              <Ionicons
                name={mostrarConfirmar ? 'eye-off-outline' : 'eye-outline'}
                size={20}
                color={Colors.textMuted}
              />
            </TouchableOpacity>
          </View>

          {/* Botão Criar */}
          <TouchableOpacity
            style={[styles.botao, carregando && styles.botaoDisabled]}
            onPress={handleRegistro}
            disabled={carregando}
            activeOpacity={0.8}
          >
            {carregando ? (
              <Text style={styles.botaoTexto}>Criando conta...</Text>
            ) : (
              <View style={styles.botaoConteudo}>
                <Text style={styles.botaoTexto}>Criar conta</Text>
                <Ionicons name="arrow-forward" size={20} color={Colors.textWhite} />
              </View>
            )}
          </TouchableOpacity>
        </View>

        <TouchableOpacity
          style={styles.linkContainer}
          onPress={() => navigation.goBack()}
        >
          <Text style={styles.linkTexto}>
            Já possui uma conta? <Text style={styles.linkDestaque}>Fazer login</Text>
          </Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  scroll: {
    flexGrow: 1,
    padding: Spacing.lg,
  },
  header: {
    marginTop: 40,
    marginBottom: Spacing.lg,
  },
  voltarBotao: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: Colors.surfaceVariant,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: Spacing.md,
  },
  titulo: {
    fontSize: FontSize.xxl,
    fontWeight: '700',
    color: Colors.textPrimary,
  },
  subtitulo: {
    fontSize: FontSize.md,
    color: Colors.textSecondary,
    marginTop: Spacing.xs,
  },
  form: {
    backgroundColor: Colors.surface,
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
    color: Colors.textSecondary,
    marginBottom: Spacing.xs,
    marginTop: Spacing.md,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  inputContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: Colors.surfaceVariant,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: Colors.border,
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
    color: Colors.textPrimary,
  },
  botao: {
    backgroundColor: Colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md + 2,
    alignItems: 'center',
    marginTop: Spacing.lg,
  },
  botaoDisabled: {
    opacity: 0.6,
  },
  botaoConteudo: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  botaoTexto: {
    color: Colors.textWhite,
    fontSize: FontSize.lg,
    fontWeight: '700',
  },
  linkContainer: {
    alignItems: 'center',
    marginTop: Spacing.lg,
    marginBottom: Spacing.xl,
  },
  linkTexto: {
    fontSize: FontSize.md,
    color: Colors.textSecondary,
  },
  linkDestaque: {
    color: Colors.primary,
    fontWeight: '600',
  },
});