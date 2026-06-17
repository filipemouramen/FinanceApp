import React, { createContext, useContext, useEffect, useState } from 'react';
import * as SecureStore from 'expo-secure-store';
import api from '../api/client';
import { AuthResponse, UsuarioResponse, Resultado } from '../types';

const CHAVE_TOKEN = 'finance_token';
const CHAVE_REFRESH = 'finance_refreshToken';
const CHAVE_USUARIO = 'finance_usuario';

interface AuthContextData {
  usuario: UsuarioResponse | null;
  token: string | null;
  carregando: boolean;
  logado: boolean;
  login: (email: string, senha: string) => Promise<Resultado<AuthResponse>>;
  registrar: (nome: string, email: string, senha: string, confirmarSenha: string, telefone?: string) => Promise<Resultado<AuthResponse>>;
  logout: () => Promise<void>;
  atualizarPerfil: (usuario: UsuarioResponse) => void;
}

const AuthContext = createContext<AuthContextData>({} as AuthContextData);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioResponse | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    carregarSessao();
  }, []);

  async function carregarSessao() {
    try {
      const tokenSalvo = await SecureStore.getItemAsync(CHAVE_TOKEN);
      const usuarioSalvo = await SecureStore.getItemAsync(CHAVE_USUARIO);

      if (tokenSalvo && usuarioSalvo) {
        setToken(tokenSalvo);
        setUsuario(JSON.parse(usuarioSalvo));
      }
    } catch {
      // sessão não encontrada ou corrompida — continua como deslogado
    } finally {
      setCarregando(false);
    }
  }

  async function salvarSessao(dados: AuthResponse) {
    try {
      await SecureStore.setItemAsync(CHAVE_TOKEN, dados.token);
      await SecureStore.setItemAsync(CHAVE_REFRESH, dados.refreshToken);
      await SecureStore.setItemAsync(CHAVE_USUARIO, JSON.stringify(dados.usuario));
    } catch {
      // SecureStore não disponível na plataforma web — sessão mantida apenas em memória
    }
    setToken(dados.token);
    setUsuario(dados.usuario);
  }

  async function login(email: string, senha: string): Promise<Resultado<AuthResponse>> {
    try {
      const response = await api.post('/auth/login', { email, senha });
      const resultado: Resultado<AuthResponse> = response.data;

      if (resultado.sucesso && resultado.dados) {
        await salvarSessao(resultado.dados);
      }

      return resultado;
    } catch (error: any) {
      if (error.response?.data) {
        return error.response.data;
      }
      return {
        sucesso: false,
        erros: ['Erro de conexão. Verifique sua internet e se a API está rodando.'],
      };
    }
  }

  async function registrar(
    nome: string,
    email: string,
    senha: string,
    confirmarSenha: string,
    telefone?: string
  ): Promise<Resultado<AuthResponse>> {
    try {
      const response = await api.post('/auth/registrar', {
        nomeCompleto: nome,
        email,
        senha,
        confirmarSenha,
        telefoneWhatsApp: telefone,
      });
      const resultado: Resultado<AuthResponse> = response.data;

      if (resultado.sucesso && resultado.dados) {
        await salvarSessao(resultado.dados);
      }

      return resultado;
    } catch (error: any) {
      if (error.response?.data) {
        return error.response.data;
      }
      return {
        sucesso: false,
        erros: ['Erro de conexão. Verifique sua internet e se a API está rodando.'],
      };
    }
  }

  async function logout() {
    try {
      await api.post('/auth/logout');
    } catch {
      // mesmo se falhar no servidor, limpa localmente
    }

    try {
      await SecureStore.deleteItemAsync(CHAVE_TOKEN);
      await SecureStore.deleteItemAsync(CHAVE_REFRESH);
      await SecureStore.deleteItemAsync(CHAVE_USUARIO);
    } catch { /* web */ }
    setToken(null);
    setUsuario(null);
  }

  function atualizarPerfil(usuarioAtualizado: UsuarioResponse) {
    setUsuario(usuarioAtualizado);
    SecureStore.setItemAsync(CHAVE_USUARIO, JSON.stringify(usuarioAtualizado)).catch(() => {});
  }

  // Hook de teste (apenas dev) — permite que Playwright defina sessão diretamente
  useEffect(() => {
    if (__DEV__) {
      (window as any).__testSetAuth = (dados: AuthResponse) => salvarSessao(dados);
    }
  }, []);

  return (
    <AuthContext.Provider
      value={{
        usuario,
        token,
        carregando,
        logado: !!token,
        login,
        registrar,
        logout,
        atualizarPerfil,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider');
  }
  return context;
}
