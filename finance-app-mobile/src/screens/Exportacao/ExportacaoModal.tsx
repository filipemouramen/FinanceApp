import React, { useState } from 'react';
import {
  View,
  Text,
  Modal,
  TouchableOpacity,
  StyleSheet,
  Alert,
  ActivityIndicator,
  Platform,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import DateTimePicker from '@react-native-community/datetimepicker';
import * as FileSystem from 'expo-file-system';
import * as Sharing from 'expo-sharing';
import api from '../../api/client';
import { useTheme } from '../../theme/useTheme';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';

interface Props {
  visivel: boolean;
  onFechar: () => void;
}

export default function ExportacaoModal({ visivel, onFechar }: Props) {
  const { colors } = useTheme();
  const styles = getStyles(colors);

  const hoje = new Date();
  const mesPassado = new Date(hoje);
  mesPassado.setMonth(mesPassado.getMonth() - 1);

  const [dataInicio, setDataInicio] = useState(mesPassado);
  const [dataFim, setDataFim] = useState(hoje);
  const [carregando, setCarregando] = useState(false);

  // pickerAtivo: qual campo está sendo editado ('inicio' | 'fim' | null)
  const [pickerAtivo, setPickerAtivo] = useState<'inicio' | 'fim' | null>(null);
  // dataTmp: valor temporário enquanto o usuário rola a roleta (iOS)
  const [dataTmp, setDataTmp] = useState(new Date());

  function formatarData(d: Date): string {
    return d.toLocaleDateString('pt-BR');
  }

  function abrirPicker(campo: 'inicio' | 'fim') {
    setDataTmp(campo === 'inicio' ? dataInicio : dataFim);
    setPickerAtivo(campo);
  }

  function confirmarData() {
    if (pickerAtivo === 'inicio') setDataInicio(dataTmp);
    else if (pickerAtivo === 'fim') setDataFim(dataTmp);
    setPickerAtivo(null);
  }

  async function handleExportar() {
    const diffDias = (dataFim.getTime() - dataInicio.getTime()) / (1000 * 60 * 60 * 24);
    if (diffDias > 92) {
      Alert.alert('Atenção', 'O período máximo para exportação é de 3 meses.');
      return;
    }

    setCarregando(true);
    try {
      const response = await api.post(
        '/exportacao/pdf',
        { dataInicio: dataInicio.toISOString(), dataFim: dataFim.toISOString() }
      );

      const { base64, nomeArquivo } = response.data.dados;
      const caminho = `${FileSystem.cacheDirectory}${nomeArquivo}`;

      await FileSystem.writeAsStringAsync(caminho, base64, {
        encoding: FileSystem.EncodingType.Base64,
      });

      const disponivel = await Sharing.isAvailableAsync();
      if (disponivel) {
        await Sharing.shareAsync(caminho, { mimeType: 'application/pdf', dialogTitle: 'Compartilhar extrato PDF' });
      } else {
        Alert.alert('Sucesso', 'PDF gerado com sucesso.');
      }

      await FileSystem.deleteAsync(caminho, { idempotent: true });
      onFechar();
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível gerar o PDF.');
    } finally {
      setCarregando(false);
    }
  }

  const dataPickerAtual = pickerAtivo === 'inicio' ? dataInicio : dataFim;
  const minDate = pickerAtivo === 'fim' ? dataInicio : undefined;
  const maxDate = pickerAtivo === 'inicio' ? dataFim : hoje;

  return (
    <>
      {/* Modal principal — bottom sheet */}
      <Modal visible={visivel} animationType="slide" transparent onRequestClose={onFechar}>
        <View style={styles.overlay}>
          <View style={styles.container}>
            <View style={styles.cabecalho}>
              <Text style={styles.titulo}>Exportar extrato em PDF</Text>
              <TouchableOpacity onPress={onFechar}>
                <Ionicons name="close" size={24} color={colors.textPrimary} />
              </TouchableOpacity>
            </View>

            <Text style={styles.descricao}>
              Selecione o período (máximo 3 meses) para gerar o extrato.
            </Text>

            <Text style={styles.label}>Data de início</Text>
            <TouchableOpacity style={styles.dataSeletor} onPress={() => abrirPicker('inicio')}>
              <Ionicons name="calendar-outline" size={20} color={colors.primary} />
              <Text style={styles.dataTexto}>{formatarData(dataInicio)}</Text>
              <Ionicons name="chevron-down" size={18} color={colors.textMuted} />
            </TouchableOpacity>

            <Text style={styles.label}>Data de fim</Text>
            <TouchableOpacity style={styles.dataSeletor} onPress={() => abrirPicker('fim')}>
              <Ionicons name="calendar-outline" size={20} color={colors.primary} />
              <Text style={styles.dataTexto}>{formatarData(dataFim)}</Text>
              <Ionicons name="chevron-down" size={18} color={colors.textMuted} />
            </TouchableOpacity>

            <View style={styles.resumoPeriodo}>
              <Ionicons name="calendar-outline" size={16} color={colors.textMuted} />
              <Text style={styles.resumoTexto}>
                {formatarData(dataInicio)} → {formatarData(dataFim)}
                {' '}({Math.round((dataFim.getTime() - dataInicio.getTime()) / (1000 * 60 * 60 * 24))} dias)
              </Text>
            </View>

            <TouchableOpacity
              style={[styles.botao, carregando && styles.botaoDisabled]}
              onPress={handleExportar}
              disabled={carregando}
            >
              {carregando ? (
                <ActivityIndicator color={colors.textWhite} size="small" />
              ) : (
                <>
                  <Ionicons name="document-text-outline" size={20} color={colors.textWhite} />
                  <Text style={styles.botaoTexto}>Gerar PDF</Text>
                </>
              )}
            </TouchableOpacity>
          </View>
        </View>
      </Modal>

      {/* Modal separado para o picker — fora do Modal principal (resolve bug iOS) */}
      {Platform.OS === 'ios' ? (
        <Modal
          visible={pickerAtivo !== null}
          transparent
          animationType="slide"
          onRequestClose={() => setPickerAtivo(null)}
        >
          <View style={styles.pickerOverlay}>
            <View style={styles.pickerContainer}>
              <View style={styles.pickerHeader}>
                <TouchableOpacity onPress={() => setPickerAtivo(null)}>
                  <Text style={styles.pickerCancelar}>Cancelar</Text>
                </TouchableOpacity>
                <Text style={styles.pickerTitulo}>
                  {pickerAtivo === 'inicio' ? 'Data de início' : 'Data de fim'}
                </Text>
                <TouchableOpacity onPress={confirmarData}>
                  <Text style={styles.pickerConfirmar}>Confirmar</Text>
                </TouchableOpacity>
              </View>
              <DateTimePicker
                value={dataTmp}
                mode="date"
                display="spinner"
                minimumDate={minDate}
                maximumDate={maxDate}
                onChange={(_, date) => { if (date) setDataTmp(date); }}
                locale="pt-BR"
              />
            </View>
          </View>
        </Modal>
      ) : (
        pickerAtivo !== null && (
          <DateTimePicker
            value={dataPickerAtual}
            mode="date"
            display="default"
            minimumDate={minDate}
            maximumDate={maxDate}
            onChange={(_, date) => {
              setPickerAtivo(null);
              if (date) {
                if (pickerAtivo === 'inicio') setDataInicio(date);
                else setDataFim(date);
              }
            }}
          />
        )
      )}
    </>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'flex-end',
  },
  container: {
    backgroundColor: colors.surface,
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    padding: Spacing.xl,
    paddingBottom: Platform.OS === 'ios' ? 40 : Spacing.xl,
  },
  cabecalho: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: Spacing.md,
  },
  titulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  descricao: { fontSize: FontSize.md, color: colors.textSecondary, marginBottom: Spacing.lg },
  label: {
    fontSize: FontSize.sm,
    fontWeight: '600',
    color: colors.textSecondary,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
    marginBottom: Spacing.xs,
    marginTop: Spacing.md,
  },
  dataSeletor: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    backgroundColor: colors.surfaceVariant,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: colors.border,
    padding: Spacing.sm,
  },
  dataTexto: { fontSize: FontSize.lg, fontWeight: '600', color: colors.textPrimary, flex: 1 },
  resumoPeriodo: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    marginTop: Spacing.md,
    backgroundColor: colors.primary + '10',
    borderRadius: BorderRadius.sm,
    padding: Spacing.sm,
  },
  resumoTexto: { fontSize: FontSize.sm, color: colors.textSecondary },
  botao: {
    backgroundColor: colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    marginTop: Spacing.xl,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoTexto: { color: colors.textWhite, fontSize: FontSize.lg, fontWeight: '600' },
  // Picker modal (iOS)
  pickerOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.4)',
    justifyContent: 'flex-end',
  },
  pickerContainer: {
    backgroundColor: colors.surface,
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
    paddingBottom: 34,
  },
  pickerHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: Spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
  },
  pickerTitulo: { fontSize: FontSize.md, fontWeight: '600', color: colors.textPrimary },
  pickerCancelar: { fontSize: FontSize.md, color: colors.textSecondary },
  pickerConfirmar: { fontSize: FontSize.md, fontWeight: '700', color: colors.primary },
});
