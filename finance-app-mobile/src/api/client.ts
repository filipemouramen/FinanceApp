import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { Platform } from 'react-native';

// ===== CONFIGURAÇÃO DE API =====
// Funciona em:
// - Web (npm run web): http://localhost:7137/api
// - Expo Go no celular: http://192.168.1.5:7137/api
// - Emulador Android: http://10.0.2.2:7137/api
// - Emulador iOS: http://localhost:7137/api

const API_CONFIG = {
  HOST_PC: '192.168.1.5', // Seu IP local - AJUSTE SE NECESSÁRIO
  PORT: 7137,
  PROTOCOL: 'http',
};

let API_URL: string;

if (__DEV__) {
  // Desenvolvimento - detecta automaticamente baseado no platform
  if (Platform.OS === 'android') {
    // Emulador Android acessa o host via 10.0.2.2
    API_URL = `${API_CONFIG.PROTOCOL}://10.0.2.2:${API_CONFIG.PORT}/api`;
    console.log('[API] 🤖 Emulador Android detectado');
  } else if (Platform.OS === 'ios') {
    // iOS real (Expo Go) - usar IP local
    // iOS emulador no Mac - mudar manualmente para localhost
    API_URL = `${API_CONFIG.PROTOCOL}://${API_CONFIG.HOST_PC}:${API_CONFIG.PORT}/api`;
    console.log('[API] 🍎 iOS detectado (Expo Go no celular)');
  } else if (Platform.OS === 'web') {
    // Web (Expo Web / navegador) usa localhost
    API_URL = `${API_CONFIG.PROTOCOL}://localhost:${API_CONFIG.PORT}/api`;
    console.log('[API] 🌐 Web (navegador) detectado');
  } else {
    // Fallback para outros casos
    API_URL = `${API_CONFIG.PROTOCOL}://${API_CONFIG.HOST_PC}:${API_CONFIG.PORT}/api`;
    console.log('[API] 📱 Plataforma desconhecida - usando IP local');
  }
} else {
  // Produção
  API_URL = 'https://seudominio.com/api'; // alterar para sua URL de produção
  console.log('[API] 🚀 Produção');
}

console.log('[API Config] URL:', API_URL);
console.log('[API Config] Platform.OS:', Platform.OS || 'unknown');

const api = axios.create({
  baseURL: API_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
  // Ignorar erro de certificado SSL em desenvolvimento
  validateStatus: function (status) {
    return status >= 200 && status < 500;
  },
});

api.interceptors.request.use(
  async (config) => {
    const token = await AsyncStorage.getItem('@finance:token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    console.log(`[API] 📤 ${config.method?.toUpperCase()} ${config.url}`);
    return config;
  },
  (error) => {
    console.error('[API] ❌ Erro na requisição:', error.message);
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => {
    console.log(`[API] ✅ Resposta ${response.status}: ${response.config.url}`);
    return response;
  },
  async (error) => {
    const url = error.config?.url || 'desconhecida';
    
    if (!error.response) {
      // Erro de rede (sem resposta do servidor)
      console.error('[API] ❌ ERRO DE REDE - A API não respondeu');
      console.error('[API] Verifique:');
      console.error('  1. A API está rodando em http://localhost:7137?');
      console.error('  2. Há bloqueio de CORS?');
      console.error('  3. A API está respondendo a requisições HTTP?');
      console.error('[API] Erro:', error.message);
    } else {
      console.error(`[API] ❌ Resposta ${error.response.status}: ${url}`);
      console.error('[API] Dados do erro:', error.response.data);
    }
    
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const token = await AsyncStorage.getItem('@finance:token');
        const refreshToken = await AsyncStorage.getItem('@finance:refreshToken');
        if (token && refreshToken) {
          const response = await axios.post(`${API_URL}/auth/refresh-token`, {
            token,
            refreshToken,
          });

          const { dados } = response.data;

          if (dados) {
            await AsyncStorage.setItem('@finance:token', dados.token);
            await AsyncStorage.setItem('@finance:refreshToken', dados.refreshToken);

            originalRequest.headers.Authorization = `Bearer ${dados.token}`;
            return api(originalRequest);
          }
        }
      } catch (refreshError) {
        console.error('[API] ❌ Erro ao renovar token:', refreshError);
        await AsyncStorage.multiRemove([
          '@finance:token',
          '@finance:refreshToken',
          '@finance:usuario',
        ]);
      }
    }

    return Promise.reject(error);
  }
);

export default api;