import axios from 'axios';
import * as SecureStore from 'expo-secure-store';
import { API_BASE_URL } from './config';

const CHAVE_TOKEN = 'finance_token';
const CHAVE_REFRESH = 'finance_refreshToken';
const CHAVE_USUARIO = 'finance_usuario';

const API_URL = `${API_BASE_URL}/api`;

const api = axios.create({
  baseURL: API_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use(
  async (config) => {
    const token = await SecureStore.getItemAsync(CHAVE_TOKEN);
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const token = await SecureStore.getItemAsync(CHAVE_TOKEN);
        const refreshToken = await SecureStore.getItemAsync(CHAVE_REFRESH);
        if (token && refreshToken) {
          const response = await axios.post(`${API_URL}/auth/refresh-token`, {
            token,
            refreshToken,
          });

          const { dados } = response.data;
          if (dados) {
            await SecureStore.setItemAsync(CHAVE_TOKEN, dados.token);
            await SecureStore.setItemAsync(CHAVE_REFRESH, dados.refreshToken);
            originalRequest.headers.Authorization = `Bearer ${dados.token}`;
            return api(originalRequest);
          }
        }
      } catch {
        await SecureStore.deleteItemAsync(CHAVE_TOKEN);
        await SecureStore.deleteItemAsync(CHAVE_REFRESH);
        await SecureStore.deleteItemAsync(CHAVE_USUARIO);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
