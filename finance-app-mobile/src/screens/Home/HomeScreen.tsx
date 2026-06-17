import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
  Dimensions,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import { usePreventScreenCapture } from 'expo-screen-capture';
import { useAuth } from '../../contexts/AuthContext';
import { useTheme } from '../../theme/useTheme';
import api from '../../api/client';
import { DashboardResponse, Resultado } from '../../types';
import { LightColors, Spacing, FontSize, BorderRadius } from '../../theme/colors';
import { formatarMoeda, saudacao } from '../../utils/formatters';

const { width } = Dimensions.get('window');
const NOMES_MESES = ['', 'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

export default function HomeScreen() {
  usePreventScreenCapture();

  const { usuario } = useAuth();
  const { colors } = useTheme();
  const navigation = useNavigation<any>();
  const [dashboard, setDashboard] = useState<DashboardResponse | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [naoLidas, setNaoLidas] = useState(0);
  const hoje = new Date();
  const [mes, setMes] = useState(hoje.getMonth() + 1);
  const [ano, setAno] = useState(hoje.getFullYear());

  const styles = getStyles(colors);

  useFocusEffect(
    useCallback(() => {
      carregarDashboard(mes, ano);
      api.get('/notificacoes/contador')
        .then((r) => { if (r.data.sucesso) setNaoLidas(r.data.dados?.count ?? 0); })
        .catch(() => {});
    }, [mes, ano])
  );

  async function carregarDashboard(m: number, a: number) {
    try {
      setCarregando(true);
      const response = await api.get(`/dashboard/${m}/${a}`);
      const resultado: Resultado<DashboardResponse> = response.data;
      if (resultado.sucesso && resultado.dados) {
        setDashboard(resultado.dados);
      }
    } catch (error) {
      console.log('Erro ao carregar dashboard:', error);
    } finally {
      setCarregando(false);
    }
  }

  function navegarMes(direcao: -1 | 1) {
    let novoMes = mes + direcao;
    let novoAno = ano;
    if (novoMes > 12) { novoMes = 1; novoAno++; }
    if (novoMes < 1) { novoMes = 12; novoAno--; }
    setMes(novoMes);
    setAno(novoAno);
  }

  const ehMesAtual = mes === hoje.getMonth() + 1 && ano === hoje.getFullYear();
  const primeiroNome = usuario?.nomeCompleto?.split(' ')[0] || 'Usuário';

  return (
    <ScrollView
      style={styles.container}
      refreshControl={
        <RefreshControl refreshing={carregando} onRefresh={carregarDashboard} colors={[colors.primary]} />
      }
      showsVerticalScrollIndicator={false}
    >
      {/* Header */}
      <View style={styles.header}>
        <View>
          <Text style={styles.saudacao}>{saudacao()},</Text>
          <Text style={styles.nome}>{primeiroNome} 👋</Text>
        </View>
        <TouchableOpacity style={styles.notifBotao} onPress={() => navigation.navigate('Notificacoes')}>
          <Ionicons name={naoLidas > 0 ? 'notifications' : 'notifications-outline'} size={24} color={naoLidas > 0 ? colors.primary : colors.textPrimary} />
          {naoLidas > 0 && (
            <View style={styles.notifBadge}>
              <Text style={styles.notifBadgeTexto}>{naoLidas > 9 ? '9+' : naoLidas}</Text>
            </View>
          )}
        </TouchableOpacity>
      </View>

      {/* Navegador de mês */}
      <View style={styles.mesNavigator}>
        <TouchableOpacity onPress={() => navegarMes(-1)} style={styles.mesBotao}>
          <Ionicons name="chevron-back" size={20} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.mesTitulo}>{NOMES_MESES[mes]} {ano}</Text>
        <TouchableOpacity
          onPress={() => navegarMes(1)}
          style={styles.mesBotao}
          disabled={ehMesAtual}
        >
          <Ionicons name="chevron-forward" size={20} color={ehMesAtual ? colors.textMuted : colors.primary} />
        </TouchableOpacity>
      </View>

      {/* Card Saldo Principal */}
      <View style={styles.saldoCard}>
        <Text style={styles.saldoLabel}>Saldo total em contas</Text>
        <Text style={styles.saldoValor}>
          {formatarMoeda(dashboard?.resumo?.saldoTotalContas || 0)}
        </Text>
        <View style={styles.saldoLinhaResumo}>
          <View style={styles.saldoItem}>
            <Ionicons name="arrow-up-circle" size={18} color="#4ADE80" />
            <Text style={styles.saldoItemLabel}>Receitas</Text>
            <Text style={styles.saldoReceita}>
              {formatarMoeda(dashboard?.resumo?.totalReceitas || 0)}
            </Text>
          </View>
          <View style={styles.saldoDivisor} />
          <View style={styles.saldoItem}>
            <Ionicons name="arrow-down-circle" size={18} color="#F87171" />
            <Text style={styles.saldoItemLabel}>Despesas</Text>
            <Text style={styles.saldoDespesa}>
              {formatarMoeda(dashboard?.resumo?.totalDespesas || 0)}
            </Text>
          </View>
        </View>
        <Text style={styles.semTransferencias}>* sem transferências entre contas</Text>
      </View>

      {/* Ação Rápida */}
      <TouchableOpacity
        style={styles.acaoRapida}
        onPress={() => navigation.navigate('CriarTransacao')}
        activeOpacity={0.8}
      >
        <Ionicons name="add-circle" size={22} color={colors.primary} />
        <Text style={styles.acaoRapidaTexto}>Registrar transação</Text>
        <Ionicons name="chevron-forward" size={18} color={colors.primary} />
      </TouchableOpacity>

      {/* Resumo do mês */}
      {dashboard?.resumo && (
        <View style={styles.secao}>
          <Text style={styles.secaoTitulo}>Resumo do mês</Text>
          <View style={styles.resumoGrid}>
            <View style={styles.resumoItem}>
              <Ionicons name="receipt-outline" size={20} color={colors.info} />
              <Text style={styles.resumoNumero}>{dashboard.resumo.totalTransacoesMes}</Text>
              <Text style={styles.resumoLabel}>Transações</Text>
            </View>
            <View style={styles.resumoItem}>
              <Ionicons name="trending-down-outline" size={20} color={colors.danger} />
              <Text style={styles.resumoNumero}>{formatarMoeda(dashboard.resumo.mediaDiariaDespesas)}</Text>
              <Text style={styles.resumoLabel}>Média/dia</Text>
            </View>
            <View style={styles.resumoItem}>
              <Ionicons name="flame-outline" size={20} color={colors.warning} />
              <Text style={styles.resumoNumero}>{formatarMoeda(dashboard.resumo.maiorDespesaMes)}</Text>
              <Text style={styles.resumoLabel}>Maior gasto</Text>
            </View>
          </View>
          {dashboard.resumo.categoriaMaisGasta && (
            <Text style={styles.categoriaMaisGasta}>
              Categoria mais gasta: {dashboard.resumo.categoriaMaisGasta}
            </Text>
          )}
        </View>
      )}

      {/* Gastos por categoria */}
      {dashboard?.gastosPorCategoria && dashboard.gastosPorCategoria.length > 0 && (
        <View style={styles.secao}>
          <Text style={styles.secaoTitulo}>Gastos por categoria</Text>
          {dashboard.gastosPorCategoria.map((cat) => (
            <View key={cat.categoriaId} style={styles.catItem}>
              <View style={styles.catEsquerda}>
                <View style={[styles.catDot, { backgroundColor: cat.corCategoria }]} />
                <Text style={styles.catNome}>{cat.nomeCategoria}</Text>
              </View>
              <View style={styles.catDireita}>
                <Text style={styles.catValor}>{formatarMoeda(cat.valorTotal)}</Text>
                <Text style={styles.catPercent}>{cat.percentual}%</Text>
              </View>
              <View style={styles.catBarraFundo}>
                <View
                  style={[
                    styles.catBarra,
                    { width: `${Math.min(cat.percentual, 100)}%`, backgroundColor: cat.corCategoria },
                  ]}
                />
              </View>
            </View>
          ))}
        </View>
      )}

      {/* Balanço últimos 6 meses */}
      {dashboard?.balancoUltimos6Meses && dashboard.balancoUltimos6Meses.length > 0 && (
        <View style={styles.secao}>
          <Text style={styles.secaoTitulo}>Últimos 6 meses</Text>
          <View style={styles.balancoContainer}>
            {dashboard.balancoUltimos6Meses.map((mes) => {
              const maxValor = Math.max(
                ...dashboard.balancoUltimos6Meses.map((m) => Math.max(m.totalReceitas, m.totalDespesas, 1))
              );
              const alturaReceita = (mes.totalReceitas / maxValor) * 80;
              const alturaDespesa = (mes.totalDespesas / maxValor) * 80;

              return (
                <View key={`${mes.ano}-${mes.mes}`} style={styles.balancoMes}>
                  <View style={styles.balancoBarras}>
                    <View style={[styles.balancoBarraReceita, { height: Math.max(alturaReceita, 4) }]} />
                    <View style={[styles.balancoBarraDespesa, { height: Math.max(alturaDespesa, 4) }]} />
                  </View>
                  <Text style={styles.balancoMesNome}>{mes.nomeMes}</Text>
                </View>
              );
            })}
          </View>
          <View style={styles.balancoLegenda}>
            <View style={styles.legendaItem}>
              <View style={[styles.legendaDot, { backgroundColor: colors.success }]} />
              <Text style={styles.legendaTexto}>Receitas</Text>
            </View>
            <View style={styles.legendaItem}>
              <View style={[styles.legendaDot, { backgroundColor: colors.danger }]} />
              <Text style={styles.legendaTexto}>Despesas</Text>
            </View>
          </View>
        </View>
      )}

      {/* Contas */}
      {dashboard?.contas && dashboard.contas.length > 0 && (
        <View style={styles.secao}>
          <Text style={styles.secaoTitulo}>Minhas contas</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false}>
            {dashboard.contas.map((conta) => (
              <View key={conta.id} style={[styles.contaCard, { borderLeftColor: conta.cor }]}>
                <Text style={styles.contaNome}>{conta.nome}</Text>
                {conta.banco && <Text style={styles.contaBanco}>{conta.banco}</Text>}
                <Text style={[styles.contaSaldo, { color: conta.saldoAtual >= 0 ? colors.success : colors.danger }]}>
                  {formatarMoeda(conta.saldoAtual)}
                </Text>
              </View>
            ))}
          </ScrollView>
        </View>
      )}

      {/* Orçamentos */}
      {dashboard?.orcamentos && dashboard.orcamentos.length > 0 && (
        <View style={styles.secao}>
          <Text style={styles.secaoTitulo}>Orçamentos do mês</Text>
          {dashboard.orcamentos.map((orc, i) => (
            <View key={i} style={styles.orcItem}>
              <View style={styles.orcHeader}>
                <Text style={styles.orcNome}>{orc.nomeCategoria}</Text>
                <Text style={styles.orcValores}>
                  {formatarMoeda(orc.totalGasto)} / {formatarMoeda(orc.valorLimite)}
                </Text>
              </View>
              <View style={styles.orcBarraFundo}>
                <View
                  style={[
                    styles.orcBarra,
                    {
                      width: `${Math.min(orc.percentualUsado, 100)}%`,
                      backgroundColor: orc.estourado ? colors.danger : orc.percentualUsado >= 80 ? colors.warning : colors.success,
                    },
                  ]}
                />
              </View>
              <Text style={[styles.orcPercent, orc.estourado && { color: colors.danger }]}>
                {orc.percentualUsado}% usado {orc.estourado ? '⚠️ Estourado!' : ''}
              </Text>
            </View>
          ))}
        </View>
      )}

      {/* Metas */}
      {dashboard?.metas && dashboard.metas.length > 0 && (
        <View style={styles.secao}>
          <Text style={styles.secaoTitulo}>Metas de economia</Text>
          {dashboard.metas.map((meta) => (
            <View key={meta.id} style={styles.metaItem}>
              <Text style={styles.metaTitulo}>{meta.titulo}</Text>
              <View style={styles.metaBarraFundo}>
                <View
                  style={[
                    styles.metaBarra,
                    { width: `${Math.min(meta.percentualConcluido, 100)}%`, backgroundColor: meta.cor || colors.primary },
                  ]}
                />
              </View>
              <View style={styles.metaValores}>
                <Text style={styles.metaAtual}>{formatarMoeda(meta.valorAtual)}</Text>
                <Text style={styles.metaAlvo}>{formatarMoeda(meta.valorAlvo)}</Text>
              </View>
            </View>
          ))}
        </View>
      )}

      {/* Próximas faturas */}
      {dashboard?.proximasFaturas && dashboard.proximasFaturas.length > 0 && (
        <View style={styles.secao}>
          <Text style={styles.secaoTitulo}>Próximas faturas</Text>
          {dashboard.proximasFaturas.map((fat) => (
            <View key={fat.id} style={styles.faturaItem}>
              <View style={[styles.faturaDot, { backgroundColor: fat.corCartao }]} />
              <View style={styles.faturaInfo}>
                <Text style={styles.faturaNome}>{fat.nomeCartao}</Text>
                <Text style={styles.faturaVenc}>
                  Vence em {fat.diasParaVencimento} dias
                </Text>
              </View>
              <Text style={styles.faturaValor}>{formatarMoeda(fat.valorTotal - fat.valorPago)}</Text>
            </View>
          ))}
        </View>
      )}

      {/* Últimas transações */}
      {dashboard?.ultimasTransacoes && dashboard.ultimasTransacoes.length > 0 && (
        <View style={styles.secao}>
          <View style={styles.secaoHeader}>
            <Text style={styles.secaoTitulo}>Últimas transações</Text>
            <TouchableOpacity onPress={() => navigation.navigate('Transacoes')}>
              <Text style={styles.verTudo}>Ver tudo</Text>
            </TouchableOpacity>
          </View>
          {dashboard.ultimasTransacoes.slice(0, 5).map((t) => (
            <View key={t.id} style={styles.transItem}>
              <View style={[styles.transDot, { backgroundColor: t.corCategoria }]} />
              <View style={styles.transInfo}>
                <Text style={styles.transDesc}>{t.descricao || t.nomeCategoria}</Text>
                <Text style={styles.transCat}>{t.nomeCategoria}</Text>
              </View>
              <Text style={[styles.transValor, { color: t.tipo === 'DESPESA' ? colors.danger : colors.success }]}>
                {t.tipo === 'DESPESA' ? '-' : '+'}{formatarMoeda(t.valor)}
              </Text>
            </View>
          ))}
        </View>
      )}

      <View style={{ height: 100 }} />
    </ScrollView>
  );
}

const getStyles = (colors: typeof LightColors) => StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  // Header
  header: {
    flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center',
    paddingHorizontal: Spacing.lg, paddingTop: 60, paddingBottom: Spacing.md,
    backgroundColor: colors.surface,
  },
  saudacao: { fontSize: FontSize.md, color: colors.textSecondary },
  nome: { fontSize: FontSize.xxl, fontWeight: '700', color: colors.textPrimary },
  notifBotao: {
    width: 44, height: 44, borderRadius: 22, backgroundColor: colors.surfaceVariant,
    justifyContent: 'center', alignItems: 'center', position: 'relative',
  },
  notifBadge: {
    position: 'absolute', top: 4, right: 4,
    backgroundColor: colors.danger, borderRadius: 8, minWidth: 16, height: 16,
    justifyContent: 'center', alignItems: 'center', paddingHorizontal: 2,
  },
  notifBadgeTexto: { fontSize: 9, color: '#fff', fontWeight: '700' },
  // Mês navigator
  mesNavigator: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    paddingVertical: Spacing.sm, backgroundColor: colors.surface,
    borderBottomWidth: 1, borderBottomColor: colors.borderLight,
  },
  mesBotao: { padding: Spacing.sm },
  mesTitulo: { fontSize: FontSize.lg, fontWeight: '700', color: colors.textPrimary, minWidth: 180, textAlign: 'center' },
  semTransferencias: {
    fontSize: 10, color: 'rgba(255,255,255,0.5)', marginTop: Spacing.xs, textAlign: 'center', fontStyle: 'italic',
  },
  // Saldo
  saldoCard: {
    backgroundColor: colors.primary, borderRadius: BorderRadius.lg, padding: Spacing.lg,
    margin: Spacing.lg, marginTop: Spacing.md,
  },
  saldoLabel: { fontSize: FontSize.sm, color: 'rgba(255,255,255,0.7)', textTransform: 'uppercase', letterSpacing: 0.5 },
  saldoValor: { fontSize: FontSize.hero, fontWeight: '800', color: '#FFF', marginVertical: 8 },
  saldoLinhaResumo: { flexDirection: 'row', alignItems: 'center', marginTop: Spacing.sm },
  saldoItem: { flex: 1, alignItems: 'center', gap: 4 },
  saldoDivisor: { width: 1, height: 40, backgroundColor: 'rgba(255,255,255,0.2)' },
  saldoItemLabel: { fontSize: FontSize.xs, color: 'rgba(255,255,255,0.7)' },
  saldoReceita: { fontSize: FontSize.lg, fontWeight: '700', color: '#4ADE80' },
  saldoDespesa: { fontSize: FontSize.lg, fontWeight: '700', color: '#F87171' },
  // Ação rápida
  acaoRapida: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: colors.primaryLight,
    marginHorizontal: Spacing.lg, padding: Spacing.md, borderRadius: BorderRadius.sm, gap: 8,
  },
  acaoRapidaTexto: { flex: 1, fontSize: FontSize.md, fontWeight: '600', color: colors.primary },
  // Seções
  secao: { marginHorizontal: Spacing.lg, marginTop: Spacing.lg },
  secaoHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  secaoTitulo: { fontSize: FontSize.lg, fontWeight: '700', color: colors.textPrimary, marginBottom: Spacing.md },
  verTudo: { fontSize: FontSize.md, color: colors.primary, fontWeight: '600' },
  // Resumo
  resumoGrid: { flexDirection: 'row', gap: 10 },
  resumoItem: {
    flex: 1, backgroundColor: colors.surface, borderRadius: BorderRadius.md, padding: Spacing.md,
    alignItems: 'center', gap: 6,
    shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.05, shadowRadius: 4, elevation: 2,
  },
  resumoNumero: { fontSize: FontSize.md, fontWeight: '700', color: colors.textPrimary },
  resumoLabel: { fontSize: FontSize.xs, color: colors.textMuted },
  categoriaMaisGasta: {
    fontSize: FontSize.sm, color: colors.textSecondary, marginTop: Spacing.sm, textAlign: 'center', fontStyle: 'italic',
  },
  // Categorias
  catItem: { marginBottom: Spacing.md },
  catEsquerda: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  catDot: { width: 10, height: 10, borderRadius: 5 },
  catNome: { fontSize: FontSize.md, color: colors.textPrimary, fontWeight: '500' },
  catDireita: { flexDirection: 'row', justifyContent: 'space-between', marginTop: 4 },
  catValor: { fontSize: FontSize.sm, fontWeight: '600', color: colors.textPrimary },
  catPercent: { fontSize: FontSize.sm, color: colors.textMuted },
  catBarraFundo: { height: 6, backgroundColor: colors.surfaceVariant, borderRadius: 3, marginTop: 6 },
  catBarra: { height: 6, borderRadius: 3 },
  // Balanço
  balancoContainer: { flexDirection: 'row', justifyContent: 'space-around', alignItems: 'flex-end', height: 120 },
  balancoMes: { alignItems: 'center', flex: 1 },
  balancoBarras: { flexDirection: 'row', alignItems: 'flex-end', gap: 3, height: 80 },
  balancoBarraReceita: { width: 14, backgroundColor: colors.success, borderRadius: 3 },
  balancoBarraDespesa: { width: 14, backgroundColor: colors.danger, borderRadius: 3 },
  balancoMesNome: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 6 },
  balancoLegenda: { flexDirection: 'row', justifyContent: 'center', gap: 20, marginTop: Spacing.md },
  legendaItem: { flexDirection: 'row', alignItems: 'center', gap: 6 },
  legendaDot: { width: 8, height: 8, borderRadius: 4 },
  legendaTexto: { fontSize: FontSize.xs, color: colors.textMuted },
  // Contas
  contaCard: {
    backgroundColor: colors.surface, borderRadius: BorderRadius.md, padding: Spacing.md,
    marginRight: 12, width: 160, borderLeftWidth: 4,
    shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.05, shadowRadius: 4, elevation: 2,
  },
  contaNome: { fontSize: FontSize.md, fontWeight: '600', color: colors.textPrimary },
  contaBanco: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 2 },
  contaSaldo: { fontSize: FontSize.lg, fontWeight: '700', marginTop: 8 },
  // Orçamentos
  orcItem: { backgroundColor: colors.surface, borderRadius: BorderRadius.md, padding: Spacing.md, marginBottom: 10 },
  orcHeader: { flexDirection: 'row', justifyContent: 'space-between' },
  orcNome: { fontSize: FontSize.md, fontWeight: '600', color: colors.textPrimary },
  orcValores: { fontSize: FontSize.sm, color: colors.textSecondary },
  orcBarraFundo: { height: 8, backgroundColor: colors.surfaceVariant, borderRadius: 4, marginTop: 8 },
  orcBarra: { height: 8, borderRadius: 4 },
  orcPercent: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 4 },
  // Metas
  metaItem: { backgroundColor: colors.surface, borderRadius: BorderRadius.md, padding: Spacing.md, marginBottom: 10 },
  metaTitulo: { fontSize: FontSize.md, fontWeight: '600', color: colors.textPrimary },
  metaBarraFundo: { height: 8, backgroundColor: colors.surfaceVariant, borderRadius: 4, marginTop: 8 },
  metaBarra: { height: 8, borderRadius: 4 },
  metaValores: { flexDirection: 'row', justifyContent: 'space-between', marginTop: 4 },
  metaAtual: { fontSize: FontSize.sm, color: colors.primary, fontWeight: '600' },
  metaAlvo: { fontSize: FontSize.sm, color: colors.textMuted },
  // Faturas
  faturaItem: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: colors.surface,
    borderRadius: BorderRadius.md, padding: Spacing.md, marginBottom: 8, gap: 12,
  },
  faturaDot: { width: 10, height: 10, borderRadius: 5 },
  faturaInfo: { flex: 1 },
  faturaNome: { fontSize: FontSize.md, fontWeight: '600', color: colors.textPrimary },
  faturaVenc: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 2 },
  faturaValor: { fontSize: FontSize.md, fontWeight: '700', color: colors.danger },
  // Transações
  transItem: {
    flexDirection: 'row', alignItems: 'center', paddingVertical: 10,
    borderBottomWidth: 1, borderBottomColor: colors.borderLight, gap: 12,
  },
  transDot: { width: 10, height: 10, borderRadius: 5 },
  transInfo: { flex: 1 },
  transDesc: { fontSize: FontSize.md, fontWeight: '500', color: colors.textPrimary },
  transCat: { fontSize: FontSize.xs, color: colors.textMuted, marginTop: 2 },
  transValor: { fontSize: FontSize.md, fontWeight: '700' },
});
