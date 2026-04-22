import React, { createContext, useContext, useEffect, useState } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import api from '../api/client';
import { AuthResponse, UsuarioResponse, Resultado } from '../types';

// Definir o que o contexto oferece pra todas as telas
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

// Criar o contexto
const AuthContext = createContext<AuthContextData>({} as AuthContextData);

// Provider que envolve o app inteiro
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioResponse | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(true);

  // Ao abrir o app, verificar se tem sessão salva
  useEffect(() => {
    carregarSessao();
  }, []);

  async function carregarSessao() {
    try {
      const tokenSalvo = await AsyncStorage.getItem('@finance:token');
      const usuarioSalvo = await AsyncStorage.getItem('@finance:usuario');

      if (tokenSalvo && usuarioSalvo) {
        setToken(tokenSalvo);
        setUsuario(JSON.parse(usuarioSalvo));
      }
    } catch (error) {
      console.log('Erro ao carregar sessão:', error);
    } finally {
      setCarregando(false);
    }
  }

  // Salvar sessão no celular
  async function salvarSessao(dados: AuthResponse) {
    await AsyncStorage.setItem('@finance:token', dados.token);
    await AsyncStorage.setItem('@finance:refreshToken', dados.refreshToken);
    await AsyncStorage.setItem('@finance:usuario', JSON.stringify(dados.usuario));

    setToken(dados.token);
    setUsuario(dados.usuario);
  }

  // Login
  async function login(email: string, senha: string): Promise<Resultado<AuthResponse>> {
    try {
      console.log('[Auth] Tentando login com:', email);
      const response = await api.post('/auth/login', { email, senha });
      const resultado: Resultado<AuthResponse> = response.data;

      console.log('[Auth] Resposta do login:', resultado);

      if (resultado.sucesso && resultado.dados) {
        await salvarSessao(resultado.dados);
      }

      return resultado;
    } catch (error: any) {
      console.error('[Auth] Erro no login:', error.message);
      if (error.response?.data) {
        console.error('[Auth] Resposta do servidor:', error.response.data);
        return error.response.data;
      }
      return {
        sucesso: false,
        erros: ['Erro de conexão. Verifique sua internet e se a API está rodando.'],
      };
    }
  }

  // Registrar
  async function registrar(
    nome: string,
    email: string,
    senha: string,
    confirmarSenha: string,
    telefone?: string
  ): Promise<Resultado<AuthResponse>> {
    try {
      console.log('[Auth] Tentando registrar:', { nome, email, telefone });
      const response = await api.post('/auth/registrar', {
        nomeCompleto: nome,
        email,
        senha,
        confirmarSenha,
        telefoneWhatsApp: telefone,
      });
      const resultado: Resultado<AuthResponse> = response.data;

      console.log('[Auth] Resposta do registro:', resultado);

      if (resultado.sucesso && resultado.dados) {
        await salvarSessao(resultado.dados);
      }

      return resultado;
    } catch (error: any) {
      console.error('[Auth] Erro no registro:', error.message);
      if (error.response?.data) {
        console.error('[Auth] Resposta do servidor:', error.response.data);
        return error.response.data;
      }
      return {
        sucesso: false,
        erros: ['Erro de conexão. Verifique sua internet e se a API está rodando.'],
      };
    }
  }

  // Logout
  async function logout() {
    try {
      await api.post('/auth/logout');
    } catch (error) {
      // Mesmo se falhar no servidor, limpa local
    }

    await AsyncStorage.multiRemove([
      '@finance:token',
      '@finance:refreshToken',
      '@finance:usuario',
    ]);

    setToken(null);
    setUsuario(null);
  }

  // Atualizar dados do perfil localmente
  function atualizarPerfil(usuarioAtualizado: UsuarioResponse) {
    setUsuario(usuarioAtualizado);
    AsyncStorage.setItem('@finance:usuario', JSON.stringify(usuarioAtualizado));
  }

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

// Hook pra usar em qualquer tela
export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider');
  }
  return context;
}