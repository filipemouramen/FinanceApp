import React from 'react';
import { ActivityIndicator, View } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Ionicons } from '@expo/vector-icons';
import { useAuth } from '../contexts/AuthContext';
import { Colors } from '../theme/colors';

// Telas Auth
import LoginScreen from '../screens/Auth/LoginScreen';
import RegistroScreen from '../screens/Auth/RegistroScreen';

// Telas principais
import HomeScreen from '../screens/Home/HomeScreen';
import TransacoesScreen from '../screens/Transacoes/TransacoesScreen';
import CriarTransacaoScreen from '../screens/Transacoes/CriarTransacaoScreen';
import ContasScreen from '../screens/Contas/ContasScreen';
import ConfigScreen from '../screens/Config/ConfigScreen';

const Stack = createNativeStackNavigator();
const Tab = createBottomTabNavigator();

function TabNavigator() {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        headerShown: false,
        tabBarActiveTintColor: Colors.primary,
        tabBarInactiveTintColor: Colors.textMuted,
        tabBarStyle: {
          backgroundColor: Colors.surface,
          borderTopColor: Colors.borderLight,
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
          else if (route.name === 'Config') iconName = focused ? 'settings' : 'settings-outline';

          return <Ionicons name={iconName} size={22} color={color} />;
        },
      })}
    >
      <Tab.Screen name="Home" component={HomeScreen} options={{ tabBarLabel: 'Início' }} />
      <Tab.Screen name="Transacoes" component={TransacoesScreen} options={{ tabBarLabel: 'Transações' }} />
      <Tab.Screen name="Contas" component={ContasScreen} options={{ tabBarLabel: 'Contas' }} />
      <Tab.Screen name="Config" component={ConfigScreen} options={{ tabBarLabel: 'Config' }} />
    </Tab.Navigator>
  );
}

export default function AppNavigator() {
  const { logado, carregando } = useAuth();

  if (carregando) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: Colors.background }}>
        <ActivityIndicator size="large" color={Colors.primary} />
      </View>
    );
  }

  return (
    <NavigationContainer>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        {logado ? (
          <>
            <Stack.Screen name="Main" component={TabNavigator} />
            <Stack.Screen
              name="CriarTransacao"
              component={CriarTransacaoScreen}
              options={{ animation: 'slide_from_bottom' }}
            />
          </>
        ) : (
          <>
            <Stack.Screen name="Login" component={LoginScreen} />
            <Stack.Screen name="Registro" component={RegistroScreen} />
          </>
        )}
      </Stack.Navigator>
    </NavigationContainer>
  );
}