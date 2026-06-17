import React, { useEffect, useRef } from 'react';
import { ActivityIndicator, View } from 'react-native';
import { NavigationContainer, NavigationContainerRef, CommonActions } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Ionicons } from '@expo/vector-icons';
import { useAuth } from '../contexts/AuthContext';
import { useTheme } from '../theme/useTheme';

// Telas Auth
import LoginScreen from '../screens/Auth/LoginScreen';
import RegistroScreen from '../screens/Auth/RegistroScreen';
import EsqueciSenhaScreen from '../screens/Auth/EsqueciSenhaScreen';
import VerificarCodigoScreen from '../screens/Auth/VerificarCodigoScreen';
import NovaSenhaScreen from '../screens/Auth/NovaSenhaScreen';

// Telas principais
import HomeScreen from '../screens/Home/HomeScreen';
import TransacoesScreen from '../screens/Transacoes/TransacoesScreen';
import CriarTransacaoScreen from '../screens/Transacoes/CriarTransacaoScreen';
import ContasScreen from '../screens/Contas/ContasScreen';
import ConfigScreen from '../screens/Config/ConfigScreen';
import TransferenciaScreen from '../screens/Transferencias/TransferenciaScreen';
import CartoesScreen from '../screens/Cartoes/CartoesScreen';
import CriarCartaoScreen from '../screens/Cartoes/CriarCartaoScreen';
import FaturaDetalheScreen from '../screens/Cartoes/FaturaDetalheScreen';
import NotificacoesScreen from '../screens/Notificacoes/NotificacoesScreen';
import OrcamentosScreen from '../screens/Orcamentos/OrcamentosScreen';
import MetasScreen from '../screens/Metas/MetasScreen';
import CategoriasScreen from '../screens/Categorias/CategoriasScreen';

const Stack = createNativeStackNavigator();
const Tab = createBottomTabNavigator();

function TabNavigator() {
  const { colors } = useTheme();
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.textMuted,
        tabBarStyle: {
          backgroundColor: colors.surface,
          borderTopColor: colors.borderLight,
          height: 60,
          paddingBottom: 8,
          paddingTop: 8,
        },
        tabBarLabelStyle: {
          fontSize: 11,
          fontWeight: '500',
        },
        tabBarIcon: ({ focused, color }) => {
          let iconName: keyof typeof Ionicons.glyphMap = 'home';

          if (route.name === 'Home') iconName = focused ? 'home' : 'home-outline';
          else if (route.name === 'Transacoes') iconName = focused ? 'swap-vertical' : 'swap-vertical-outline';
          else if (route.name === 'Contas') iconName = focused ? 'wallet' : 'wallet-outline';
          else if (route.name === 'Cartoes') iconName = focused ? 'card' : 'card-outline';
          else if (route.name === 'Config') iconName = focused ? 'settings' : 'settings-outline';

          return <Ionicons name={iconName} size={22} color={color} />;
        },
      })}
    >
      <Tab.Screen name="Home" component={HomeScreen} options={{ tabBarLabel: 'Início' }} />
      <Tab.Screen name="Transacoes" component={TransacoesScreen} options={{ tabBarLabel: 'Transações' }} />
      <Tab.Screen name="Contas" component={ContasScreen} options={{ tabBarLabel: 'Contas' }} />
      <Tab.Screen name="Cartoes" component={CartoesScreen} options={{ tabBarLabel: 'Cartões' }} />
      <Tab.Screen name="Config" component={ConfigScreen} options={{ tabBarLabel: 'Config' }} />
    </Tab.Navigator>
  );
}

export default function AppNavigator() {
  const { logado, carregando } = useAuth();
  const navRef = useRef<NavigationContainerRef<any>>(null);

  useEffect(() => {
    if (__DEV__) {
      (window as any).__testGoBack = () => {
        if (navRef.current?.isReady()) navRef.current.goBack();
      };
      (window as any).__testNavigate = (name: string, params?: any) => {
        if (navRef.current?.isReady()) navRef.current.navigate(name, params);
      };
    }
  }, []);

  if (carregando) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#F8F9FA' }}>
        <ActivityIndicator size="large" color="#6C63FF" />
      </View>
    );
  }

  return (
    <NavigationContainer ref={navRef}>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        {logado ? (
          <>
            <Stack.Screen name="Main" component={TabNavigator} />
            <Stack.Screen
              name="CriarTransacao"
              component={CriarTransacaoScreen}
              options={{ animation: 'slide_from_bottom' }}
            />
            <Stack.Screen
              name="Transferencia"
              component={TransferenciaScreen}
              options={{ animation: 'slide_from_bottom' }}
            />
            <Stack.Screen name="CriarCartao" component={CriarCartaoScreen} options={{ animation: 'slide_from_bottom' }} />
            <Stack.Screen name="FaturaDetalhe" component={FaturaDetalheScreen} />
            <Stack.Screen name="Notificacoes" component={NotificacoesScreen} />
            <Stack.Screen name="Orcamentos" component={OrcamentosScreen} />
            <Stack.Screen name="Metas" component={MetasScreen} />
            <Stack.Screen name="Categorias" component={CategoriasScreen} />
          </>
        ) : (
          <>
            <Stack.Screen name="Login" component={LoginScreen} />
            <Stack.Screen name="Registro" component={RegistroScreen} />
            <Stack.Screen name="EsqueciSenha" component={EsqueciSenhaScreen} />
            <Stack.Screen name="VerificarCodigo" component={VerificarCodigoScreen} />
            <Stack.Screen name="NovaSenha" component={NovaSenhaScreen} />
          </>
        )}
      </Stack.Navigator>
    </NavigationContainer>
  );
}