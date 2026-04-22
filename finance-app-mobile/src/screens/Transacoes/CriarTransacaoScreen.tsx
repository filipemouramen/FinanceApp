import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TextInput,
  TouchableOpacity,
  ScrollView,
  Alert,
  ActivityIndicator,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import api from '../../api/client';
import { CategoriaResponse, ContaResponse, Resultado } from '../../types';
import { Colors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { formatarMoeda } from '../../utils/formatters';

const FORMAS_PAGAMENTO = [
  { id: 1, nome: 'Dinheiro', icone: 'cash-outline' },
  { id: 2, nome: 'Crédito', icone: 'card-outline' },
  { id: 3, nome: 'Débito', icone: 'card-outline' },
  { id: 4, nome: 'PIX', icone: 'qr-code-outline' },
  { id: 5, nome: 'Boleto', icone: 'document-text-outline' },
  { id: 6, nome: 'Transfer.', icone: 'swap-horizontal-outline' },
];

export default function CriarTransacaoScreen({ navigation }: any) {

  const [categorias, setCategorias] = useState<CategoriaResponse[]>([]);
  const [contas, setContas] = useState<ContaResponse[]>([]);
  const [carregandoDados, setCarregandoDados] = useState(true);

  const [tipo, setTipo] = useState<'DESPESA' | 'RECEITA'>('DESPESA');
  const [valor, setValor] = useState('');
  const [descricao, setDescricao] = useState('');
  const [categoriaId, setCategoriaId] = useState<number | null>(null);
  const [contaId, setContaId] = useState<string | null>(null);
  const [formaPagamentoId, setFormaPagamentoId] = useState<number>(4);
  const [totalParcelas, setTotalParcelas] = useState('');
  const [agendar, setAgendar] = useState(false);
  const [salvando, setSalvando] = useState(false);

  useEffect(() => {
    carregarDados();
  }, []);

  useEffect(() => {
    setCategoriaId(null);
  }, [tipo]);

  async function carregarDados() {
    try {
      const [catRes, contRes] = await Promise.all([
        api.get('/categorias'),
        api.get('/contas'),
      ]);

      if (catRes.data.sucesso) setCategorias(catRes.data.dados);
      if (contRes.data.sucesso) {
        setContas(contRes.data.dados);
        const principal = contRes.data.dados.find((c: ContaResponse) => c.principal);
        if (principal) setContaId(principal.id);
      }
    } catch (error) {
      console.log('Erro ao carregar dados:', error);
    } finally {
      setCarregandoDados(false);
    }
  }

  function categoriasFiltradas(): CategoriaResponse[] {
    return categorias.filter((c) => c.tipo === tipo);
  }

  async function handleSalvar() {
    if (!valor || parseFloat(valor) <= 0) {
      Alert.alert('Atenção', 'Informe um valor maior que zero.');
      return;
    }

    if (!categoriaId) {
      Alert.alert('Atenção', 'Selecione uma categoria.');
      return;
    }

    setSalvando(true);
    try {
      const parcelas = parseInt(totalParcelas);

      const body: any = {
        categoriaId,
        contaId,
        formaPagamentoId,
        valor: parseFloat(valor),
        tipo,
        descricao: descricao.trim() || null,
        agendar,
      };

      if (parcelas >= 2) {
        body.totalParcelas = parcelas;
      }

      const response = await api.post('/transacoes', body);
      const resultado: Resultado<any> = response.data;

      if (resultado.sucesso) {
        Alert.alert('Sucesso', resultado.mensagem || 'Transação registrada!', [
          { text: 'OK', onPress: () => navigation.goBack() },
        ]);
      } else {
        Alert.alert('Erro', resultado.erros?.join('\n') || 'Erro ao salvar.');
      }
    } catch (error: any) {
      Alert.alert('Erro', error.response?.data?.erros?.[0] || 'Erro de conexão.');
    } finally {
      setSalvando(false);
    }
  }

  if (carregandoDados) {
    return (
      <View style={styles.carregando}>
        <ActivityIndicator size="large" color={Colors.primary} />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="close" size={28} color={Colors.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitulo}>Nova Transação</Text>
        <View style={{ width: 28 }} />
      </View>

      <ScrollView style={styles.scroll} showsVerticalScrollIndicator={false}>
        {/* Toggle Despesa / Receita */}
        <View style={styles.toggleContainer}>
          <TouchableOpacity
            style={[styles.toggleBotao, tipo === 'DESPESA' && styles.toggleDespesaAtivo]}
            onPress={() => setTipo('DESPESA')}
          >
            <Ionicons
              name="arrow-down-circle"
              size={20}
              color={tipo === 'DESPESA' ? Colors.textWhite : Colors.danger}
            />
            <Text style={[styles.toggleTexto, tipo === 'DESPESA' && styles.toggleTextoAtivo]}>
              Despesa
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.toggleBotao, tipo === 'RECEITA' && styles.toggleReceitaAtivo]}
            onPress={() => setTipo('RECEITA')}
          >
            <Ionicons
              name="arrow-up-circle"
              size={20}
              color={tipo === 'RECEITA' ? Colors.textWhite : Colors.success}
            />
            <Text style={[styles.toggleTexto, tipo === 'RECEITA' && styles.toggleTextoAtivo]}>
              Receita
            </Text>
          </TouchableOpacity>
        </View>

        {/* Valor */}
        <View style={styles.valorContainer}>
          <Text style={styles.valorLabel}>Valor</Text>
          <View style={styles.valorInputContainer}>
            <Text style={[styles.valorPrefixo, { color: tipo === 'DESPESA' ? Colors.danger : Colors.success }]}>
              R$
            </Text>
            <TextInput
              style={[styles.valorInput, { color: tipo === 'DESPESA' ? Colors.danger : Colors.success }]}
              placeholder="0,00"
              placeholderTextColor={Colors.textMuted}
              keyboardType="decimal-pad"
              value={valor}
              onChangeText={setValor}
            />
          </View>
        </View>

        {/* Descrição */}
        <Text style={styles.secaoTitulo}>Descrição</Text>
        <TextInput
          style={styles.input}
          placeholder="Ex: Almoço, Salário, Netflix..."
          placeholderTextColor={Colors.textMuted}
          value={descricao}
          onChangeText={setDescricao}
        />

        {/* Categoria */}
        <Text style={styles.secaoTitulo}>Categoria *</Text>
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.categoriasScroll}>
          {categoriasFiltradas().map((cat) => (
            <TouchableOpacity
              key={cat.id}
              style={[
                styles.categoriaItem,
                categoriaId === cat.id && { borderColor: cat.cor, backgroundColor: cat.cor + '15' },
              ]}
              onPress={() => setCategoriaId(cat.id)}
            >
              <View style={[styles.categoriaCor, { backgroundColor: cat.cor }]} />
              <Text
                style={[
                  styles.categoriaTexto,
                  categoriaId === cat.id && { color: cat.cor, fontWeight: '700' },
                ]}
                numberOfLines={1}
              >
                {cat.nome}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>

        {/* Conta */}
        <Text style={styles.secaoTitulo}>Conta</Text>
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.categoriasScroll}>
          {contas.map((conta) => (
            <TouchableOpacity
              key={conta.id}
              style={[
                styles.categoriaItem,
                contaId === conta.id && { borderColor: conta.cor, backgroundColor: conta.cor + '15' },
              ]}
              onPress={() => setContaId(conta.id)}
            >
              <View style={[styles.categoriaCor, { backgroundColor: conta.cor }]} />
              <Text
                style={[
                  styles.categoriaTexto,
                  contaId === conta.id && { color: conta.cor, fontWeight: '700' },
                ]}
                numberOfLines={1}
              >
                {conta.nome}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>

        {/* Forma de Pagamento (só pra despesa) */}
        {tipo === 'DESPESA' && (
          <>
            <Text style={styles.secaoTitulo}>Forma de pagamento</Text>
            <View style={styles.formasContainer}>
              {FORMAS_PAGAMENTO.map((fp) => (
                <TouchableOpacity
                  key={fp.id}
                  style={[
                    styles.formaItem,
                    formaPagamentoId === fp.id && styles.formaItemAtivo,
                  ]}
                  onPress={() => setFormaPagamentoId(fp.id)}
                >
                  <Ionicons
                    name={fp.icone as any}
                    size={20}
                    color={formaPagamentoId === fp.id ? Colors.primary : Colors.textMuted}
                  />
                  <Text
                    style={[
                      styles.formaTexto,
                      formaPagamentoId === fp.id && styles.formaTextoAtivo,
                    ]}
                  >
                    {fp.nome}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>
          </>
        )}

        {/* Parcelamento (só pra despesa) */}
        {tipo === 'DESPESA' && (
          <>
            <Text style={styles.secaoTitulo}>Parcelas (opcional)</Text>
            <TextInput
              style={styles.input}
              placeholder="Ex: 3, 6, 12 (deixe vazio pra compras à vista)"
              placeholderTextColor={Colors.textMuted}
              keyboardType="number-pad"
              value={totalParcelas}
              onChangeText={setTotalParcelas}
            />
            {parseInt(totalParcelas) >= 2 && valor && (
              <Text style={styles.parcelaInfo}>
                {totalParcelas}x de {formatarMoeda(parseFloat(valor) / parseInt(totalParcelas))}
              </Text>
            )}
          </>
        )}

        {/* Agendar */}
        <TouchableOpacity
          style={styles.checkboxContainer}
          onPress={() => setAgendar(!agendar)}
        >
          <Ionicons
            name={agendar ? 'checkbox' : 'square-outline'}
            size={24}
            color={agendar ? Colors.primary : Colors.textMuted}
          />
          <Text style={styles.checkboxTexto}>Agendar (não debitar agora)</Text>
        </TouchableOpacity>

        {/* Resumo */}
        {valor && categoriaId && (
          <View style={styles.resumoContainer}>
            <Text style={styles.resumoTitulo}>Resumo</Text>
            <View style={styles.resumoLinha}>
              <Text style={styles.resumoLabel}>Tipo</Text>
              <Text style={[styles.resumoValor, { color: tipo === 'DESPESA' ? Colors.danger : Colors.success }]}>
                {tipo}
              </Text>
            </View>
            <View style={styles.resumoLinha}>
              <Text style={styles.resumoLabel}>Valor</Text>
              <Text style={styles.resumoValor}>{formatarMoeda(parseFloat(valor) || 0)}</Text>
            </View>
            <View style={styles.resumoLinha}>
              <Text style={styles.resumoLabel}>Categoria</Text>
              <Text style={styles.resumoValor}>
                {categorias.find((c) => c.id === categoriaId)?.nome}
              </Text>
            </View>
            <View style={styles.resumoLinha}>
              <Text style={styles.resumoLabel}>Conta</Text>
              <Text style={styles.resumoValor}>
                {contas.find((c) => c.id === contaId)?.nome || 'Nenhuma'}
              </Text>
            </View>
            <View style={styles.resumoLinha}>
              <Text style={styles.resumoLabel}>Status</Text>
              <Text style={styles.resumoValor}>{agendar ? 'Pendente' : 'Efetivada'}</Text>
            </View>
          </View>
        )}

        {/* Espaço pro botão */}
        <View style={{ height: 100 }} />
      </ScrollView>

      {/* Botão Salvar (fixo embaixo) */}
      <View style={styles.botaoContainer}>
        <TouchableOpacity
          style={[styles.botaoSalvar, salvando && styles.botaoDisabled]}
          onPress={handleSalvar}
          disabled={salvando}
        >
          <Ionicons name="checkmark-circle" size={22} color={Colors.textWhite} />
          <Text style={styles.botaoSalvarTexto}>
            {salvando ? 'Salvando...' : 'Registrar transação'}
          </Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  carregando: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: Colors.background,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: Spacing.lg,
    paddingTop: 60,
    paddingBottom: Spacing.md,
    backgroundColor: Colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: Colors.borderLight,
  },
  headerTitulo: {
    fontSize: FontSize.xl,
    fontWeight: '700',
    color: Colors.textPrimary,
  },
  scroll: {
    flex: 1,
    padding: Spacing.lg,
  },
  // Toggle Despesa/Receita
  toggleContainer: {
    flexDirection: 'row',
    backgroundColor: Colors.surfaceVariant,
    borderRadius: BorderRadius.sm,
    padding: 4,
    marginBottom: Spacing.lg,
  },
  toggleBotao: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 12,
    borderRadius: BorderRadius.sm - 2,
    gap: 6,
  },
  toggleDespesaAtivo: {
    backgroundColor: Colors.danger,
  },
  toggleReceitaAtivo: {
    backgroundColor: Colors.success,
  },
  toggleTexto: {
    fontSize: FontSize.md,
    fontWeight: '600',
    color: Colors.textSecondary,
  },
  toggleTextoAtivo: {
    color: Colors.textWhite,
  },
  // Valor
  valorContainer: {
    alignItems: 'center',
    marginBottom: Spacing.lg,
  },
  valorLabel: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    marginBottom: Spacing.xs,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
    fontWeight: '600',
  },
  valorInputContainer: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  valorPrefixo: {
    fontSize: FontSize.title,
    fontWeight: '700',
    marginRight: 4,
  },
  valorInput: {
    fontSize: FontSize.hero,
    fontWeight: '800',
    minWidth: 100,
    textAlign: 'center',
  },
  // Seções
  secaoTitulo: {
    fontSize: FontSize.sm,
    fontWeight: '600',
    color: Colors.textSecondary,
    marginBottom: Spacing.sm,
    marginTop: Spacing.lg,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  input: {
    backgroundColor: Colors.surface,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    fontSize: FontSize.lg,
    color: Colors.textPrimary,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  // Categorias (horizontal)
  categoriasScroll: {
    marginBottom: Spacing.sm,
  },
  categoriaItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: Colors.border,
    backgroundColor: Colors.surface,
    marginRight: 8,
    gap: 8,
  },
  categoriaCor: {
    width: 10,
    height: 10,
    borderRadius: 5,
  },
  categoriaTexto: {
    fontSize: FontSize.md,
    color: Colors.textPrimary,
    fontWeight: '500',
  },
  // Formas de pagamento
  formasContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  formaItem: {
    alignItems: 'center',
    paddingHorizontal: 12,
    paddingVertical: 10,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: Colors.border,
    backgroundColor: Colors.surface,
    minWidth: 80,
    gap: 4,
  },
  formaItemAtivo: {
    borderColor: Colors.primary,
    backgroundColor: Colors.primaryLight,
  },
  formaTexto: {
    fontSize: FontSize.xs,
    color: Colors.textSecondary,
    fontWeight: '500',
  },
  formaTextoAtivo: {
    color: Colors.primary,
    fontWeight: '600',
  },
  // Parcelas
  parcelaInfo: {
    fontSize: FontSize.md,
    color: Colors.primary,
    fontWeight: '600',
    marginTop: Spacing.xs,
  },
  // Checkbox
  checkboxContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    marginTop: Spacing.lg,
  },
  checkboxTexto: {
    fontSize: FontSize.md,
    color: Colors.textPrimary,
  },
  // Resumo
  resumoContainer: {
    backgroundColor: Colors.surface,
    borderRadius: BorderRadius.lg,
    padding: Spacing.md,
    marginTop: Spacing.lg,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  resumoTitulo: {
    fontSize: FontSize.md,
    fontWeight: '700',
    color: Colors.textPrimary,
    marginBottom: Spacing.sm,
  },
  resumoLinha: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 6,
    borderBottomWidth: 1,
    borderBottomColor: Colors.borderLight,
  },
  resumoLabel: {
    fontSize: FontSize.md,
    color: Colors.textSecondary,
  },
  resumoValor: {
    fontSize: FontSize.md,
    fontWeight: '600',
    color: Colors.textPrimary,
  },
  // Botão fixo
  botaoContainer: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    padding: Spacing.lg,
    paddingBottom: Spacing.xl,
    backgroundColor: Colors.background,
    borderTopWidth: 1,
    borderTopColor: Colors.borderLight,
  },
  botaoSalvar: {
    backgroundColor: Colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md + 2,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
  },
  botaoDisabled: {
    opacity: 0.6,
  },
  botaoSalvarTexto: {
    color: Colors.textWhite,
    fontSize: FontSize.lg,
    fontWeight: '700',
  },
});