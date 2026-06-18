import Constants from 'expo-constants';

function getApiUrl(): string {
  // Em web, a API está no mesmo host (localhost)
  if (typeof window !== 'undefined' && typeof document !== 'undefined') {
    return 'http://localhost:7137';
  }

  // No Expo Go, hostUri é o endereço do dev server (ex: "192.168.1.x:8081")
  // Extrai o IP e usa na porta da API
  const hostUri = Constants.expoConfig?.hostUri ?? Constants.manifest?.debuggerHost;
  if (hostUri) {
    const ip = hostUri.split(':')[0];
    return `http://${ip}:7137`;
  }

  // Fallback para emulador Android
  return 'http://10.0.2.2:7137';
}

export const API_BASE_URL = getApiUrl();
