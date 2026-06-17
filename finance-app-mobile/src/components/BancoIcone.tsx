import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';

interface BancoInfo {
  sigla: string;
  cor: string;
  corTexto: string;
}

const BANCO_MAP: { keywords: string[]; info: BancoInfo }[] = [
  { keywords: ['nubank', 'nu pagamentos', 'nu '], info: { sigla: 'N', cor: '#8A05BE', corTexto: '#fff' } },
  { keywords: ['itaú', 'itau', 'unibanco'], info: { sigla: 'I', cor: '#EC7000', corTexto: '#fff' } },
  { keywords: ['bradesco'], info: { sigla: 'B', cor: '#CC092F', corTexto: '#fff' } },
  { keywords: ['santander'], info: { sigla: 'S', cor: '#EC0000', corTexto: '#fff' } },
  { keywords: ['caixa', 'cef', 'caixa econômica'], info: { sigla: 'C', cor: '#005CA9', corTexto: '#fff' } },
  { keywords: ['banco do brasil', 'bb '], info: { sigla: 'BB', cor: '#F9C200', corTexto: '#000' } },
  { keywords: ['inter'], info: { sigla: 'I', cor: '#FF7A00', corTexto: '#fff' } },
  { keywords: ['btg'], info: { sigla: 'BTG', cor: '#002050', corTexto: '#fff' } },
  { keywords: ['xp '], info: { sigla: 'XP', cor: '#1B1B1B', corTexto: '#fff' } },
  { keywords: ['sicoob'], info: { sigla: 'SC', cor: '#006835', corTexto: '#fff' } },
  { keywords: ['sicredi'], info: { sigla: 'SR', cor: '#00853E', corTexto: '#fff' } },
  { keywords: ['mercado pago', 'mercadopago'], info: { sigla: 'MP', cor: '#009EE3', corTexto: '#fff' } },
  { keywords: ['picpay'], info: { sigla: 'PP', cor: '#21C25E', corTexto: '#fff' } },
  { keywords: ['c6 bank', 'c6bank', 'c6'], info: { sigla: 'C6', cor: '#2E2E2E', corTexto: '#fff' } },
  { keywords: ['pagbank', 'pagseguro'], info: { sigla: 'PB', cor: '#F7941D', corTexto: '#fff' } },
  { keywords: ['banco original', 'original'], info: { sigla: 'OG', cor: '#00B140', corTexto: '#fff' } },
  { keywords: ['neon'], info: { sigla: 'NE', cor: '#00E5B3', corTexto: '#000' } },
  { keywords: ['will bank', 'will'], info: { sigla: 'WB', cor: '#FFDB00', corTexto: '#000' } },
  { keywords: ['modal'], info: { sigla: 'MD', cor: '#6C63FF', corTexto: '#fff' } },
  { keywords: ['avenue'], info: { sigla: 'AV', cor: '#1A1A2E', corTexto: '#fff' } },
  { keywords: ['warren'], info: { sigla: 'WR', cor: '#1DC7A8', corTexto: '#fff' } },
  { keywords: ['rico'], info: { sigla: 'RI', cor: '#FF5E00', corTexto: '#fff' } },
  { keywords: ['clear'], info: { sigla: 'CL', cor: '#F05323', corTexto: '#fff' } },
  { keywords: ['iti'], info: { sigla: 'iti', cor: '#E91E8C', corTexto: '#fff' } },
  { keywords: ['carteira', 'dinheiro', 'cash'], info: { sigla: '$', cor: '#27AE60', corTexto: '#fff' } },
  { keywords: ['poupança', 'poupanca', 'poupança'], info: { sigla: 'P', cor: '#2196F3', corTexto: '#fff' } },
  { keywords: ['investimento', 'corretora'], info: { sigla: 'IV', cor: '#9C27B0', corTexto: '#fff' } },
];

function detectarBanco(banco: string | null | undefined, nome: string): BancoInfo | null {
  const texto = ((banco || '') + ' ' + (nome || '')).toLowerCase();
  for (const item of BANCO_MAP) {
    if (item.keywords.some((kw) => texto.includes(kw))) {
      return item.info;
    }
  }
  return null;
}

interface Props {
  banco?: string | null;
  nome: string;
  corConta: string;
  size?: number;
}

export default function BancoIcone({ banco, nome, corConta, size = 44 }: Props) {
  const info = detectarBanco(banco, nome);
  const fontSize = size <= 36 ? 11 : size <= 44 ? 13 : 15;

  if (info) {
    return (
      <View
        style={[
          styles.badge,
          {
            width: size,
            height: size,
            borderRadius: size / 2,
            backgroundColor: info.cor,
          },
        ]}
      >
        <Text style={[styles.sigla, { color: info.corTexto, fontSize }]}>
          {info.sigla}
        </Text>
      </View>
    );
  }

  return (
    <View
      style={[
        styles.badge,
        {
          width: size,
          height: size,
          borderRadius: size / 2.5,
          backgroundColor: corConta + '20',
        },
      ]}
    >
      <Ionicons name="wallet" size={size * 0.5} color={corConta} />
    </View>
  );
}

const styles = StyleSheet.create({
  badge: {
    justifyContent: 'center',
    alignItems: 'center',
  },
  sigla: {
    fontWeight: '800',
    letterSpacing: -0.5,
  },
});
