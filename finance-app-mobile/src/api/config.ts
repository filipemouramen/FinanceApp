/**
 * Configuração de API para diferentes ambientes
 * 
 * COMO USAR:
 * 1. Se seu IP local mudou, atualize API_CONFIG.HOST_PC em client.ts
 * 2. Se a porta mudou, atualize API_CONFIG.PORT em client.ts
 * 
 * AMBIENTES SUPORTADOS:
 * - Web (npm run web): http://localhost:7137/api
 * - Expo Go no celular físico: http://192.168.1.5:7137/api (IP local)
 * - Emulador Android: http://10.0.2.2:7137/api
 * - Emulador iOS: http://localhost:7137/api
 */

// Se precisar adicionar hosts customizados, adicione aqui:
export const API_HOSTS = {
  LOCAL: 'localhost',
  PC_NETWORK: '192.168.1.5', // Seu IP local - ALTERE SE NECESSÁRIO
  ANDROID_EMULATOR: '10.0.2.2',
  IOS_EMULATOR: 'localhost',
};

export const API_PORT = 7137;
export const API_PROTOCOL = 'http'; // Sempre HTTP em desenvolvimento!
