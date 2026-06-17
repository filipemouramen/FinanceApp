const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

const BASE_URL = 'http://localhost:8081';
const SCREENSHOTS_DIR = path.join(__dirname, 'screenshots');

const USUARIO_TESTE = {
  nome: 'Playwright Teste',
  email: `playwright${Date.now()}@teste.com`,
  senha: 'Teste@123',
};

// Credenciais reais para testar as telas autenticadas
const USUARIO_REAL = {
  email: 'filipemouramen@gmail.com',
  senha: 'Campeao09@',
};

function shot(nome) { return path.join(SCREENSHOTS_DIR, `${nome}.png`); }
async function esperar(ms) { return new Promise(r => setTimeout(r, ms)); }

// Preenche input React Native Web via native value setter.
// Dispara eventos em TODOS os inputs com o placeholder (resolve duplicatas de navegação).
async function preencher(page, placeholder, valor) {
  const preencheu = await page.evaluate(({ ph, val }) => {
    const inputs = Array.from(document.querySelectorAll(`input[placeholder="${ph}"]`));
    if (inputs.length === 0) throw new Error(`Input não encontrado: ${ph}`);
    const setter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value')?.set;
    let count = 0;
    inputs.forEach(input => {
      if (setter) setter.call(input, val); else input.value = val;
      input.dispatchEvent(new Event('input',  { bubbles: true }));
      input.dispatchEvent(new Event('change', { bubbles: true }));
      count++;
    });
    return count;
  }, { ph: placeholder, val: valor });
  await esperar(200);
  return preencheu;
}

// Clica em qualquer elemento que mostra o texto.
// Dispara pointer events em toda a hierarquia para garantir que RN Web receba o evento.
async function clicarElementoComTexto(page, texto) {
  await page.evaluate((txt) => {
    const todos = Array.from(document.querySelectorAll('*'));
    const folha = todos.find(el => el.childElementCount === 0 && el.textContent.trim() === txt);
    if (!folha) throw new Error(`"${txt}" não encontrado no DOM`);
    let el = folha;
    while (el && el !== document.body) {
      el.dispatchEvent(new PointerEvent('pointerover',  { bubbles: true, isPrimary: true }));
      el.dispatchEvent(new PointerEvent('pointerdown',  { bubbles: true, isPrimary: true, cancelable: true }));
      el.dispatchEvent(new PointerEvent('pointerup',    { bubbles: true, isPrimary: true, cancelable: true }));
      el.dispatchEvent(new MouseEvent('click',          { bubbles: true, cancelable: true }));
      el = el.parentElement;
    }
  }, texto);
  await esperar(500);
}

const API_URL = 'http://localhost:7137';

async function criarUsuarioDeTeste() {
  console.log('🔧 Criando usuário de teste via API...');
  const email = `playwright${Date.now()}@teste.com`;
  const senha = 'PlaywrightTeste@123';

  // Registra
  const resReg = await fetch(`${API_URL}/api/auth/registrar`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ nomeCompleto: 'Playwright Teste', email, senha, confirmarSenha: senha }),
  });
  const dadosReg = await resReg.json();
  console.log(`   📝 Registro: ${resReg.status} — sucesso=${dadosReg.sucesso}`);

  // Tenta login pela API para obter token
  const resLogin = await fetch(`${API_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, senha }),
  });
  const dadosLogin = await resLogin.json();
  console.log(`   🔑 Login API: ${resLogin.status} — sucesso=${dadosLogin.sucesso}`);

  if (dadosLogin.sucesso && dadosLogin.dados?.token) {
    console.log(`   ✅ Token obtido para: ${email}\n`);
    return { email, senha, token: dadosLogin.dados.token, refreshToken: dadosLogin.dados.refreshToken, usuario: dadosLogin.dados.usuario };
  }

  // Fallback: tenta com credenciais reais
  const resLoginReal = await fetch(`${API_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(USUARIO_REAL),
  });
  const dadosLoginReal = await resLoginReal.json();
  if (dadosLoginReal.sucesso && dadosLoginReal.dados?.token) {
    console.log(`   ✅ Token obtido (fallback): ${USUARIO_REAL.email}\n`);
    return { ...USUARIO_REAL, token: dadosLoginReal.dados.token, refreshToken: dadosLoginReal.dados.refreshToken, usuario: dadosLoginReal.dados.usuario };
  }

  console.log('   ⚠️  Sem token — login via UI será tentado\n');
  return { email, senha, token: null, usuario: null };
}

async function main() {
  if (!fs.existsSync(SCREENSHOTS_DIR)) fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
  console.log('🚀 Iniciando testes Playwright — FinanceApp Web\n');

  // Descobre a senha correta: tenta criar novo usuário via API
  const usuarioLogin = await criarUsuarioDeTeste();

  const browser = await chromium.launch({ headless: false, slowMo: 150 });
  const context = await browser.newContext({ viewport: { width: 390, height: 844 }, deviceScaleFactor: 2 });
  const page = await context.newPage();
  // Auto-dismiss alerts (React Native Alert.alert no web vira window.alert)
  page.on('dialog', async dialog => { console.log(`   ⚠️  Dialog: ${dialog.message()}`); await dialog.accept(); });
  page.on('console', msg => { if (msg.type() === 'error') console.log(`   🔴 Console error: ${msg.text()}`); });
  page.on('pageerror', err => console.log(`   🔴 Page error: ${err.message}`));

  // Monitora respostas da API de login
  page.on('response', async res => {
    if (res.url().includes('/auth/login') || res.url().includes('/auth/registrar')) {
      console.log(`   🌐 API ${res.url().split('/').slice(-2).join('/')} → ${res.status()}`);
    }
  });

  try {
    // ── 01. LOGIN ────────────────────────────────────────────────
    console.log('📸 01 — Tela de Login');
    await page.goto(BASE_URL, { waitUntil: 'networkidle', timeout: 30000 });
    await esperar(2500);
    await page.screenshot({ path: shot('01-login'), fullPage: true });
    console.log('   ✅\n');

    // ── 02. TELA DE REGISTRO (vazia) ────────────────────────────
    console.log('📸 02 — Tela de Registro (vazia)');
    await clicarElementoComTexto(page, 'Criar conta');
    await esperar(2000);
    await page.screenshot({ path: shot('02-registro'), fullPage: true });
    console.log('   ✅\n');

    // ── 03. REGISTRO PREENCHIDO ─────────────────────────────────
    console.log('📸 03 — Registro preenchido');
    await preencher(page, 'Seu nome completo', USUARIO_TESTE.nome);
    await preencher(page, 'seu@email.com', USUARIO_TESTE.email);
    await preencher(page, 'Mínimo 6 caracteres', USUARIO_TESTE.senha);
    await preencher(page, 'Repita a senha', USUARIO_TESTE.senha);
    try { await preencher(page, '(00) 00000-0000', '11999999999'); } catch {}
    await esperar(500);
    await page.screenshot({ path: shot('03-registro-preenchido'), fullPage: true });
    console.log('   ✅\n');

    // ── 04. ACEITAR TERMOS LGPD ─────────────────────────────────
    console.log('📸 04 — Aceitar termos LGPD');
    // TouchableOpacity renderiza como div[role="button"] no RN Web
    await page.evaluate(() => {
      const buttons = Array.from(document.querySelectorAll('[role="button"]'));
      // Pega o button que contém exatamente o texto dos termos (não o botão Criar conta)
      const termos = buttons.find(b =>
        b.textContent.includes('Li e aceito') &&
        !b.textContent.includes('Criar conta') &&
        !b.textContent.includes('Fazer login')
      );
      if (termos) termos.click();
    });
    await esperar(500);
    await page.screenshot({ path: shot('04-registro-termos'), fullPage: true });
    console.log('   ✅\n');

    // ── 05. SUBMETER REGISTRO ────────────────────────────────────
    console.log('📸 05 — Submetendo registro...');
    await page.evaluate(() => {
      const buttons = Array.from(document.querySelectorAll('[role="button"]'));
      const submit = buttons.find(b =>
        b.textContent.includes('Criar conta') &&
        !b.textContent.includes('Fazer login') &&
        !b.textContent.includes('Não tem conta') &&
        !b.textContent.includes('Li e aceito')
      );
      if (submit) { submit.scrollIntoView({ block: 'center' }); submit.click(); }
    });
    await esperar(4000);
    await page.screenshot({ path: shot('05-pos-registro'), fullPage: true });
    console.log('   ✅\n');

    // ── 06. ESQUECI MINHA SENHA ──────────────────────────────────
    console.log('📸 06 — Esqueci minha senha');
    await page.goto(BASE_URL, { waitUntil: 'networkidle', timeout: 15000 });
    await esperar(2000);
    await clicarElementoComTexto(page, 'Esqueci minha senha');
    await esperar(2000);
    await page.screenshot({ path: shot('06-esqueci-senha'), fullPage: true });
    console.log('   ✅\n');

    // ── 07. ESQUECI SENHA — PREENCHIDO ───────────────────────────
    try {
      await preencher(page, 'seu@email.com', USUARIO_TESTE.email);
      await esperar(400);
      await page.screenshot({ path: shot('07-esqueci-senha-preenchido'), fullPage: true });
      console.log('📸 07 — Esqueci senha preenchido ✅\n');
    } catch { console.log('📸 07 — Pulado\n'); }

    // ── 08. LOGIN PREENCHIDO ─────────────────────────────────────
    console.log('📸 08 — Login preenchido');
    // Limpa sessão para garantir que estamos na tela de login (não auto-redirecionado)
    await page.goto(BASE_URL, { waitUntil: 'networkidle', timeout: 15000 });
    await page.evaluate(() => { try { localStorage.clear(); sessionStorage.clear(); } catch {} });
    await page.reload({ waitUntil: 'networkidle', timeout: 15000 });
    await esperar(2500);

    await preencher(page, 'seu@email.com', usuarioLogin.email);
    await preencher(page, 'Sua senha', usuarioLogin.senha);
    await esperar(500);
    await page.screenshot({ path: shot('08-login-preenchido'), fullPage: true });
    console.log('   ✅\n');

    // ── 09. PÓS-LOGIN ────────────────────────────────────────────
    console.log('📸 09 — Fazendo login...');
    // Usa helper de teste exposto pelo AuthContext (__testSetAuth) para definir sessão diretamente
    if (usuarioLogin.token) {
      await page.evaluate((dados) => {
        if (typeof window.__testSetAuth === 'function') {
          window.__testSetAuth(dados);
        }
      }, { token: usuarioLogin.token, refreshToken: usuarioLogin.token, usuario: usuarioLogin.usuario });
      await esperar(3000);
    }
    await page.screenshot({ path: shot('09-pos-login'), fullPage: true });
    console.log('   ✅\n');

    // ── 10. HOME / DASHBOARD ─────────────────────────────────────
    console.log('📸 10 — Home (topo)');
    await esperar(1500);
    const textoPos = await page.evaluate(() => document.body.innerText.replace(/\s+/g,' ').slice(0,200));
    console.log('   🔍 Após login:', textoPos);
    await page.screenshot({ path: shot('10-home'), fullPage: true });
    console.log('   ✅\n');

    console.log('📸 11 — Home (scroll)');
    await page.mouse.wheel(0, 600);
    await esperar(1000);
    await page.screenshot({ path: shot('11-home-scroll'), fullPage: true });
    console.log('   ✅\n');

    // ── 12. TRANSAÇÕES ───────────────────────────────────────────
    console.log('📸 12 — Transações');
    await clicarElementoComTexto(page, 'Transações');
    await esperar(2500);
    await page.screenshot({ path: shot('12-transacoes'), fullPage: true });
    console.log('   ✅\n');

    // ── 13. CRIAR TRANSAÇÃO ──────────────────────────────────────
    console.log('📸 13 — Criar Transação');
    try {
      await page.evaluate(() => { if (typeof window.__testNavigate === 'function') window.__testNavigate('CriarTransacao'); });
      await esperar(2500);
      await page.screenshot({ path: shot('13-criar-transacao'), fullPage: true });
      console.log('   ✅\n');
      await page.evaluate(() => { if (typeof window.__testGoBack === 'function') window.__testGoBack(); });
      await esperar(1500);
    } catch {
      await page.screenshot({ path: shot('13-criar-transacao'), fullPage: true });
      console.log('   ⚠️  Screenshot da tela atual\n');
    }

    // ── 14. CONTAS ───────────────────────────────────────────────
    console.log('📸 14 — Contas');
    await clicarElementoComTexto(page, 'Contas');
    await esperar(2500);
    await page.screenshot({ path: shot('14-contas'), fullPage: true });
    console.log('   ✅\n');

    // ── 15. CARTÕES ──────────────────────────────────────────────
    console.log('📸 15 — Cartões');
    await clicarElementoComTexto(page, 'Cartões');
    await esperar(2500);
    await page.screenshot({ path: shot('15-cartoes'), fullPage: true });
    console.log('   ✅\n');

    // ── 16. CRIAR CARTÃO ─────────────────────────────────────────
    console.log('📸 16 — Criar Cartão');
    try {
      await page.evaluate(() => { if (typeof window.__testNavigate === 'function') window.__testNavigate('CriarCartao'); });
      await esperar(2500);
      await page.screenshot({ path: shot('16-criar-cartao'), fullPage: true });
      console.log('   ✅\n');
      await page.evaluate(() => { if (typeof window.__testGoBack === 'function') window.__testGoBack(); });
      await esperar(1500);
    } catch {
      await page.screenshot({ path: shot('16-criar-cartao'), fullPage: true });
      console.log('   ⚠️  Screenshot da tela atual\n');
    }

    // ── 17. CONFIGURAÇÕES ────────────────────────────────────────
    console.log('📸 17 — Configurações');
    await clicarElementoComTexto(page, 'Config');
    await esperar(2500);
    await page.screenshot({ path: shot('17-config'), fullPage: true });
    console.log('   ✅\n');

    console.log('📸 18 — Config (scroll)');
    await page.mouse.wheel(0, 500);
    await esperar(800);
    await page.screenshot({ path: shot('18-config-scroll'), fullPage: true });
    console.log('   ✅\n');

    // ── 19. ORÇAMENTOS ───────────────────────────────────────────
    console.log('📸 19 — Orçamentos');
    try {
      await page.evaluate(() => { if (typeof window.__testNavigate === 'function') window.__testNavigate('Orcamentos'); });
      await esperar(2500);
      await page.screenshot({ path: shot('19-orcamentos'), fullPage: true });
      console.log('   ✅\n');
      await page.evaluate(() => { if (typeof window.__testGoBack === 'function') window.__testGoBack(); });
      await esperar(1000);
    } catch { console.log('   ⚠️  Pulado\n'); }

    // ── 20. METAS ────────────────────────────────────────────────
    console.log('📸 20 — Metas');
    try {
      await page.evaluate(() => { if (typeof window.__testNavigate === 'function') window.__testNavigate('Metas'); });
      await esperar(2500);
      await page.screenshot({ path: shot('20-metas'), fullPage: true });
      console.log('   ✅\n');
      await page.evaluate(() => { if (typeof window.__testGoBack === 'function') window.__testGoBack(); });
      await esperar(1000);
    } catch { console.log('   ⚠️  Pulado\n'); }

    // ── 21. CATEGORIAS ───────────────────────────────────────────
    console.log('📸 21 — Categorias');
    try {
      await page.evaluate(() => { if (typeof window.__testNavigate === 'function') window.__testNavigate('Categorias'); });
      await esperar(2500);
      await page.screenshot({ path: shot('21-categorias'), fullPage: true });
      console.log('   ✅\n');
      await page.evaluate(() => { if (typeof window.__testGoBack === 'function') window.__testGoBack(); });
      await esperar(1000);
    } catch { console.log('   ⚠️  Pulado\n'); }

    // ── 22. NOTIFICAÇÕES ─────────────────────────────────────────
    console.log('📸 22 — Notificações');
    try {
      await page.evaluate(() => { if (typeof window.__testNavigate === 'function') window.__testNavigate('Notificacoes'); });
      await esperar(2500);
      await page.screenshot({ path: shot('22-notificacoes'), fullPage: true });
      console.log('   ✅\n');
      await page.evaluate(() => { if (typeof window.__testGoBack === 'function') window.__testGoBack(); });
      await esperar(1000);
    } catch { console.log('   ⚠️  Pulado\n'); }

    // ── 23. TRANSFERÊNCIAS ───────────────────────────────────────
    console.log('📸 23 — Transferências');
    try {
      await page.evaluate(() => { if (typeof window.__testNavigate === 'function') window.__testNavigate('Transferencia'); });
      await esperar(2500);
      await page.screenshot({ path: shot('23-transferencias'), fullPage: true });
      console.log('   ✅\n');
      await page.evaluate(() => { if (typeof window.__testGoBack === 'function') window.__testGoBack(); });
      await esperar(1000);
    } catch { console.log('   ⚠️  Pulado\n'); }

    // ── 24. EXPORTAÇÃO ───────────────────────────────────────────
    console.log('📸 24 — Exportação');
    try {
      await clicarElementoComTexto(page, 'Config');
      await esperar(1500);
      await page.mouse.wheel(0, 600);
      await esperar(800);
      await clicarElementoComTexto(page, 'Exportar extrato PDF');
      await esperar(2500);
      await page.screenshot({ path: shot('24-exportacao'), fullPage: true });
      console.log('   ✅\n');
    } catch { console.log('   ⚠️  Pulado\n'); }

    // ── 25. HOME FINAL ───────────────────────────────────────────
    console.log('📸 25 — Home final');
    await clicarElementoComTexto(page, 'Início');
    await esperar(2000);
    await page.screenshot({ path: shot('25-home-final'), fullPage: true });
    console.log('   ✅\n');

  } catch (err) {
    console.error('\n❌ Erro inesperado:', err.message);
    await page.screenshot({ path: shot('ERRO'), fullPage: true });
  } finally {
    await esperar(1000);
    await browser.close();
    const arquivos = fs.readdirSync(SCREENSHOTS_DIR).filter(f => f.endsWith('.png'));
    console.log('\n════════════════════════════════════════');
    console.log(`✅ ${arquivos.length} screenshots em: ${SCREENSHOTS_DIR}`);
    console.log('════════════════════════════════════════');
    arquivos.forEach(f => console.log(`   📷 ${f}`));
  }
}

main();
