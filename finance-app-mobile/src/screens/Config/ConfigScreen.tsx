import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Switch,
  Alert,
  RefreshControl,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect } from '@react-navigation/native';
import { useAuth } from '../../contexts/AuthContext';
import api from '../../api/client';
import { ConfiguracaoResponse, Resultado } from '../../types';
import { Colors, Spacing, FontSize, BorderRadius } from '../../theme/colors';

export default function ConfigScreen() {
  const { usuario, logout } = useAuth();
  const [config, setConfig] = useState<ConfiguracaoResponse | null>(null);
  const [carregando, setCarregando] = useState(true);

  useFocusEffect(
    useCallback(() => {
      carregarConfiguracoes();
    }, [])
  );

  async function carregarConfiguracoes() {
    try {
      setCarregando(true);
      const response = await api.get('/configuracoes');
      const resultado: Resultado<ConfiguracaoResponse> = response.data;
      if (resultado.sucesso && resultado.dados) {
        setConfig(resultado.dados);
      }
    } catch (error) {
      console.log('Erro ao carregar configurações:', error);
    } finally {
      setCarregando(false);
    }
  }

  async function atualizarConfig(campo: Partial<ConfiguracaoResponse>) {
    try {
      const response = await api.put('/configuracoes', campo);
      const resultado: Resultado<ConfiguracaoResponse> = response.data;
      if (resultado.sucesso && resultado.dados) {
        setConfig(resultado.dados);
      } else {
        Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao atualizar.');
        carregarConfiguracoes();
      }
    } catch (error: any) {
      Alert.alert('Erro', error.response?.data?.erros?.[0] || 'Erro de conexão.');
      carregarConfiguracoes();
    }
  }

  function handleLogout() {
    Alert.alert(
      'Sair da conta',
      'Tem certeza que deseja sair?',
      [
        { text: 'Cancelar', style: 'cancel' },
        { text: 'Sair', style: 'destructive', onPress: logout },
      ]
    );
  }

  function getIniciais(nome: string): string {
    return nome
      .split(' ')
      .map((n) => n[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  return (
    <ScrollView
      style={styles.container}
      refreshControl={
        <RefreshControl refreshing={carregando} onRefresh={carregarConfiguracoes} colors={[Colors.primary]} />
      }
      showsVerticalScrollIndicator={false}
    >
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitulo}>Configurações</Text>
      </View>

      {/* Card Perfil */}
      <View style={styles.perfilCard}>
        <View style={styles.avatar}>
          <Text style={styles.avatarTexto}>
            {getIniciais(usuario?.nomeCompleto || 'U')}
          </Text>
        </View>
        <View style={styles.perfilInfo}>
          <Text style={styles.perfilNome}>{usuario?.nomeCompleto}</Text>
          <Text style={styles.perfilEmail}>{usuario?.email}</Text>
          {usuario?.telefoneWhatsApp && (
            <View style={styles.perfilTelContainer}>
              <Ionicons name="logo-whatsapp" size={14} color={Colors.success} />
              <Text style={styles.perfilTelefone}>{usuario.telefoneWhatsApp}</Text>
            </View>
          )}
        </View>
      </View>

      {/* Seção: Preferências */}
      <Text style={styles.secaoTitulo}>Preferências</Text>
      <View style={styles.secaoCard}>
        <View style={styles.configItem}>
          <View style={styles.configEsquerda}>
            <Ionicons name="moon-outline" size={22} color={Colors.textSecondary} />
            <Text style={styles.configTexto}>Modo escuro</Text>
          </View>
          <Switch
            value={config?.modoEscuro || false}
            onValueChange={(valor) => atualizarConfig({ modoEscuro: valor })}
            trackColor={{ false: Colors.border, true: Colors.primary }}
            thumbColor={Colors.textWhite}
          />
        </View>

        <View style={styles.separador} />

        <View style={styles.configItem}>
          <View style={styles.configEsquerda}>
            <Ionicons name="calendar-outline" size={22} color={Colors.textSecondary} />
            <View>
              <Text style={styles.configTexto}>Dia de início do mês</Text>
              <Text style={styles.configSub}>Dia {config?.diaInicioMes || 1}</Text>
            </View>
          </View>
        </View>

        <View style={styles.separador} />

        <View style={styles.configItem}>
          <View style={styles.configEsquerda}>
            <Ionicons name="cash-outline" size={22} color={Colors.textSecondary} />
            <View>
              <Text style={styles.configTexto}>Moeda</Text>
              <Text style={styles.configSub}>{config?.moeda || 'BRL'}</Text>
            </View>
          </View>
        </View>
      </View>

      {/* Seção: Notificações */}
      <Text style={styles.secaoTitulo}>Notificações</Text>
      <View style={styles.secaoCard}>
        <View style={styles.configItem}>
          <View style={styles.configEsquerda}>
            <Ionicons name="notifications-outline" size={22} color={Colors.textSecondary} />
            <Text style={styles.configTexto}>Notificações push</Text>
          </View>
          <Switch
            value={config?.notificacoesPush || false}
            onValueChange={(valor) => atualizarConfig({ notificacoesPush: valor })}
            trackColor={{ false: Colors.border, true: Colors.primary }}
            thumbColor={Colors.textWhite}
          />
        </View>

        <View style={styles.separador} />

        <View style={styles.configItem}>
          <View style={styles.configEsquerda}>
            <Ionicons name="pie-chart-outline" size={22} color={Colors.textSecondary} />
            <Text style={styles.configTexto}>Alertas de orçamento</Text>
          </View>
          <Switch
            value={config?.alertasOrcamento || false}
            onValueChange={(valor) => atualizarConfig({ alertasOrcamento: valor })}
            trackColor={{ false: Colors.border, true: Colors.primary }}
            thumbColor={Colors.textWhite}
          />
        </View>

        <View style={styles.separador} />

        <View style={styles.configItem}>
          <View style={styles.configEsquerda}>
            <Ionicons name="card-outline" size={22} color={Colors.textSecondary} />
            <Text style={styles.configTexto}>Alertas de fatura</Text>
          </View>
          <Switch
            value={config?.alertasFatura || false}
            onValueChange={(valor) => atualizarConfig({ alertasFatura: valor })}
            trackColor={{ false: Colors.border, true: Colors.primary }}
            thumbColor={Colors.textWhite}
          />
        </View>
      </View>

      {/* Seção: Integrações */}
      <Text style={styles.secaoTitulo}>Integrações</Text>
      <View style={styles.secaoCard}>
        <View style={styles.configItem}>
          <View style={styles.configEsquerda}>
            <Ionicons name="logo-whatsapp" size={22} color="#25D366" />
            <View>
              <Text style={styles.configTexto}>WhatsApp Bot</Text>
              <Text style={styles.configSub}>Registre gastos por mensagem</Text>
            </View>
          </View>
          <Switch
            value={config?.whatsAppAtivado || false}
            onValueChange={(valor) => atualizarConfig({ whatsAppAtivado: valor })}
            trackColor={{ false: Colors.border, true: '#25D366' }}
            thumbColor={Colors.textWhite}
          />
        </View>
      </View>

      {/* Seção: Sobre */}
      <Text style={styles.secaoTitulo}>Sobre</Text>
      <View style={styles.secaoCard}>
        <View style={styles.configItem}>
          <View style={styles.configEsquerda}>
            <Ionicons name="information-circle-outline" size={22} color={Colors.textSecondary} />
            <View>
              <Text style={styles.configTexto}>Versão do app</Text>
              <Text style={styles.configSub}>1.0.0</Text>
            </View>
          </View>
        </View>
      </View>

      {/* Botão Logout */}
      <TouchableOpacity style={styles.botaoLogout} onPress={handleLogout}>
        <Ionicons name="log-out-outline" size={22} color={Colors.danger} />
        <Text style={styles.botaoLogoutTexto}>Sair da conta</Text>
      </TouchableOpacity>

      <View style={{ height: 120 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  header: {
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
  // Perfil
  perfilCard: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: Colors.surface,
    margin: Spacing.lg,
    padding: Spacing.lg,
    borderRadius: BorderRadius.lg,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.06,
    shadowRadius: 8,
    elevation: 3,
    gap: 16,
  },
  avatar: {
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: Colors.primary,
    justifyContent: 'center',
    alignItems: 'center',
  },
  avatarTexto: {
    fontSize: FontSize.xxl,
    fontWeight: '700',
    color: Colors.textWhite,
  },
  perfilInfo: {
    flex: 1,
  },
  perfilNome: {
    fontSize: FontSize.xl,
    fontWeight: '700',
    color: Colors.textPrimary,
  },
  perfilEmail: {
    fontSize: FontSize.md,
    color: Colors.textSecondary,
    marginTop: 2,
  },
  perfilTelContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    marginTop: 4,
  },
  perfilTelefone: {
    fontSize: FontSize.sm,
    color: Colors.success,
  },
  // Seções
  secaoTitulo: {
    fontSize: FontSize.sm,
    fontWeight: '700',
    color: Colors.textMuted,
    textTransform: 'uppercase',
    letterSpacing: 1,
    marginHorizontal: Spacing.lg,
    marginTop: Spacing.lg,
    marginBottom: Spacing.sm,
  },
  secaoCard: {
    backgroundColor: Colors.surface,
    marginHorizontal: Spacing.lg,
    borderRadius: BorderRadius.lg,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.04,
    shadowRadius: 4,
    elevation: 2,
  },
  configItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: Spacing.md,
  },
  configEsquerda: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    flex: 1,
  },
  configTexto: {
    fontSize: FontSize.md,
    color: Colors.textPrimary,
    fontWeight: '500',
  },
  configSub: {
    fontSize: FontSize.sm,
    color: Colors.textMuted,
    marginTop: 2,
  },
  separador: {
    height: 1,
    backgroundColor: Colors.borderLight,
    marginHorizontal: Spacing.md,
  },
  // Logout
  botaoLogout: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    marginHorizontal: Spacing.lg,
    marginTop: Spacing.xl,
    padding: Spacing.md,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: Colors.danger,
  },
  botaoLogoutTexto: {
    fontSize: FontSize.lg,
    fontWeight: '600',
    color: Colors.danger,
  },
});