import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  Modal,
  ScrollView,
  TouchableOpacity,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { CategoriaResponse } from '../../types';
import { Colors, Spacing, FontSize, BorderRadius } from '../../theme/colors';

export interface FiltroAtivo {
  mes?: number;
  ano?: number;
  tipo?: 'DESPESA' | 'RECEITA';
  status?: string;
  categoriasIds?: number[];
  busca?: string;
}

interface Props {
  visivel: boolean;
  filtro: FiltroAtivo;
  categorias: CategoriaResponse[];
  onAplicar: (filtro: FiltroAtivo) => void;
  onFechar: () => void;
}

const MESES = ['Jan','Fev','Mar','Abr','Mai','Jun','Jul','Ago','Set','Out','Nov','Dez'];
const STATUS_OPCOES = ['EFETIVADA','PENDENTE','VENCIDA','CANCELADA'];

export default function FiltroTransacaoModal({ visivel, filtro, categorias, onAplicar, onFechar }: Props) {
  const agora = new Date();
  const [mes, setMes] = useState<number | undefined>(filtro.mes);
  const [ano, setAno] = useState<number>(filtro.ano ?? agora.getFullYear());
  const [tipo, setTipo] = useState<'DESPESA' | 'RECEITA' | undefined>(filtro.tipo);
  const [status, setStatus] = useState<string | undefined>(filtro.status);
  const [categoriasSelecionadas, setCategoriasSelecionadas] = useState<number[]>(filtro.categoriasIds ?? []);

  function toggleCategoria(id: number) {
    setCategoriasSelecionadas((prev) =>
      prev.includes(id) ? prev.filter((c) => c !== id) : [...prev, id]
    );
  }

  function handleAplicar() {
    onAplicar({ mes, ano, tipo, status, categoriasIds: categoriasSelecionadas.length > 0 ? categoriasSelecionadas : undefined });
    onFechar();
  }

  function handleLimpar() {
    setMes(undefined);
    setAno(agora.getFullYear());
    setTipo(undefined);
    setStatus(undefined);
    setCategoriasSelecionadas([]);
    onAplicar({});
    onFechar();
  }

  const categoriasFiltradas = tipo ? categorias.filter((c) => c.tipo === tipo) : categorias;

  return (
    <Modal visible={visivel} animationType="slide" transparent onRequestClose={onFechar}>
      <View style={styles.overlay}>
        <View style={styles.container}>
          <View style={styles.header}>
            <Text style={styles.titulo}>Filtros</Text>
            <TouchableOpacity onPress={onFechar}>
              <Ionicons name="close" size={24} color={Colors.textPrimary} />
            </TouchableOpacity>
          </View>

          <ScrollView showsVerticalScrollIndicator={false}>
            {/* Período */}
            <Text style={styles.secao}>Período</Text>
            <View style={styles.anoContainer}>
              <TouchableOpacity onPress={() => setAno((a) => a - 1)} style={styles.anoBtn}>
                <Ionicons name="chevron-back" size={20} color={Colors.primary} />
              </TouchableOpacity>
              <Text style={styles.anoTexto}>{ano}</Text>
              <TouchableOpacity onPress={() => setAno((a) => a + 1)} style={styles.anoBtn} disabled={ano >= agora.getFullYear()}>
                <Ionicons name="chevron-forward" size={20} color={ano < agora.getFullYear() ? Colors.primary : Colors.disabled} />
              </TouchableOpacity>
            </View>
            <View style={styles.mesesGrid}>
              {MESES.map((m, i) => (
                <TouchableOpacity
                  key={i}
                  style={[styles.mesItem, mes === i + 1 && styles.mesItemAtivo]}
                  onPress={() => setMes(mes === i + 1 ? undefined : i + 1)}
                >
                  <Text style={[styles.mesTexto, mes === i + 1 && styles.mesTextoAtivo]}>{m}</Text>
                </TouchableOpacity>
              ))}
            </View>

            {/* Tipo */}
            <Text style={styles.secao}>Tipo</Text>
            <View style={styles.row}>
              {(['DESPESA', 'RECEITA'] as const).map((t) => (
                <TouchableOpacity
                  key={t}
                  style={[styles.chip, tipo === t && styles.chipAtivo]}
                  onPress={() => setTipo(tipo === t ? undefined : t)}
                >
                  <Text style={[styles.chipTexto, tipo === t && styles.chipTextoAtivo]}>{t}</Text>
                </TouchableOpacity>
              ))}
            </View>

            {/* Status */}
            <Text style={styles.secao}>Status</Text>
            <View style={styles.row}>
              {STATUS_OPCOES.map((s) => (
                <TouchableOpacity
                  key={s}
                  style={[styles.chip, status === s && styles.chipAtivo]}
                  onPress={() => setStatus(status === s ? undefined : s)}
                >
                  <Text style={[styles.chipTexto, status === s && styles.chipTextoAtivo]}>{s}</Text>
                </TouchableOpacity>
              ))}
            </View>

            {/* Categorias */}
            <Text style={styles.secao}>Categorias</Text>
            <View style={styles.row}>
              {categoriasFiltradas.map((cat) => (
                <TouchableOpacity
                  key={cat.id}
                  style={[
                    styles.chip,
                    categoriasSelecionadas.includes(cat.id) && { backgroundColor: cat.cor, borderColor: cat.cor },
                  ]}
                  onPress={() => toggleCategoria(cat.id)}
                >
                  <Text style={[
                    styles.chipTexto,
                    categoriasSelecionadas.includes(cat.id) && { color: Colors.textWhite },
                  ]}>
                    {cat.nome}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>
          </ScrollView>

          {/* Botões */}
          <View style={styles.botoes}>
            <TouchableOpacity style={styles.botaoLimpar} onPress={handleLimpar}>
              <Text style={styles.botaoLimparTexto}>Limpar</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.botaoAplicar} onPress={handleAplicar}>
              <Text style={styles.botaoAplicarTexto}>Aplicar filtros</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: Colors.overlay,
    justifyContent: 'flex-end',
  },
  container: {
    backgroundColor: Colors.surface,
    borderTopLeftRadius: BorderRadius.xl,
    borderTopRightRadius: BorderRadius.xl,
    padding: Spacing.lg,
    maxHeight: '85%',
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: Spacing.lg,
  },
  titulo: {
    fontSize: FontSize.xl,
    fontWeight: '700',
    color: Colors.textPrimary,
  },
  secao: {
    fontSize: FontSize.sm,
    fontWeight: '600',
    color: Colors.textSecondary,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
    marginTop: Spacing.md,
    marginBottom: Spacing.sm,
  },
  anoContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.sm,
  },
  anoBtn: {
    padding: Spacing.sm,
  },
  anoTexto: {
    fontSize: FontSize.xl,
    fontWeight: '700',
    color: Colors.textPrimary,
    marginHorizontal: Spacing.lg,
  },
  mesesGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  mesItem: {
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: Colors.border,
    backgroundColor: Colors.surfaceVariant,
    minWidth: 56,
    alignItems: 'center',
  },
  mesItemAtivo: {
    borderColor: Colors.primary,
    backgroundColor: Colors.primaryLight,
  },
  mesTexto: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    fontWeight: '500',
  },
  mesTextoAtivo: {
    color: Colors.primary,
    fontWeight: '700',
  },
  row: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  chip: {
    paddingHorizontal: 12,
    paddingVertical: 7,
    borderRadius: BorderRadius.full,
    borderWidth: 1.5,
    borderColor: Colors.border,
    backgroundColor: Colors.surfaceVariant,
  },
  chipAtivo: {
    borderColor: Colors.primary,
    backgroundColor: Colors.primaryLight,
  },
  chipTexto: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    fontWeight: '500',
  },
  chipTextoAtivo: {
    color: Colors.primary,
    fontWeight: '700',
  },
  botoes: {
    flexDirection: 'row',
    gap: 12,
    marginTop: Spacing.lg,
  },
  botaoLimpar: {
    flex: 1,
    paddingVertical: 14,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: Colors.border,
    alignItems: 'center',
  },
  botaoLimparTexto: {
    fontSize: FontSize.md,
    color: Colors.textSecondary,
    fontWeight: '600',
  },
  botaoAplicar: {
    flex: 2,
    paddingVertical: 14,
    borderRadius: BorderRadius.sm,
    backgroundColor: Colors.primary,
    alignItems: 'center',
  },
  botaoAplicarTexto: {
    fontSize: FontSize.md,
    color: Colors.textWhite,
    fontWeight: '700',
  },
});
