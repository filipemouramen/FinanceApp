import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  Alert,
  Modal,
  TextInput,
  ScrollView,
  RefreshControl,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import api from '../../api/client';
import { CategoriaResponse, Resultado } from '../../types';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { useTheme } from '../../theme/useTheme';

const ICONES_DISPONIVEIS = [
  'fast-food-outline', 'car-outline', 'home-outline', 'medkit-outline',
  'school-outline', 'shirt-outline', 'game-controller-outline', 'fitness-outline',
  'airplane-outline', 'cafe-outline', 'heart-outline', 'musical-notes-outline',
  'cash-outline', 'card-outline', 'briefcase-outline', 'gift-outline',
  'paw-outline', 'book-outline', 'build-outline', 'phone-portrait-outline',
  'wallet-outline', 'bar-chart-outline', 'star-outline', 'checkmark-circle-outline',
];

const CORES_DISPONIVEIS = [
  '#6C63FF', '#FF4757', '#2ECC71', '#F39C12', '#3498DB', '#E91E63',
  '#9C27B0', '#00BCD4', '#FF5722', '#607D8B', '#795548', '#4CAF50',
];

export default function CategoriasScreen() {
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const [categorias, setCategorias] = useState<CategoriaResponse[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [tipoFiltro, setTipoFiltro] = useState<'TODAS' | 'DESPESA' | 'RECEITA'>('TODAS');
  const [modalVisivel, setModalVisivel] = useState(false);
  const [editando, setEditando] = useState<CategoriaResponse | null>(null);
  const [salvando, setSalvando] = useState(false);
  const styles = getStyles(colors);

  // Form state
  const [nome, setNome] = useState('');
  const [icone, setIcone] = useState('star-outline');
  const [cor, setCor] = useState('#6C63FF');
  const [tipo, setTipo] = useState<'DESPESA' | 'RECEITA'>('DESPESA');

  useFocusEffect(
    useCallback(() => {
      carregarCategorias();
    }, [])
  );

  async function carregarCategorias() {
    try {
      setCarregando(true);
      const res = await api.get('/categorias');
      if (res.data.sucesso) setCategorias(res.data.dados);
    } catch {
      console.log('Erro ao carregar categorias.');
    } finally {
      setCarregando(false);
    }
  }

  function categoriasFiltradas(): CategoriaResponse[] {
    if (tipoFiltro === 'TODAS') return categorias;
    return categorias.filter((c) => c.tipo === tipoFiltro);
  }

  function abrirCriar() {
    setEditando(null);
    setNome('');
    setIcone('star-outline');
    setCor('#6C63FF');
    setTipo('DESPESA');
    setModalVisivel(true);
  }

  function abrirEditar(cat: CategoriaResponse) {
    setEditando(cat);
    setNome(cat.nome);
    setIcone(cat.icone);
    setCor(cat.cor);
    setTipo(cat.tipo as 'DESPESA' | 'RECEITA');
    setModalVisivel(true);
  }

  async function handleSalvar() {
    if (!nome.trim()) { Alert.alert('Atenção', 'Informe o nome da categoria.'); return; }

    setSalvando(true);
    try {
      let resultado: Resultado<CategoriaResponse>;
      if (editando) {
        const res = await api.put(`/categorias/${editando.id}`, { nome: nome.trim(), icone, cor });
        resultado = res.data;
      } else {
        const res = await api.post('/categorias', { nome: nome.trim(), icone, cor, tipo });
        resultado = res.data;
      }

      if (resultado.sucesso) {
        setModalVisivel(false);
        carregarCategorias();
      } else {
        Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao salvar categoria.');
      }
    } catch (e: any) {
      Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Não foi possível salvar a categoria.');
    } finally {
      setSalvando(false);
    }
  }

  async function handleExcluir(cat: CategoriaResponse) {
    Alert.alert(
      'Excluir categoria',
      `Deseja excluir "${cat.nome}"?${cat.totalTransacoes > 0 ? `\n\nEsta categoria possui ${cat.totalTransacoes} transação(ões) e será apenas desativada.` : ''}`,
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Excluir',
          style: 'destructive',
          onPress: async () => {
            try {
              const res = await api.delete(`/categorias/${cat.id}`);
              const resultado: Resultado<boolean> = res.data;
              if (resultado.sucesso) {
                carregarCategorias();
              } else {
                Alert.alert('Erro', resultado.erros?.join('\n') || 'Não foi possível excluir.');
              }
            } catch (e: any) {
              Alert.alert('Erro', e?.response?.data?.erros?.[0] || 'Erro ao excluir categoria.');
            }
          },
        },
      ]
    );
  }

  function renderCategoria({ item }: { item: CategoriaResponse }) {
    return (
      <View style={styles.categoriaItem}>
        <View style={[styles.categoriaIcone, { backgroundColor: item.cor + '20' }]}>
          <Ionicons name={item.icone as any} size={22} color={item.cor} />
        </View>
        <View style={styles.categoriaInfo}>
          <Text style={styles.categoriaNome}>{item.nome}</Text>
          <View style={styles.categoriaMeta}>
            <View style={[styles.tipoBadge, { backgroundColor: item.tipo === 'DESPESA' ? colors.danger + '20' : colors.success + '20' }]}>
              <Text style={[styles.tipoTexto, { color: item.tipo === 'DESPESA' ? colors.danger : colors.success }]}>
                {item.tipo}
              </Text>
            </View>
            {item.totalTransacoes > 0 && (
              <Text style={styles.totalTx}>{item.totalTransacoes} transações</Text>
            )}
            {item.padrao && (
              <Text style={styles.padraoLabel}>PADRÃO</Text>
            )}
          </View>
        </View>
        {!item.padrao && (
          <View style={styles.acoes}>
            <TouchableOpacity style={styles.acao} onPress={() => abrirEditar(item)}>
              <Ionicons name="pencil-outline" size={18} color={colors.primary} />
            </TouchableOpacity>
            <TouchableOpacity style={styles.acao} onPress={() => handleExcluir(item)}>
              <Ionicons name="trash-outline" size={18} color={colors.danger} />
            </TouchableOpacity>
          </View>
        )}
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={24} color={colors.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitulo}>Categorias</Text>
        <TouchableOpacity style={styles.addBotao} onPress={abrirCriar}>
          <Ionicons name="add" size={22} color="#fff" />
        </TouchableOpacity>
      </View>

      {/* Filtro tipo */}
      <View style={styles.filtros}>
        {(['TODAS', 'DESPESA', 'RECEITA'] as const).map((f) => (
          <TouchableOpacity
            key={f}
            style={[styles.filtroBotao, tipoFiltro === f && styles.filtroBotaoAtivo]}
            onPress={() => setTipoFiltro(f)}
          >
            <Text style={[styles.filtroTexto, tipoFiltro === f && styles.filtroTextoAtivo]}>
              {f === 'TODAS' ? 'Todas' : f === 'DESPESA' ? 'Despesa' : 'Receita'}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      <FlatList
        data={categoriasFiltradas()}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderCategoria}
        refreshControl={
          <RefreshControl refreshing={carregando} onRefresh={carregarCategorias} colors={[colors.primary]} />
        }
        contentContainerStyle={styles.lista}
        ListEmptyComponent={
          !carregando ? (
            <View style={styles.vazio}>
              <Ionicons name="pricetag-outline" size={56} color={colors.textMuted} />
              <Text style={styles.vazioTexto}>Nenhuma categoria encontrada</Text>
            </View>
          ) : null
        }
      />

      {/* Modal criar/editar */}
      <Modal visible={modalVisivel} animationType="slide" transparent>
        <View style={styles.modalOverlay}>
          <View style={styles.modalContainer}>
            <View style={styles.modalHeader}>
              <Text style={styles.modalTitulo}>
                {editando ? 'Editar categoria' : 'Nova categoria'}
              </Text>
              <TouchableOpacity onPress={() => setModalVisivel(false)}>
                <Ionicons name="close" size={24} color={colors.textPrimary} />
              </TouchableOpacity>
            </View>

            {/* Tipo (só pra criar) */}
            {!editando && (
              <>
                <Text style={styles.label}>Tipo</Text>
                <View style={styles.tipoToggle}>
                  <TouchableOpacity
                    style={[styles.tipoOpcao, tipo === 'DESPESA' && styles.tipoOpcaoAtivoDespesa]}
                    onPress={() => setTipo('DESPESA')}
                  >
                    <Text style={[styles.tipoOpcaoTexto, tipo === 'DESPESA' && styles.tipoOpcaoTextoAtivo]}>Despesa</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    style={[styles.tipoOpcao, tipo === 'RECEITA' && styles.tipoOpcaoAtivoReceita]}
                    onPress={() => setTipo('RECEITA')}
                  >
                    <Text style={[styles.tipoOpcaoTexto, tipo === 'RECEITA' && styles.tipoOpcaoTextoAtivo]}>Receita</Text>
                  </TouchableOpacity>
                </View>
              </>
            )}

            {/* Nome */}
            <Text style={styles.label}>Nome</Text>
            <TextInput
              style={styles.input}
              placeholder="Ex: Alimentação, Salário..."
              placeholderTextColor={colors.textMuted}
              value={nome}
              onChangeText={setNome}
            />

            {/* Ícone */}
            <Text style={styles.label}>Ícone</Text>
            <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.iconeScroll}>
              {ICONES_DISPONIVEIS.map((ic) => (
                <TouchableOpacity
                  key={ic}
                  style={[styles.iconeOpcao, icone === ic && { borderColor: cor, backgroundColor: cor + '20' }]}
                  onPress={() => setIcone(ic)}
                >
                  <Ionicons name={ic as any} size={22} color={icone === ic ? cor : colors.textMuted} />
                </TouchableOpacity>
              ))}
            </ScrollView>

            {/* Cor */}
            <Text style={styles.label}>Cor</Text>
            <View style={styles.coresCointainer}>
              {CORES_DISPONIVEIS.map((c) => (
                <TouchableOpacity
                  key={c}
                  style={[styles.corChip, { backgroundColor: c }, cor === c && styles.corChipSelecionado]}
                  onPress={() => setCor(c)}
                />
              ))}
            </View>

            {/* Preview */}
            <View style={[styles.preview, { backgroundColor: cor + '15' }]}>
              <Ionicons name={icone as any} size={28} color={cor} />
              <Text style={[styles.previewTexto, { color: cor }]}>{nome || 'Nome da categoria'}</Text>
            </View>

            <TouchableOpacity
              style={[styles.botaoSalvar, salvando && styles.botaoDisabled]}
              onPress={handleSalvar}
              disabled={salvando}
            >
              <Text style={styles.botaoSalvarTexto}>{salvando ? 'Salvando...' : editando ? 'Salvar alterações' : 'Criar categoria'}</Text>
            </TouchableOpacity>
          </View>
        </View>
      </Modal>
    </View>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  header: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    paddingHorizontal: Spacing.lg, paddingTop: 60, paddingBottom: Spacing.md,
    backgroundColor: colors.surface, borderBottomWidth: 1, borderBottomColor: colors.borderLight,
  },
  headerTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  addBotao: {
    backgroundColor: colors.primary, width: 38, height: 38, borderRadius: 19,
    justifyContent: 'center', alignItems: 'center',
  },
  filtros: {
    flexDirection: 'row', padding: Spacing.md, gap: Spacing.sm,
    backgroundColor: colors.surface, borderBottomWidth: 1, borderBottomColor: colors.borderLight,
  },
  filtroBotao: {
    paddingHorizontal: Spacing.md, paddingVertical: 6, borderRadius: BorderRadius.full,
    borderWidth: 1.5, borderColor: colors.border, backgroundColor: colors.surface,
  },
  filtroBotaoAtivo: { borderColor: colors.primary, backgroundColor: colors.primary },
  filtroTexto: { fontSize: FontSize.sm, color: colors.textSecondary, fontWeight: '600' },
  filtroTextoAtivo: { color: colors.textWhite },
  lista: { padding: Spacing.md, paddingBottom: 100 },
  categoriaItem: {
    flexDirection: 'row', alignItems: 'center', gap: 12,
    backgroundColor: colors.surface, borderRadius: BorderRadius.lg,
    padding: Spacing.md, marginBottom: Spacing.sm,
    shadowColor: '#000', shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.04, shadowRadius: 4, elevation: 2,
  },
  categoriaIcone: { width: 44, height: 44, borderRadius: 22, justifyContent: 'center', alignItems: 'center' },
  categoriaInfo: { flex: 1 },
  categoriaNome: { fontSize: FontSize.md, fontWeight: '600', color: colors.textPrimary },
  categoriaMeta: { flexDirection: 'row', alignItems: 'center', gap: 6, marginTop: 3 },
  tipoBadge: { paddingHorizontal: 6, paddingVertical: 2, borderRadius: 4 },
  tipoTexto: { fontSize: 10, fontWeight: '700' },
  totalTx: { fontSize: FontSize.xs, color: colors.textMuted },
  padraoLabel: { fontSize: 9, fontWeight: '700', color: colors.textMuted, letterSpacing: 0.5 },
  acoes: { flexDirection: 'row', gap: 4 },
  acao: { padding: 6 },
  vazio: { alignItems: 'center', marginTop: 100 },
  vazioTexto: { fontSize: FontSize.lg, color: colors.textSecondary, marginTop: Spacing.md, fontWeight: '600' },
  // Modal
  modalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' },
  modalContainer: {
    backgroundColor: colors.surface, borderTopLeftRadius: 20, borderTopRightRadius: 20,
    padding: Spacing.xl, paddingBottom: 40, maxHeight: '90%',
  },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: Spacing.lg },
  modalTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  label: { fontSize: FontSize.sm, fontWeight: '600', color: colors.textSecondary, marginBottom: Spacing.xs, marginTop: Spacing.md, textTransform: 'uppercase', letterSpacing: 0.5 },
  input: {
    backgroundColor: colors.surfaceVariant, borderRadius: BorderRadius.sm, borderWidth: 1.5,
    borderColor: colors.border, padding: Spacing.md, fontSize: FontSize.md, color: colors.textPrimary,
  },
  tipoToggle: { flexDirection: 'row', gap: Spacing.sm },
  tipoOpcao: {
    flex: 1, padding: Spacing.sm, borderRadius: BorderRadius.sm, borderWidth: 1.5,
    borderColor: colors.border, alignItems: 'center',
  },
  tipoOpcaoAtivoDespesa: { borderColor: colors.danger, backgroundColor: colors.danger },
  tipoOpcaoAtivoReceita: { borderColor: colors.success, backgroundColor: colors.success },
  tipoOpcaoTexto: { fontWeight: '600', color: colors.textSecondary, fontSize: FontSize.md },
  tipoOpcaoTextoAtivo: { color: colors.textWhite },
  iconeScroll: { marginBottom: 4 },
  iconeOpcao: {
    width: 44, height: 44, borderRadius: 22, justifyContent: 'center', alignItems: 'center',
    borderWidth: 2, borderColor: colors.border, marginRight: 8, backgroundColor: colors.surface,
  },
  coresCointainer: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, marginBottom: 4 },
  corChip: { width: 28, height: 28, borderRadius: 14 },
  corChipSelecionado: { borderWidth: 3, borderColor: colors.textPrimary },
  preview: {
    flexDirection: 'row', alignItems: 'center', gap: 12, borderRadius: BorderRadius.md,
    padding: Spacing.md, marginTop: Spacing.md,
  },
  previewTexto: { fontSize: FontSize.lg, fontWeight: '700' },
  botaoSalvar: {
    backgroundColor: colors.primary, borderRadius: BorderRadius.sm, padding: Spacing.md,
    alignItems: 'center', marginTop: Spacing.lg,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoSalvarTexto: { color: '#fff', fontSize: FontSize.lg, fontWeight: '700' },
});
