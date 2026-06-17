# Relatório de Segurança — Padrão OWASP Top 10

**Projeto:** FinanceApp — Aplicativo de Finanças Pessoais  
**Versão:** 2.0  
**Data:** 17/06/2026  
**Autor:** Filipe Moura  
**Disciplina:** H1 Mobile  

---

## Introdução

Este relatório analisa o projeto FinanceApp com base no **OWASP Top 10 (2021)**, que lista as dez vulnerabilidades de segurança mais críticas em aplicações web e APIs. Para cada item, foi verificado o estado atual do código e indicado se o risco foi **resolvido**, **parcialmente resolvido** ou **pendente**.

A aplicação é composta por:
- **Backend:** ASP.NET Core 8 Web API (C#)
- **Banco de dados:** SQL Server com Entity Framework Core
- **Frontend:** React Native (Expo) — aplicativo mobile

---

## Resumo Executivo

| # | Vulnerabilidade OWASP | Status |
|---|----------------------|--------|
| A01 | Controle de Acesso Quebrado | ✅ Resolvido |
| A02 | Falhas Criptográficas | ⚠️ Parcialmente Resolvido |
| A03 | Injeção | ✅ Resolvido |
| A04 | Design Inseguro | ⚠️ Parcialmente Resolvido |
| A05 | Configuração de Segurança Incorreta | ⚠️ Parcialmente Resolvido |
| A06 | Componentes Vulneráveis e Desatualizados | ℹ️ Não Avaliado |
| A07 | Falhas de Identificação e Autenticação | ✅ Resolvido |
| A08 | Falhas de Integridade de Software e Dados | ✅ Resolvido |
| A09 | Falhas de Log e Monitoramento de Segurança | ✅ Resolvido |
| A10 | Falsificação de Requisição do Lado do Servidor (SSRF) | ✅ Resolvido / N.A. |

---

## A01 — Controle de Acesso Quebrado ✅ RESOLVIDO

### O que é
Ocorre quando usuários conseguem acessar dados ou funcionalidades além do que deveriam ter permissão.

### Análise do Projeto

**Todos os controladores da API exigem autenticação:**
```csharp
// Todos os controllers de dados usam [Authorize] no nível da classe
[Authorize]
public class TransacoesController : BaseController { ... }

[Authorize]
public class ContasController : BaseController { ... }

// Apenas endpoints públicos usam [AllowAnonymous]
[AllowAnonymous]
[HttpPost("login")]
public async Task<IActionResult> Login(...) { ... }
```

**Isolamento de dados por usuário:** todos os serviços filtram dados pelo `UsuarioId` extraído do token JWT:
```csharp
// CartaoCreditoService.cs — exemplo de isolamento por usuário
.Where(c => c.Id == cartaoId && c.UsuarioId == usuarioId && c.Ativo)
```

**BaseController extrai o ID do token, não da requisição:**
```csharp
protected int UsuarioIdAtual =>
    int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("Usuário não autenticado."));
```

**Soft Delete com Query Filter global** impede acesso a dados logicamente excluídos.

### Resultado
Controle de acesso implementado corretamente. Não é possível um usuário acessar dados de outro usuário.

---

## A02 — Falhas Criptográficas ⚠️ PARCIALMENTE RESOLVIDO

### O que é
Uso inadequado (ou ausência) de criptografia para proteger dados sensíveis em trânsito ou em repouso.

### Análise do Projeto

#### ✅ Implementado corretamente

**Hash de senhas com PBKDF2 (350.000 iterações):**
```csharp
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
    options.IterationCount = 350_000; // Recomendação OWASP: mínimo 310.000
});
```

**OTP (código de verificação) armazenado como hash HMAC-SHA256:**
```csharp
private string HashCodigo(string codigo)
{
    using var hmac = new HMACSHA256(chave);
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(codigo));
    return Convert.ToHexString(hash).ToLowerInvariant();
}
```

**HTTPS obrigatório em produção:**
```csharp
options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
// Em produção:
app.UseHsts();
app.UseHttpsRedirection();
```

#### ⚠️ Pendente / Observações

**Segredo JWT e string de conexão expostos no histórico do Git:**  
O arquivo `appsettings.Development.json` contém o secret JWT em texto claro e foi commitado. Embora o `appsettings.Production.json` esteja no `.gitignore`, o histórico Git guarda o valor de desenvolvimento.

**Ação necessária:**
```bash
# Rotacionar o JWT Secret e usar .NET User Secrets em desenvolvimento
dotnet user-secrets set "Jwt:Secret" "NOVA_CHAVE_SECRETA_AQUI"
```

**Tokens de atualização (refresh tokens) não sofrem hash no banco:**  
Os refresh tokens são strings aleatórias armazenadas diretamente — se o banco for comprometido, todos os tokens ficam expostos. A recomendação é aplicar SHA-256 antes de salvar.

### Resultado
Criptografia aplicada em pontos críticos (senhas, OTP, HTTPS). Pendente: rotação de secrets expostos no Git e hash dos refresh tokens.

---

## A03 — Injeção ✅ RESOLVIDO

### O que é
Envio de dados hostis a um interpretador (SQL, LDAP, OS) para executar comandos não intencionais. SQL Injection é o caso mais comum.

### Análise do Projeto

**Entity Framework Core com LINQ:** toda comunicação com o banco usa consultas parametrizadas automaticamente. Nenhuma concatenação de string SQL foi encontrada no código.

```csharp
// Exemplo — ContaService.cs
var conta = await _context.Contas
    .Where(c => c.Id == id && c.UsuarioId == usuarioId)
    .FirstOrDefaultAsync();
```

**Validação de entrada via Data Annotations nos DTOs:**
```csharp
public class RegistroRequest
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string NomeCompleto { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Senha { get; set; }
}
```

**Serialização JSON com camelCase e sem campos nulos** evita exposição acidental de dados internos.

### Resultado
Risco de injeção SQL eliminado pelo uso de ORM com queries parametrizadas. Validação de entrada presente em todos os endpoints públicos.

---

## A04 — Design Inseguro ⚠️ PARCIALMENTE RESOLVIDO

### O que é
Ausência de controles de segurança desde a fase de design da aplicação — não é um erro de implementação, mas de arquitetura.

### Análise do Projeto

#### ✅ Boas práticas de design implementadas

**Rate Limiting nos endpoints de autenticação:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,         // máximo 5 tentativas
                Window = TimeSpan.FromMinutes(1)  // por minuto
            }));
});

[EnableRateLimiting("auth")]
[HttpPost("login")]
public async Task<IActionResult> Login(...) { ... }
```

**Bloqueio de conta após tentativas falhas:**
```csharp
options.Lockout.MaxFailedAccessAttempts = 5;
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
```

**Soft Delete** preserva histórico e integridade referencial mesmo após exclusão.

**Isolamento de dados por usuário** (ver A01).

#### ⚠️ Ponto de atenção

**Política de senha abaixo do recomendado OWASP:**
```csharp
options.Password.RequireUppercase = false;         // OWASP recomenda maiúscula
options.Password.RequireNonAlphanumeric = false;   // OWASP recomenda caractere especial
options.Password.RequiredLength = 6;               // OWASP recomenda mínimo 8
```

A política atual aceita senhas simples como `abc123`. Recomenda-se aumentar o mínimo para 8 caracteres e exigir ao menos 1 caractere especial.

### Resultado
Principais controles de design implementados (rate limiting, lockout, isolamento). Política de senha precisa ser fortalecida.

---

## A05 — Configuração de Segurança Incorreta ⚠️ PARCIALMENTE RESOLVIDO

### O que é
Configurações padrão inseguras, mensagens de erro detalhadas expostas, permissões desnecessárias habilitadas.

### Análise do Projeto

#### ✅ Configurado corretamente

**Swagger desativado em produção:**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(...);
}
```

**`appsettings.json` de produção usa placeholders (não contém secrets reais):**
```json
{
  "Jwt": {
    "Secret": "SUBSTITUA_POR_CHAVE_SECRETA_COM_MINIMO_32_CARACTERES_VIA_SECRETS"
  },
  "AllowedHosts": "SUBSTITUA_PELO_DOMINIO_DE_PRODUCAO"
}
```

**`.gitignore` bloqueia arquivos sensíveis:**
```gitignore
appsettings.Production.json
appsettings.Staging.json
secrets.json
.env
finance-app-mobile/.env
```

**HSTS e redirecionamento HTTPS ativados em produção.**

#### ⚠️ Pendente

**CORS amplo em desenvolvimento:**  
`appsettings.Development.json` permite múltiplas origens incluindo IPs de rede local. Embora aceitável em desenvolvimento, deve ser restrito antes de qualquer deploy.

**`AllowedHosts: "*"` no ambiente de desenvolvimento** — aceitável só localmente.

**`appsettings.Development.json` no repositório** contém o JWT Secret de desenvolvimento. Deveria usar `dotnet user-secrets`.

### Resultado
Configuração de produção adequada. Desenvolvimento usa algumas permissividades aceitáveis localmente, mas o arquivo de configuração com o secret JWT não deveria estar no repositório.

---

## A06 — Componentes Vulneráveis e Desatualizados ℹ️ NÃO AVALIADO FORMALMENTE

### O que é
Uso de bibliotecas, frameworks ou dependências com vulnerabilidades conhecidas.

### Análise do Projeto

O projeto usa:
- **ASP.NET Core 8** — versão LTS atual, suporte até novembro de 2026
- **Entity Framework Core 8** — versão atual
- **Microsoft.AspNetCore.Identity** — gerenciado pela Microsoft
- **React Native / Expo** — no lado mobile

**Ação recomendada para produção:**
```bash
# Verificar vulnerabilidades no .NET
dotnet list package --vulnerable

# Verificar vulnerabilidades no Node.js/Expo
cd finance-app-mobile
npm audit
```

### Resultado
Componentes aparentemente atualizados (projeto recente). Auditoria formal de dependências não foi executada nesta análise.

---

## A07 — Falhas de Identificação e Autenticação ✅ RESOLVIDO

### O que é
Implementações fracas de autenticação que permitem ataques de força bruta, credential stuffing ou sequestro de sessão.

### Análise do Projeto

**JWT com validação completa:**
```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,   // valida assinatura
    ValidateIssuer = true,             // valida emissor
    ValidateAudience = true,           // valida audiência
    ValidateLifetime = true,           // valida expiração
    ClockSkew = TimeSpan.Zero          // sem tolerância de tempo
};
```

**Tokens de curta duração + Refresh Token:**
- JWT expira em 60 minutos
- Refresh Token válido por 7 dias
- Refresh Tokens são revogados no logout

**Proteção contra força bruta:**
- Rate limiting: 5 requisições/minuto por IP nos endpoints de auth
- Lockout: 5 tentativas erradas → bloqueio por 5 minutos
- Mensagem de erro genérica: `"E-mail ou senha inválidos"` (não revela qual está errado)

**Verificação OTP para reset de senha** com código com hash no banco.

**Unique email** enforçado no Identity.

### Resultado
Autenticação robusta com múltiplas camadas de proteção. Atende aos requisitos OWASP para autenticação segura.

---

## A08 — Falhas de Integridade de Software e Dados ✅ RESOLVIDO

### O que é
Código ou dados sem proteção de integridade — atualizações sem verificação de assinatura, deserialização insegura.

### Análise do Projeto

**Assinatura JWT verificada em toda requisição** — qualquer token adulterado é rejeitado.

**Audit Interceptor** registra automaticamente todas as alterações no banco:
```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    // Atualiza AtualizadoEm em toda modificação
    // Converte Delete em SoftDelete com timestamp
}
```

**Log de auditoria** armazena todas as ações sensíveis (login, alterações de perfil, etc.) na tabela `LogsAuditoria`.

**Deserialização segura** — uso do `System.Text.Json` com política camelCase, sem tipos customizados inseguros.

**Sem `[FromBody]` sem validação** — todos os DTOs usam Data Annotations e o ModelState é validado automaticamente pelo `[ApiController]`.

### Resultado
Integridade de dados garantida por JWT, audit log e interceptor de mudanças.

---

## A09 — Falhas de Log e Monitoramento de Segurança ✅ RESOLVIDO

### O que é
Ausência de logs de eventos de segurança, impedindo detecção de ataques e resposta a incidentes.

### Análise do Projeto

**Login registrado na tabela de auditoria:**
```csharp
_context.LogsAuditoria.Add(new LogAuditoria
{
    UsuarioId = usuario.Id,
    Acao = "LOGIN",
    TipoEntidade = "Usuario",
    Detalhes = "Login realizado com sucesso"
});
```

**Falhas de login rastreadas** pelo ASP.NET Identity (`AccessFailedAsync`).

**Interceptor de auditoria automático** registra toda criação/edição no banco com timestamp.

**Serviço de purge** (`LogAuditoriaPurgeService`) gerencia retenção dos logs.

**Logs de nível `Warning`** para eventos do ASP.NET Core via configuração.

#### ⚠️ Ponto de melhoria

Em produção, recomenda-se enviar logs para um serviço externo (Azure Application Insights, Elastic, etc.) para garantir que os logs sobrevivam a falhas do servidor.

### Resultado
Log e monitoramento de segurança implementados para as principais ações. Adequado para o escopo do projeto.

---

## A10 — Falsificação de Requisição do Lado do Servidor (SSRF) ✅ RESOLVIDO / N.A.

### O que é
O atacante faz com que o servidor realize requisições HTTP para destinos arbitrários (URLs fornecidas pelo usuário).

### Análise do Projeto

A API do FinanceApp **não aceita URLs fornecidas pelo usuário** para realizar requisições server-side. As únicas integrações externas são:

- **E-mail** (SMTP configurado no `appsettings`) — destino fixo, não controlado pelo usuário
- **WhatsApp Bot** — integração por mensagens recebidas, não por URL do usuário

Não há endpoints que recebam uma URL e realizem uma requisição HTTP a partir do servidor.

### Resultado
Risco de SSRF não aplicável à arquitetura atual do projeto.

---

## Conclusão e Plano de Ação

### Itens resolvidos (7/10)
- A01 — Controle de acesso por JWT + filtro por UsuarioId
- A03 — ORM com queries parametrizadas + validação de entrada
- A07 — Autenticação com JWT, lockout e rate limiting
- A08 — Audit log e verificação de assinatura JWT
- A09 — Log de auditoria de segurança
- A10 — SSRF não aplicável

### Itens com pendências (3/10)

| Item | Pendência | Prioridade |
|------|-----------|-----------|
| A02 | Rotacionar JWT Secret exposto no histórico do Git | 🔴 Alta |
| A02 | Aplicar hash nos Refresh Tokens antes de salvar no banco | 🟡 Média |
| A04 | Aumentar requisitos de senha (mínimo 8 chars, 1 especial) | 🟡 Média |
| A05 | Mover `appsettings.Development.json` para `dotnet user-secrets` | 🟡 Média |
| A05 | Restringir CORS ao domínio específico em staging/produção | 🟡 Média |
| A06 | Executar `dotnet list package --vulnerable` e `npm audit` | 🟢 Baixa |

---

*Relatório gerado para fins acadêmicos — Disciplina H1 Mobile*  
*Metodologia: análise estática de código-fonte com base no OWASP Top 10 (2021)*  
*Referência: https://owasp.org/Top10/*
