# Contrato: Autenticação e Reset de Senha

**Base URL**: `/api/auth`

---

## POST /api/auth/registrar *(AllowAnonymous)*

**Request Body**:
```json
{
  "nomeCompleto": "João Silva",
  "email": "joao@email.com",
  "telefoneWhatsApp": "(11) 99999-9999",
  "senha": "minhasenha123",
  "confirmarSenha": "minhasenha123"
}
```

**Validações**: Senha min 6 chars, email único, confirmação igual.

**Response 201**: `{ "accessToken": "...", "refreshToken": "...", "expiresIn": 900, "usuario": {...} }`

---

## POST /api/auth/login *(AllowAnonymous)*

**Request Body**: `{ "email": "...", "senha": "..." }`

**Response 200**: `{ "accessToken": "...", "refreshToken": "...", "expiresIn": 900, "usuario": {...} }`

**Access Token**: Expira em **900 segundos (15 minutos)**
**Refresh Token**: Expira em **7 dias**, rotacionado a cada uso

---

## POST /api/auth/refresh-token *(AllowAnonymous)*

**Request Body**: `{ "refreshToken": "..." }`

**Behavior**: Valida token, cria novo par access+refresh, invalida o token anterior (rotação).

**Response 200**: `{ "accessToken": "...", "refreshToken": "...", "expiresIn": 900 }`
**Response 401**: Token inválido, expirado ou já rotacionado.

---

## POST /api/auth/solicitar-reset-senha *(AllowAnonymous)*

Inicia o fluxo de recuperação de senha.

**Request Body**:
```json
{
  "email": "joao@email.com",
  "canal": "EMAIL"
}
```

`canal`: `EMAIL` (sempre disponível) ou `WHATSAPP` (somente se usuário tem `TelefoneWhatsApp` cadastrado).

**Behavior**:
1. Localiza usuário pelo email (se não existir, retorna 200 silenciosamente — não revelar se email existe)
2. Se canal `WHATSAPP` mas sem telefone cadastrado: retorna 422
3. Invalida códigos anteriores do usuário
4. Gera código de 6 dígitos numéricos
5. Persiste em `CodigoVerificacao` com `Expira = now + 15min`, `Canal = canal`
6. Envia por e-mail (SMTP) ou registra para envio WhatsApp
7. Retorna 200 sempre (evitar enumeração de emails)

**Response 200**: `{ "mensagem": "Código enviado. Verifique seu e-mail." }`
**Response 422**: `{ "erro": "Canal WhatsApp indisponível: número não cadastrado" }`

---

## POST /api/auth/verificar-codigo *(AllowAnonymous)*

Valida o código recebido sem ainda trocar a senha.

**Request Body**:
```json
{
  "email": "joao@email.com",
  "codigo": "847291"
}
```

**Behavior**:
1. Localiza `CodigoVerificacao` mais recente e não-usado para o usuário
2. Valida: `Expira > now`, `TentativasErradas < 3`, `Codigo == fornecido`
3. Se código incorreto: incrementa `TentativasErradas`
4. Se código correto: marca `CodigoVerificacao.Usado = true`, retorna token temporário

**Response 200**: `{ "tokenTemporario": "..." }` *(válido por 5 min, para usar no próximo passo)*
**Response 400**: `{ "erro": "Código inválido", "tentativasRestantes": 2 }`
**Response 400**: `{ "erro": "Código expirado" }`
**Response 429**: `{ "erro": "Muitas tentativas. Aguarde 5 minutos." }` *(após 3 erros)*

---

## POST /api/auth/resetar-senha *(AllowAnonymous)*

Define nova senha após verificação do código.

**Request Body**:
```json
{
  "email": "joao@email.com",
  "tokenTemporario": "...",
  "novaSenha": "novasenha456",
  "confirmarSenha": "novasenha456"
}
```

**Behavior**:
1. Valida `tokenTemporario` (JWT de curta duração gerado na etapa anterior)
2. Atualiza `PasswordHash` do usuário
3. Invalida todos os `TokenAtualizacao` do usuário (força novo login em todos os dispositivos)

**Response 200**: `{ "mensagem": "Senha alterada com sucesso. Faça login com a nova senha." }`
**Response 401**: Token temporário inválido ou expirado.

---

## GET /api/auth/perfil *(Authorized)*

**Response 200**:
```json
{
  "id": 1,
  "nomeCompleto": "João Silva",
  "email": "joao@email.com",
  "telefoneWhatsApp": "(11) 99999-9999",
  "fotoUrl": null,
  "criadoEm": "2026-01-15T10:00:00"
}
```

---

## PUT /api/auth/perfil *(Authorized)*

Atualiza nome, telefone e foto.

---

## POST /api/auth/alterar-senha *(Authorized)*

**Request Body**: `{ "senhaAtual": "...", "novaSenha": "...", "confirmarSenha": "..." }`

---

## POST /api/auth/logout *(Authorized)*

Invalida o refresh token atual.
