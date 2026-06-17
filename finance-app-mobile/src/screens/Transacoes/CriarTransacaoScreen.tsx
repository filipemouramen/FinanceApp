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
import { CategoriaResponse, CartaoCreditoResponse, ContaResponse, Resultado, TransacaoResponse, TipoTransacao } from '../../types';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { useTheme } from '../../theme/useTheme';
import { formatarMoeda } from '../../utils/formatters';

function centavosParaDisplay(centavos: number): string {
  if (centavos === 0) return '';
  const reais = Math.floor(centavos / 100);
  const cents = centavos % 100;
  const reaisStr = reais.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  return `${reaisStr},${cents.toString().padStart(2, '0')}`;
}

const FORMAS_PAGAMENTO = [
  { id: 1, nome: 'Dinheiro', icone: 'cash-outline' },
  { id: 2, nome: 'Crédito', icone: 'card-outline' },
  { id: 3, nome: 'Débito', icone: 'card-outline' },
  { id: 4, nome: 'PIX', icone: 'qr-code-outline' },
  { id: 5, nome: 'Boleto', icone: 'document-text-outline' },
  { id: 6, nome: 'Transfer.', icone: 'swap-horizontal-outline' },
];

export default function CriarTransacaoScreen({ navigation, route }: any) {
  const { colors } = useTheme();
  const transacaoParaEditar: TransacaoResponse | undefined = route?.params?.transacaoParaEditar;
  const modoEdicao = !!transacaoParaEditar;

  const [categorias, setCategorias] = useState<CategoriaResponse[]>([]);
  const [contas, setContas] = useState<ContaResponse[]>([]);
  const [cartoes, setCartoes] = useState<CartaoCreditoResponse[]>([]);
  const [carregandoDados, setCarregandoDados] = useState(true);

  const [tipo, setTipo] = useState<TipoTransacao>(transacaoParaEditar?.tipo ?? 'DESPESA');
  const initialCentavos = transacaoParaEditar ? Math.round(transacaoParaEditar.valor * 100) : 0;
  const [valorCentavos, setValorCentavos] = useState(initialCentavos);
  const [valorDisplay, setValorDisplay] = useState(centavosParaDisplay(initialCentavos));
  const [descricao, setDescricao] = useState(transacaoParaEditar?.descricao ?? '');
  const [categoriaId, setCategoriaId] = useState<number | null>(transacaoParaEditar?.categoriaId ?? null);
  const [contaId, setContaId] = useState<number | null>(transacaoParaEditar?.contaId ?? null);
  const [cartaoId, setCartaoId] = useState<number | null>(null);
  const [formaPagamentoId, setFormaPagamentoId] = useState<number>(transacaoParaEditar?.formaPagamentoId ?? 4);
  const [totalParcelas, setTotalParcelas] = useState('');
  const [agendar, setAgendar] = useState(false);
  const [salvando, setSalvando] = useState(false);

  const styles = getStyles(colors);

  function handleValorChange(text: string) {
    const digits = text.replace(/\D/g, '');
    const centavos = parseInt(digits || '0', 10);
    setValorCentavos(centavos);
    setValorDisplay(centavosParaDisplay(centavos));
  }

  useEffect(() => {
    carregarDados();
  }, []);

  useEffect(() => {
    if (!modoEdicao) setCategoriaId(null);
  }, [tipo]);

  async function carregarDados() {
    try {
      const [catRes, contRes, cartRes] = await Promise.all([
        api.get('/categorias'),
        api.get('/contas'),
        api.get('/cartoes'),
      ]);

      if (catRes.data.sucesso) setCategorias(catRes.data.dados);
      if (contRes.data.sucesso) {
        setContas(contRes.data.dados);
        if (!modoEdicao && !contaId) {
          const principal = contRes.data.dados.find((c: ContaResponse) => c.principal);
          if (principal) setContaId(principal.id);
        }
      }
      if (cartRes.data.sucesso) setCartoes(cartRes.data.dados);
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
    if (modoEdicao && transacaoParaEditar?.parcelamentoId) {
      Alert.alert(
        'Transação parcelada',
        'Esta transação faz parte de um parcelamento e não pode ser editada individualmente.',
        [
          { text: 'Fechar', style: 'cancel' },
          {
            text: 'Cancelar parcelamento',
            style: 'destructive',
            onPress: async () => {
              try {
                await api.delete(`/transacoes/parcelamento/${transacaoParaEditar.parcelamentoId}`);
                Alert.alert('Parcelamento cancelado', '', [{ text: 'OK', onPress: () => navigation.goBack() }]);
              } catch {
                Alert.alert('Erro', 'Não foi possível cancelar o parcelamento.');
              }
            },
          },
        ]
      );
      return;
    }

    if (valorCentavos <= 0) {
      Alert.alert('Atenção', 'Informe um valor maior que zero.');
      return;
    }

    if (!categoriaId) {
      Alert.alert('Atenção', 'Selecione uma categoria.');
      return;
    }

    setSalvando(true);
    try {
      let resultado: Resultado<any>;

      if (modoEdicao) {
        const body: any = {
          categoriaId,
          contaId,
          formaPagamentoId,
          valor: valorCentavos / 100,
          descricao: descricao.trim() || null,
        };
        const response = await api.put(`/transacoes/${transacaoParaEditar!.id}`, body);
        resultado = response.data;
      } else {
        const parcelas = parseInt(totalParcelas);
        const body: any = {
          categoriaId,
          contaId,
          formaPagamentoId,
          valor: valorCentavos / 100,
          tipo,
          descricao: descricao.trim() || null,
          agendar,
        };
        if (parcelas >= 2) body.totalParcelas = parcelas;
        if (cartaoId) body.cartaoId = cartaoId;
        const response = await api.post('/transacoes', body);
        resultado = response.data;
      }

      if (resultado.sucesso) {
        const msg = modoEdicao ? 'Transação atualizada!' : 'Transação registrada!';
        Alert.alert('Sucesso', resultado.mensagem || msg, [
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
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="close" size={28} color={colors.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitulo}>{modoEdicao ? 'Editar Transação' : 'Nova Transação'}</Text>
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
              color={tipo === 'DESPESA' ? colors.textWhite : colors.danger}
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
              color={tipo === 'RECEITA' ? colors.textWhite : colors.success}
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
            <Text style={[styles.valorPrefixo, { color: tipo === 'DESPESA' ? colors.danger : colors.success }]}>
              R$
            </Text>
            <TextInput
              style={[styles.valorInput, { color: tipo === 'DESPESA' ? colors.danger : colors.success }]}
              placeholder="0,00"
              placeholderTextColor={colors.textMuted}
              keyboardType="number-pad"
              value={valorDisplay}
              onChangeText={handleValorChange}
            />
          </View>
        </View>

        {/* Descrição */}
        <Text style={styles.secaoTitulo}>Descrição</Text>
        <TextInput
          style={styles.input}
          placeholder="Ex: Almoço, Salário, Netflix..."
          placeholderTextColor={colors.textMuted}
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
              key={conta.id.toString()}
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
                    color={formaPagamentoId === fp.id ? colors.primary : colors.textMuted}
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

        {/* Cartão de Crédito (só quando forma = Crédito) */}
        {tipo === 'DESPESA' && formaPagamentoId === 2 && cartoes.length > 0 && (
          <>
            <Text style={styles.secaoTitulo}>Cartão de crédito</Text>
            <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.categoriasScroll}>
              {cartoes.map((cartao) => (
                <TouchableOpacity
                  key={cartao.id}
                  style={[
                    styles.cartaoItem,
                    cartaoId === cartao.id && { borderColor: cartao.cor, backgroundColor: cartao.cor + '15' },
                  ]}
                  onPress={() => setCartaoId(cartaoId === cartao.id ? null : cartao.id)}
                >
                  <View style={[styles.cartaoCor, { backgroundColor: cartao.cor }]} />
                  <View>
                    <Text style={[styles.cartaoNome, cartaoId === cartao.id && { color: cartao.cor, fontWeight: '700' }]} numberOfLines={1}>
                      {cartao.nome}
                    </Text>
                    <Text style={styles.cartaoLimite}>{formatarMoeda(cartao.limiteDisponivel)} disponível</Text>
                  </View>
                </TouchableOpacity>
              ))}
            </ScrollView>
          </>
        )}

        {/* Parcelamento (só pra despesa) */}
        {tipo === 'DESPESA' && (
          <>
            <Text style={styles.secaoTitulo}>Parcelas (opcional)</Text>
            <TextInput
              style={styles.input}
              placeholder="Ex: 3, 6, 12 (deixe vazio pra compras à vista)"
              placeholderTextColor={colors.textMuted}
              keyboardType="number-pad"
              value={totalParcelas}
              onChangeText={setTotalParcelas}
            />
            {parseInt(totalParcelas) >= 2 && valor && (
              <Text style={styles.parcelaInfo}>
                {totalParcelas}x de {formatarMoeda((valorCentavos / 100) / parseInt(totalParcelas))}
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
            color={agendar ? colors.primary : colors.textMuted}
          />
          <Text style={styles.checkboxTexto}>Agendar (não debitar agora)</Text>
        </TouchableOpacity>

        {/* Resumo */}
        {valorCentavos > 0 && categoriaId && (
          <View style={styles.resumoContainer}>
            <Text style={styles.resumoTitulo}>Resumo</Text>
            <View style={styles.resumoLinha}>
              <Text style={styles.resumoLabel}>Tipo</Text>
              <Text style={[styles.resumoValor, { color: tipo === 'DESPESA' ? colors.danger : colors.success }]}>
                {tipo}
              </Text>
            </View>
            <View style={styles.resumoLinha}>
              <Text style={styles.resumoLabel}>Valor</Text>
              <Text style={styles.resumoValor}>{formatarMoeda(valorCentavos / 100)}</Text>
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
                {contas.find((c) => c.id === contaId)?.nome ?? 'Nenhuma'}
              </Text>
            </View>
            <View style={styles.resumoLinha}>
              <Text style={styles.resumoLabel}>Status</Text>
              <Text style={styles.resumoValor}>{agendar ? 'Pendente' : 'Efetivada'}</Text>
            </View>
          </View>
        )}

        <View style={{ height: 100 }} />
      </ScrollView>

      {/* Botão Salvar (fixo embaixo) */}
      <View style={styles.botaoContainer}>
        <TouchableOpacity
          style={[styles.botaoSalvar, salvando && styles.botaoDisabled]}
          onPress={handleSalvar}
          disabled={salvando}
        >
          <Ionicons name="checkmark-circle" size={22} color={colors.textWhite} />
          <Text style={styles.botaoSalvarTexto}>
            {salvando ? 'Salvando...' : modoEdicao ? 'Salvar Alterações' : 'Registrar transação'}
          </Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  carregando: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: colors.background,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: Spacing.lg,
    paddingTop: 60,
    paddingBottom: Spacing.md,
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
  },
  headerTitulo: { fontSize: FontSize.xl, fontWeight: '700', color: colors.textPrimary },
  scroll: { flex: 1, padding: Spacing.lg },
  toggleContainer: {
    flexDirection: 'row',
    backgroundColor: colors.surfaceVariant,
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
  toggleDespesaAtivo: { backgroundColor: colors.danger },
  toggleReceitaAtivo: { backgroundColor: colors.success },
  toggleTexto: { fontSize: FontSize.md, fontWeight: '600', color: colors.textSecondary },
  toggleTextoAtivo: { color: colors.textWhite },
  valorContainer: { alignItems: 'center', marginBottom: Spacing.lg },
  valorLabel: {
    fontSize: FontSize.sm,
    color: colors.textSecondary,
    marginBottom: Spacing.xs,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
    fontWeight: '600',
  },
  valorInputContainer: { flexDirection: 'row', alignItems: 'center' },
  valorPrefixo: { fontSize: FontSize.title, fontWeight: '700', marginRight: 4 },
  valorInput: { fontSize: FontSize.hero, fontWeight: '800', minWidth: 100, textAlign: 'center' },
  secaoTitulo: {
    fontSize: FontSize.sm,
    fontWeight: '600',
    color: colors.textSecondary,
    marginBottom: Spacing.sm,
    marginTop: Spacing.lg,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  input: {
    backgroundColor: colors.surface,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md,
    fontSize: FontSize.lg,
    color: colors.textPrimary,
    borderWidth: 1,
    borderColor: colors.border,
  },
  categoriasScroll: { marginBottom: Spacing.sm },
  categoriaItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: colors.border,
    backgroundColor: colors.surface,
    marginRight: 8,
    gap: 8,
  },
  categoriaCor: { width: 10, height: 10, borderRadius: 5 },
  categoriaTexto: { fontSize: FontSize.md, color: colors.textPrimary, fontWeight: '500' },
  cartaoItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: colors.border,
    backgroundColor: colors.surface,
    marginRight: 8,
    gap: 10,
    minWidth: 140,
  },
  cartaoCor: { width: 12, height: 12, borderRadius: 6 },
  cartaoNome: { fontSize: FontSize.md, color: colors.textPrimary, fontWeight: '500' },
  cartaoLimite: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 1 },
  formasContainer: { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  formaItem: {
    alignItems: 'center',
    paddingHorizontal: 12,
    paddingVertical: 10,
    borderRadius: BorderRadius.sm,
    borderWidth: 1.5,
    borderColor: colors.border,
    backgroundColor: colors.surface,
    minWidth: 80,
    gap: 4,
  },
  formaItemAtivo: { borderColor: colors.primary, backgroundColor: colors.primaryLight },
  formaTexto: { fontSize: FontSize.xs, color: colors.textSecondary, fontWeight: '500' },
  formaTextoAtivo: { color: colors.primary, fontWeight: '600' },
  parcelaInfo: { fontSize: FontSize.md, color: colors.primary, fontWeight: '600', marginTop: Spacing.xs },
  checkboxContainer: { flexDirection: 'row', alignItems: 'center', gap: 10, marginTop: Spacing.lg },
  checkboxTexto: { fontSize: FontSize.md, color: colors.textPrimary },
  resumoContainer: {
    backgroundColor: colors.surface,
    borderRadius: BorderRadius.lg,
    padding: Spacing.md,
    marginTop: Spacing.lg,
    borderWidth: 1,
    borderColor: colors.border,
  },
  resumoTitulo: { fontSize: FontSize.md, fontWeight: '700', color: colors.textPrimary, marginBottom: Spacing.sm },
  resumoLinha: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 6,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
  },
  resumoLabel: { fontSize: FontSize.md, color: colors.textSecondary },
  resumoValor: { fontSize: FontSize.md, fontWeight: '600', color: colors.textPrimary },
  botaoContainer: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    padding: Spacing.lg,
    paddingBottom: Spacing.xl,
    backgroundColor: colors.background,
    borderTopWidth: 1,
    borderTopColor: colors.borderLight,
  },
  botaoSalvar: {
    backgroundColor: colors.primary,
    borderRadius: BorderRadius.sm,
    padding: Spacing.md + 2,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
  },
  botaoDisabled: { opacity: 0.6 },
  botaoSalvarTexto: { color: colors.textWhite, fontSize: FontSize.lg, fontWeight: '700' },
});
