# Tasks: app-Finance v2.0 — Revamp Completo

**Input**: Design documents from `specs/001-finance-app-v2/`

**Prerequisites**: [plan.md](plan.md) | [spec.md](spec.md) | [data-model.md](data-model.md) | [contracts/](contracts/) | [research.md](research.md)

**Tests**: Não solicitados — validação manual via quickstart.md e Swagger.

**Organization**: Tasks organizadas por User Story para permitir implementação e teste independentes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências incompletas)
- **[Story]**: A qual User Story a task pertence (US1–US13)
- Paths são relativos à raiz do repositório

---

## Phase 1: Setup (Configuração Inicial)

**Purpose**: Remover configurações de desenvolvimento que impedem o funcionamento em outros ambientes.

- [X] T001 Criar arquivo `finance-app-mobile/.env.example` com `EXPO_PUBLIC_API_URL=http://10.0.2.2:7137`
- [X] T002 Criar arquivo `finance-app-mobile/.env` com valor correto para o ambiente local (adicionar ao `.gitignore`)
- [X] T003 Atualizar `finance-app-mobile/src/api/config.ts` — ler `process.env.EXPO_PUBLIC_API_URL` com fallback `http://10.0.2.2:7137`, remover IP hardcoded `192.168.1.5`
- [X] T004 Atualizar `finance-app-mobile/src/api/client.ts` — usar `API_BASE_URL` de `config.ts` em vez de lógica de detecção de plataforma com IP fixo
- [X] T005 Adicionar pacote NuGet `QuestPDF` ao `FinanceApp.API/FinanceApp.API.csproj` para geração de PDF (Phase 4)

**Checkpoint**: API URL configurável sem recompilação, app conecta ao backend correto.

---

## Phase 2: Foundational (Pré-requisito Absoluto)

**Purpose**: Migração GUID→INT, soft delete, audit log e reconciliação de saldo. NENHUMA user story pode ser implementada sem esta fase completa.

**⚠️ CRITICAL**: Completar esta fase inteira antes de iniciar qualquer User Story.

### 2A — Interface e Entidades de Domínio (Backend)

- [X] T006 [P] Criar `FinanceApp.Domain/Interfaces/ISoftDeletable.cs` — interface com `bool IsDeleted` e `DateTime? DeletedAt`
- [X] T007 [P] Atualizar `FinanceApp.Domain/Entities/Usuario.cs` — trocar `Guid Id` por `int Id`
- [X] T008 [P] Atualizar `FinanceApp.Domain/Entities/Conta.cs` — `int Id`, `int UsuarioId`, adicionar `bool IsDeleted`, `DateTime? DeletedAt`, implementar `ISoftDeletable`
- [X] T009 [P] Atualizar `FinanceApp.Domain/Entities/CartaoCredito.cs` — `int Id`, `int UsuarioId`, `int ContaId`, adicionar `bool IsDeleted`, `DateTime? DeletedAt`, implementar `ISoftDeletable`
- [X] T010 [P] Atualizar `FinanceApp.Domain/Entities/Transacao.cs` — `int Id`, todos os FKs como `int?`, adicionar `int? TransferenciaContaId`, `int? FaturaCartaoId`, `DateTime AtualizadaEm`
- [X] T011 [P] Atualizar `FinanceApp.Domain/Entities/Parcelamento.cs` — `int Id`, `int UsuarioId`
- [X] T012 [P] Atualizar `FinanceApp.Domain/Entities/FaturaCartao.cs` — `int Id`, `int CartaoCreditoId`, adicionar `DateTime? DataPagamento`, `DateTime? DataVencimento`
- [X] T013 [P] Atualizar `FinanceApp.Domain/Entities/Categoria.cs` — `int Id`, `int? UsuarioId`
- [X] T014 [P] Atualizar `FinanceApp.Domain/Entities/Orcamento.cs` — `int Id`, `int UsuarioId`, `int CategoriaId`
- [X] T015 [P] Atualizar `FinanceApp.Domain/Entities/MetaEconomia.cs` — `int Id`, `int UsuarioId`, adicionar `bool IsDeleted`, `DateTime? DeletedAt`, implementar `ISoftDeletable`
- [X] T016 [P] Atualizar `FinanceApp.Domain/Entities/LancamentoMeta.cs` — `int Id`, `int MetaEconomiaId`, `int UsuarioId`
- [X] T017 [P] Atualizar `FinanceApp.Domain/Entities/TransferenciaConta.cs` — `int Id`, `int UsuarioId`, `int ContaOrigemId`, `int ContaDestinoId`
- [X] T018 [P] Atualizar `FinanceApp.Domain/Entities/Notificacao.cs` — `int Id`, `int UsuarioId`
- [X] T019 [P] Atualizar `FinanceApp.Domain/Entities/LogAuditoria.cs` — `int Id`, `int UsuarioId` (sem FK para perf), adicionar `string? ValorAnterior`, `string? ValorNovo`
- [X] T020 [P] Atualizar `FinanceApp.Domain/Entities/ConfiguracaoUsuario.cs` — `int UsuarioId` (PK e FK)
- [X] T021 [P] Atualizar `FinanceApp.Domain/Entities/TokenAtualizacao.cs` — `int Id`, `int UsuarioId`, adicionar `DateTime ExpiraEm`
- [X] T022 [P] Atualizar `FinanceApp.Domain/Entities/CodigoVerificacao.cs` — `int Id`, `int UsuarioId`, adicionar `string Canal`, `int TentativasErradas`
- [X] T023 [P] Atualizar `FinanceApp.Domain/Entities/FormaPagamento.cs` — `int Id`
- [X] T024 [P] Atualizar `FinanceApp.Domain/Entities/MensagemWhatsApp.cs` — `int Id` (manter, fora de escopo)
- [X] T025 [P] Atualizar `FinanceApp.Domain/Entities/Anexo.cs` — `int Id` (manter, fora de escopo)
- [X] T026 [P] Atualizar `FinanceApp.Domain/Entities/ApelidoCategoria.cs` — `int Id`, `int CategoriaId`, `int UsuarioId`

### 2B — Infraestrutura e Banco de Dados

- [X] T027 Atualizar `FinanceApp.Infrastructure/Data/FinanceDbContext.cs` — remover todos os `OnDelete(DeleteBehavior.Cascade)` para entidades financeiras; adicionar `OnDelete(DeleteBehavior.Restrict)` para `Transacao→Conta`, `Transacao→Categoria`, `Transacao→CartaoCredito`, `FaturaCartao→CartaoCredito`, `Conta→CartaoCredito`; manter Cascade apenas em `TokenAtualizacao`, `Notificacao`, `ConfiguracaoUsuario`, `CodigoVerificacao`
- [X] T028 Atualizar `FinanceApp.Infrastructure/Data/FinanceDbContext.cs` — adicionar Global Query Filters: `.HasQueryFilter(e => !e.IsDeleted)` para `Conta`, `CartaoCredito`, `MetaEconomia`; adicionar seed da categoria especial `"Transferência"` com `Id=99, Padrao=true`
- [X] T029 Criar `FinanceApp.Infrastructure/Data/Interceptors/AuditInterceptor.cs` — herdar de `SaveChangesInterceptor`, interceptar `EntityState.Added/Modified/Deleted` para entidade `Transacao`, gravar registro em `LogAuditoria` com `ValorAnterior` (JSON) e `ValorNovo` (JSON) antes do SaveChanges
- [X] T030 Registrar `AuditInterceptor` no `FinanceApp.Infrastructure/DI` ou `Program.cs` via `AddDbContext(...).AddInterceptors(new AuditInterceptor())`
- [X] T031 Deletar migration existente `FinanceApp.Infrastructure/Migrations/20260419175534_Inicial.cs` e seu Designer
- [X] T032 Executar `dotnet ef migrations add Inicial_v2` na pasta `FinanceApp.Infrastructure` para gerar nova migration com toda estrutura INT — validar arquivo gerado em `FinanceApp.Infrastructure/Migrations/`
- [X] T033 Executar `dotnet ef database drop --force` + `dotnet ef database update` para recriar banco com novo schema

### 2C — DTOs, Controllers e Configurações

- [X] T034 [P] Atualizar `FinanceApp.Application/DTOs/Auth/AuthDTOs.cs` — todos os IDs como `int`
- [X] T035 [P] Atualizar `FinanceApp.Application/DTOs/Transacoes/TransacaoDTOs.cs` — IDs como `int`, adicionar campos `TransferenciaContaId`, `FaturaCartaoId`, `Status`, `AtualizadaEm`
- [X] T036 [P] Atualizar `FinanceApp.Application/DTOs/Contas/ContaDTOs.cs` — IDs como `int`, adicionar `TemCartaoVinculado`
- [X] T037 [P] Atualizar `FinanceApp.Application/DTOs/CartoesCredito/CartaoCreditoDTOs.cs` — IDs como `int`
- [X] T038 [P] Atualizar `FinanceApp.Application/DTOs/Categorias/CategoriaDTOs.cs` — IDs como `int`, adicionar `Editavel` (bool)
- [X] T039 [P] Atualizar `FinanceApp.Application/DTOs/Orcamentos/OrcamentoDTOs.cs` — IDs como `int`
- [X] T040 [P] Atualizar `FinanceApp.Application/DTOs/Metas/MetaDTOs.cs` — IDs como `int`
- [X] T041 [P] Atualizar `FinanceApp.Application/DTOs/Notificacoes/NotificacaoDTOs.cs` — IDs como `int`
- [X] T042 [P] Atualizar `FinanceApp.Application/DTOs/Dashboard/DashboardDTOs.cs` — IDs como `int`, adicionar `TotalTransferencias`, campo `Periodo { Mes, Ano, Descricao }`
- [X] T043 Atualizar `FinanceApp.API/Controllers/BaseController.cs` — `UsuarioIdAtual` retorna `int` (parse do JWT claim)
- [X] T044 Atualizar `FinanceApp.API/Program.cs` ou `appsettings.json` — configurar JWT `AccessTokenExpiry = 900s (15min)`, `RefreshTokenExpiry = 7 dias`
- [X] T045 Atualizar `finance-app-mobile/src/types/index.ts` — todos os campos de ID de `string` para `number`; adicionar tipos `StatusTransacao`, `TipoTransacao`, `TipoNotificacao`

**Checkpoint**: Backend compila sem erros, migrations rodando, banco criado com INT IDs, app mobile conecta.

---

## Phase 3: US1 + US3 — Integridade Financeira e Ciclo de Status (P1) 🎯 MVP

**Goal**: Saldos sempre corretos + máquina de estados de transação funcional.

**Independent Test**: Criar conta R$1.000 → criar despesa EFETIVADA R$300 → saldo=R$700 → cancelar → saldo=R$1.000 → criar PENDENTE com data passada → listar → aparece como VENCIDA → marcar paga → saldo=R$800.

### Backend — US1 + US3

- [X] T046 [US1] Criar `FinanceApp.Application/Interfaces/IAuditService.cs` — método `RegistrarAsync(int usuarioId, string entidade, int entidadeId, string operacao, object? valorAnterior, object? valorNovo)`
- [X] T047 [US1] Criar `FinanceApp.Application/Services/AuditService.cs` — implementar `IAuditService` gravando em `LogAuditoria`
- [X] T048 [US1] Refatorar `FinanceApp.Application/Services/ContaService.cs` — adicionar método `RecalcularSaldoAsync(int contaId)` que executa `SaldoAtual = SaldoInicial + SUM(receitas EFETIVADAS) - SUM(despesas EFETIVADAS)` usando EF Core dentro de uma transação de banco
- [X] T049 [US1] Refatorar `FinanceApp.Application/Services/TransacaoService.cs` — método `CriarAsync`: após salvar transação EFETIVADA, chamar `ContaService.RecalcularSaldoAsync(contaId)` dentro da mesma `IDbContextTransaction`
- [X] T050 [US3] Refatorar `FinanceApp.Application/Services/TransacaoService.cs` — método `ListarAsync`: antes de retornar, executar `UPDATE Transacoes SET Status='VENCIDA' WHERE UsuarioId=X AND Status='PENDENTE' AND DataTransacao < GETDATE()`
- [X] T051 [US1] Refatorar `FinanceApp.Application/Services/TransacaoService.cs` — adicionar método `CancelarAsync(int id, int usuarioId)`: se `EFETIVADA`, chamar `RecalcularSaldo` após remover; deletar fisicamente a transação; gravar em `LogAuditoria` via `AuditService`
- [X] T052 [US3] Adicionar `FinanceApp.Application/Interfaces/ITransacaoService.cs` — método `AlterarStatusAsync(int id, int usuarioId, StatusTransacao novoStatus)`
- [X] T053 [US3] Implementar `TransacaoService.AlterarStatusAsync` — validar transições permitidas (PENDENTE→EFETIVADA, VENCIDA→EFETIVADA); ao mudar para EFETIVADA, chamar `RecalcularSaldo`; gravar em `LogAuditoria`
- [X] T054 [US1] Atualizar `FinanceApp.API/Controllers/TransacoesController.cs` — endpoint `DELETE /{id}` chama `CancelarAsync`, retorna mensagem com valor estornado e novo saldo
- [X] T055 [US3] Atualizar `FinanceApp.API/Controllers/TransacoesController.cs` — adicionar endpoint `PUT /{id}/status` com body `{ status }`, chama `AlterarStatusAsync`
- [X] T056 [US1] Atualizar `FinanceApp.API/Controllers/ContasController.cs` — endpoint `DELETE /{id}` implementar soft delete: verificar se tem `CartaoCredito` ativo vinculado (retornar 422 se sim), caso contrário setar `IsDeleted=true`

### Frontend — US1 + US3

- [X] T057 [US1] Atualizar `finance-app-mobile/src/screens/Contas/ContasScreen.tsx` — substituir long-press de delete por ação que chama `DELETE /api/contas/{id}`, exibir mensagem de erro se conta tiver cartão ativo, exibir confirmação com aviso de que histórico é preservado
- [X] T058 [US3] Atualizar `finance-app-mobile/src/screens/Transacoes/TransacoesScreen.tsx` — adicionar badge visual por status: `EFETIVADA`=verde, `PENDENTE`=cinza, `VENCIDA`=vermelho
- [X] T059 [US3] Atualizar `finance-app-mobile/src/screens/Transacoes/TransacoesScreen.tsx` — adicionar botão "Marcar como pago" em transações com status `PENDENTE` ou `VENCIDA`; ao tocar, chamar `PUT /api/transacoes/{id}/status` com `{ status: "EFETIVADA" }`
- [X] T060 [US1] Atualizar `finance-app-mobile/src/screens/Transacoes/TransacoesScreen.tsx` — adicionar botão de cancelar (ícone lixeira ou X) por transação; ao confirmar, chamar `DELETE /api/transacoes/{id}`; exibir toast com valor estornado
- [X] T061 [US1] Atualizar `finance-app-mobile/src/screens/Home/HomeScreen.tsx` — garantir que `SaldoAtual` exibido vem do campo atualizado pelo backend (sem cache stale); fazer `refetch` após retornar de `TransacoesScreen`

**Checkpoint**: Saldo sempre correto; transações VENCIDAS aparecem com visual vermelho; "Marcar pago" funciona e afeta saldo; cancelar reverte saldo com toast de confirmação.

---

## Phase 4: US2 — Edição de Transação (P1)

**Goal**: Usuário pode corrigir lançamentos errados via ícone de lápis.

**Independent Test**: Criar despesa R$100 em "Alimentação" → tocar lápis → alterar para R$80 em "Transporte" → salvar → saldo ajustado +R$20, categoria correta.

### Backend — US2

- [X] T062 [US2] Criar `FinanceApp.Application/DTOs/Transacoes/EditarTransacaoDTO.cs` — campos opcionais: `Valor?`, `Descricao?`, `CategoriaId?`, `ContaId?`, `DataTransacao?`, `FormaPagamentoId?`
- [X] T063 [US2] Adicionar método `EditarAsync(int id, int usuarioId, EditarTransacaoDTO dto)` em `FinanceApp.Application/Interfaces/ITransacaoService.cs`
- [X] T064 [US2] Implementar `TransacaoService.EditarAsync` — bloquear se `ParcelamentoId IS NOT NULL` (retornar erro); se `Valor` muda e status=EFETIVADA: recalcular saldo com delta; se `ContaId` muda e status=EFETIVADA: recalcular saldo de ambas as contas; gravar `LogAuditoria` com `ValorAnterior` (snapshot JSON) e `ValorNovo`
- [X] T065 [US2] Atualizar `FinanceApp.API/Controllers/TransacoesController.cs` — adicionar endpoint `PUT /{id}` que chama `EditarAsync`, retorna transação atualizada

### Frontend — US2

- [X] T066 [US2] Atualizar `finance-app-mobile/src/screens/Transacoes/TransacoesScreen.tsx` — adicionar ícone de lápis em cada item da lista; ao tocar, navegar para `CriarTransacaoScreen` passando o objeto da transação como parâmetro de rota
- [X] T067 [US2] Refatorar `finance-app-mobile/src/screens/Transacoes/CriarTransacaoScreen.tsx` — detectar parâmetro `transacaoParaEditar` na rota; se presente: pré-preencher todos os campos com os dados existentes, alterar título para "Editar Transação", alterar botão para "Salvar Alterações"; ao salvar, chamar `PUT /api/transacoes/{id}` em vez de `POST`
- [X] T068 [US2] Atualizar `finance-app-mobile/src/screens/Transacoes/CriarTransacaoScreen.tsx` — se a transação a editar tiver `parcelamentoId`, exibir aviso "Esta transação é parte de um parcelamento e não pode ser editada. Deseja cancelar o parcelamento?" com botão de cancelar o parcelamento inteiro

**Checkpoint**: Ícone de lápis visível; formulário pré-preenchido ao editar; saldo ajustado corretamente; parceladas bloqueadas com opção de cancelar o parcelamento.

---

## Phase 5: US6 — Filtros e Busca em Transações (P2)

**Goal**: Usuário pode filtrar transações por período, categoria, tipo, status e busca textual.

**Independent Test**: Com 10 transações de categorias mistas, aplicar filtro "Alimentação + mês atual" → apenas transações dessa categoria aparecem + total correto no topo.

### Backend — US6

- [X] T069 [US6] Criar `FinanceApp.Application/DTOs/Transacoes/FiltroTransacaoDTO.cs` — campos: `int Pagina`, `int TamanhoPagina`, `int? Mes`, `int? Ano`, `DateTime? DataInicio`, `DateTime? DataFim`, `List<int>? CategoriasIds`, `int? ContaId`, `int? CartaoId`, `string? Tipo`, `string? Status`, `string? Busca`, `bool IncluirTransferencias`
- [X] T070 [US6] Criar `FinanceApp.Application/DTOs/Transacoes/ListaTransacoesDTO.cs` — resultado paginado: `Pagina`, `TamanhoPagina`, `TotalItens`, `TotalPaginas`, `TotalReceitas`, `TotalDespesas`, `SaldoPeriodo`, `List<TransacaoDTO> Itens`
- [X] T071 [US6] Refatorar `FinanceApp.Application/Services/TransacaoService.cs` — método `ListarAsync(FiltroTransacaoDTO filtro)`: aplicar todos os filtros via LINQ/EF; calcular `TotalReceitas` e `TotalDespesas` dos itens filtrados (excluindo transferências); paginar resultado
- [X] T072 [US6] Atualizar `FinanceApp.API/Controllers/TransacoesController.cs` — endpoint `GET /` aceitar todos os query params de `FiltroTransacaoDTO` via `[FromQuery]`

### Frontend — US6

- [X] T073 [US6] Criar componente `FiltroTransacaoModal` em `finance-app-mobile/src/screens/Transacoes/` — modal bottom-sheet com: seletor de período (mês/ano ou data início+fim), seleção múltipla de categorias, toggle tipo (despesa/receita/todos), seleção de status, campo de busca textual, botões "Aplicar" e "Limpar"
- [X] T074 [US6] Atualizar `finance-app-mobile/src/screens/Transacoes/TransacoesScreen.tsx` — adicionar botão de filtro no header; ao tocar, abrir `FiltroTransacaoModal`; exibir badge com número de filtros ativos; ao aplicar filtros, refazer a listagem com os parâmetros selecionados
- [X] T075 [US6] Atualizar `finance-app-mobile/src/screens/Transacoes/TransacoesScreen.tsx` — exibir banner com `TotalReceitas`, `TotalDespesas` e `SaldoPeriodo` quando filtros ativos; ocultar banner quando sem filtros

**Checkpoint**: Filtros aplicados retornam apenas transações correspondentes; total filtrado correto no banner; "Limpar filtros" restaura lista completa.

---

## Phase 6: US10 — Reset de Senha (P2)

**Goal**: Usuário recupera acesso à conta via e-mail (ou WhatsApp) sem depender de suporte.

**Independent Test**: Na tela de login, tocar "Esqueci minha senha" → inserir e-mail → código criado em BD → inserir código correto → definir nova senha → login com nova senha funciona.

### Backend — US10

- [X] T076 [US10] Criar `FinanceApp.Application/DTOs/Auth/ResetSenhaDTOs.cs` — `SolicitarResetDTO { Email, Canal }`, `VerificarCodigoDTO { Email, Codigo }`, `ResetarSenhaDTO { Email, TokenTemporario, NovaSenha, ConfirmarSenha }`
- [X] T077 [US10] Criar `FinanceApp.Application/Interfaces/IEmailService.cs` — método `EnviarCodigoResetAsync(string email, string codigo)`
- [X] T078 [US10] Criar `FinanceApp.Application/Services/EmailService.cs` — implementar envio via SMTP (usar `System.Net.Mail.SmtpClient` ou `MailKit`); ler configurações de `appsettings.json` (host, porta, usuário, senha)
- [X] T079 [US10] Adicionar configurações SMTP em `FinanceApp.API/appsettings.json` e `appsettings.Development.json` (valores de teste)
- [X] T080 [US10] Adicionar métodos em `FinanceApp.Application/Interfaces/IAuthService.cs` — `SolicitarResetAsync`, `VerificarCodigoAsync`, `ResetarSenhaAsync`
- [X] T081 [US10] Implementar `AuthService.SolicitarResetAsync` — localizar usuário pelo email (responder 200 mesmo se não existir para não revelar emails); se canal `WHATSAPP` e sem telefone: retornar 422; invalidar códigos anteriores; gerar código 6 dígitos; salvar `CodigoVerificacao` com `Canal`, `Expira=now+15min`; chamar `EmailService.EnviarCodigoResetAsync`
- [X] T082 [US10] Implementar `AuthService.VerificarCodigoAsync` — localizar `CodigoVerificacao` mais recente não-usado; validar expiração, tentativas (`TentativasErradas < 3`) e código; se errado: incrementar `TentativasErradas`; se correto: marcar `Usado=true`; gerar JWT temporário (`sub=email`, `exp=now+5min`, `purpose=reset`)
- [X] T083 [US10] Implementar `AuthService.ResetarSenhaAsync` — validar `TokenTemporario` (JWT especial com purpose=reset); atualizar `PasswordHash`; invalidar todos os `TokenAtualizacao` do usuário
- [X] T084 [US10] Adicionar endpoints em `FinanceApp.API/Controllers/AuthController.cs` — `POST /solicitar-reset-senha`, `POST /verificar-codigo`, `POST /resetar-senha` conforme contrato `contracts/auth.md`

### Frontend — US10

- [X] T085 [US10] Criar `finance-app-mobile/src/screens/Auth/EsqueciSenhaScreen.tsx` — campo de e-mail, seletor de canal (E-mail / WhatsApp — WhatsApp aparece apenas se usuário tem telefone, mas como é pré-login, mostrar ambos e tratar erro do backend), botão "Enviar código", tratamento de loading e erros
- [X] T086 [US10] Criar `finance-app-mobile/src/screens/Auth/VerificarCodigoScreen.tsx` — campo de 6 dígitos (usar `TextInput` com `keyboardType="numeric"` e `maxLength=6`), botão "Verificar", exibir tentativas restantes em caso de erro, link "Reenviar código" (chama novamente o endpoint anterior), contador regressivo de expiração (15 min)
- [X] T087 [US10] Criar `finance-app-mobile/src/screens/Auth/NovaSenhaScreen.tsx` — campos nova senha e confirmação, botão "Redefinir senha", ao sucesso navegar para `LoginScreen` com mensagem de sucesso
- [X] T088 [US10] Atualizar `finance-app-mobile/src/screens/Auth/LoginScreen.tsx` — adicionar link "Esqueci minha senha" que navega para `EsqueciSenhaScreen`
- [X] T089 [US10] Atualizar `finance-app-mobile/src/navigation/AppNavigator.tsx` — adicionar rotas `EsqueciSenha`, `VerificarCodigo`, `NovaSenha` ao `AuthStack`

**Checkpoint**: Fluxo completo do reset funciona end-to-end; código enviado por e-mail (verificável via log do SMTP em dev); nova senha permite login; tentativas inválidas bloqueiam após 3 erros.

---

## Phase 7: US4 — Transferências entre Contas (P2)

**Goal**: Mover dinheiro entre contas sem afetar o total consolidado nem os relatórios de receita/despesa.

**Independent Test**: Conta A R$1.000 + Conta B R$200 = total R$1.200 → transferir R$300 de A para B → A=R$700, B=R$500, total=R$1.200, dashboard não muda receita/despesa.

### Backend — US4

- [X] T090 [US4] Criar `FinanceApp.Application/DTOs/Transferencias/TransferenciaDTO.cs` — `CriarTransferenciaDTO`, `TransferenciaResponseDTO` conforme `contracts/transferencias.md`
- [X] T091 [US4] Criar `FinanceApp.Application/Interfaces/ITransferenciaService.cs` — métodos `CriarAsync`, `ListarAsync`, `CancelarAsync`
- [X] T092 [US4] Criar `FinanceApp.Application/Services/TransferenciaService.cs` — `CriarAsync`: validar `ContaOrigemId != ContaDestinoId`, ambas as contas existem e pertencem ao usuário; dentro de `IDbContextTransaction`: criar `TransferenciaConta`, criar `Transacao` DESPESA na origem (CategoriaId=99 "Transferência", EFETIVADA), criar `Transacao` RECEITA no destino (CategoriaId=99, EFETIVADA), recalcular saldo de ambas as contas; commit; em caso de exceção: rollback
- [X] T093 [US4] Implementar `TransferenciaService.CancelarAsync` — localizar par de transações pelo `TransferenciaContaId`; dentro de transação de BD: recalcular saldo de ambas as contas (reversão), deletar fisicamente ambas as transações, deletar `TransferenciaConta`
- [X] T094 [US4] Criar `FinanceApp.API/Controllers/TransferenciasController.cs` — `POST /`, `GET /`, `DELETE /{id}` conforme contrato
- [X] T095 [US4] Atualizar `FinanceApp.Application/Services/DashboardService.cs` — excluir transações onde `TransferenciaContaId IS NOT NULL` dos cálculos de `TotalReceitas` e `TotalDespesas`; adicionar campo `TotalTransferencias` no response

### Frontend — US4

- [X] T096 [US4] Criar `finance-app-mobile/src/screens/Transferencias/TransferenciaScreen.tsx` — seletor de conta origem, seletor de conta destino, campo de valor, campo de data, campo de descrição (opcional), botão "Realizar Transferência"; validar que origem != destino; ao confirmar, chamar `POST /api/transferencias`; exibir resumo com novos saldos após sucesso
- [X] T097 [US4] Atualizar `finance-app-mobile/src/navigation/AppNavigator.tsx` — adicionar rota `Transferencia` no stack principal (acessível via botão dedicado ou via FAB na tela de contas)
- [X] T098 [US4] Atualizar `finance-app-mobile/src/screens/Contas/ContasScreen.tsx` — adicionar botão "Transferir" (ou FAB secundário) que navega para `TransferenciaScreen`
- [X] T099 [US4] Atualizar `finance-app-mobile/src/screens/Transacoes/TransacoesScreen.tsx` — quando `tipo=TRANSFERENCIA` (identificado por `transferencia != null`), exibir ícone especial (duas setas) e label "Transferência para/de [nome conta]"

**Checkpoint**: Transferência criada → saldos corretos em ambas as contas → dashboard inalterado → transações de transferência com ícone diferenciado na lista.

---

## Phase 8: US5 — Cartão de Crédito e Faturas (P2)

**Goal**: Controle completo do ciclo de cartão: compra parcelada → fatura por mês → pagamento debita conta.

**Independent Test**: Criar cartão limite R$5.000 → compra parcelada R$1.200 em 3x → limite=R$3.800 → verificar 3 faturas → pagar fatura mês 1 → conta debitada R$400, limite=R$4.200.

### Backend — US5

- [X] T100 [US5] Criar `FinanceApp.Application/DTOs/CartoesCredito/CartaoCreditoDTOs.cs` (completar) — `CriarCartaoDTO`, `CartaoResponseDTO`, `FaturaResponseDTO`, `DetalhesFaturaResponseDTO`, `PagarFaturaDTO`
- [X] T101 [US5] Criar `FinanceApp.Application/Interfaces/IFaturaCartaoService.cs` — métodos `ObterOuCriarFaturaAsync(int cartaoId, int mes, int ano)`, `ListarFaturasAsync(int cartaoId)`, `DetalharFaturaAsync(int faturaId)`, `PagarFaturaAsync(int faturaId, int usuarioId, DateTime dataPagamento)`, `AtualizarStatusFaturasVencidasAsync(int cartaoId)`
- [X] T102 [US5] Criar `FinanceApp.Application/Services/FaturaCartaoService.cs` — `ObterOuCriarFaturaAsync`: calcular mês de alocação (se `DataTransacao.Day >= DiaFechamento`: mês seguinte); buscar `FaturaCartao` existente para `(cartaoId, mes, ano)`; se não existir: criar com `Status=ABERTA`
- [X] T103 [US5] Implementar `FaturaCartaoService.PagarFaturaAsync` — verificar `Status != PAGA` (idempotência); dentro de transação de BD: criar `Transacao` DESPESA na conta vinculada ao cartão (categoria "Pagamento Fatura"), atualizar `FaturaCartao.Status=PAGA`, `DataPagamento=agora`, recalcular `LimiteDisponivel` do cartão (`+= FaturaCartao.ValorTotal`), recalcular `SaldoAtual` da conta
- [X] T104 [US5] Implementar `FaturaCartaoService.AtualizarStatusFaturasVencidasAsync` — verificar se hoje >= DiaFechamento de faturas ABERTAS → atualizar para FECHADA + gerar notificação `FATURA_FECHADA`
- [X] T105 [US5] Refatorar `FinanceApp.Application/Services/CartaoCreditoService.cs` — ao criar cartão: `LimiteDisponivel = LimiteTotal`; ao deletar: verificar faturas ABERTA/FECHADA (422 se existir)
- [X] T106 [US5] Refatorar `FinanceApp.Application/Services/TransacaoService.cs` — ao criar transação de cartão com `parcelas > 1`: calcular `FaturaCartaoId` para cada parcela via `FaturaCartaoService.ObterOuCriarFaturaAsync`; reduzir `CartaoCredito.LimiteDisponivel -= ValorTotal` imediatamente
- [X] T107 [US5] Criar `FinanceApp.API/Controllers/FaturasController.cs` — endpoints conforme `contracts/cartoes.md`: `GET /api/cartoes/{id}/faturas`, `GET /api/cartoes/{cartaoId}/faturas/{faturaId}`, `POST /api/cartoes/{cartaoId}/faturas/{faturaId}/pagar`
- [X] T108 [US5] Atualizar `FinanceApp.API/Controllers/CartoesController.cs` — completar CRUD; `DELETE /{id}` com validação de fatura em aberto

### Frontend — US5

- [X] T109 [US5] Criar `finance-app-mobile/src/screens/Cartoes/CartoesScreen.tsx` — lista de cartões com: nome, bandeira, limite total, limite disponível (barra de progresso), fatura atual (valor + status + dias para vencer); FAB para adicionar cartão; toque em cartão navega para lista de faturas
- [X] T110 [US5] Criar `finance-app-mobile/src/screens/Cartoes/CriarCartaoScreen.tsx` — formulário: nome, bandeira (dropdown: Visa/Mastercard/Elo/Hipercard), limite total, dia de fechamento (seletor 1-28), dia de vencimento (seletor 1-28), conta vinculada (seletor de contas do usuário); ao salvar, chamar `POST /api/cartoes`
- [X] T111 [US5] Criar `finance-app-mobile/src/screens/Cartoes/FaturaDetalheScreen.tsx` — lista de faturas do cartão em cards (mês/ano, valor, status, data vencimento); ao tocar em fatura: expandir mostrando lista de transações da fatura; botão "Pagar Fatura" para faturas ABERTA ou FECHADA; confirmação com valor antes de pagar
- [X] T112 [US5] Atualizar `finance-app-mobile/src/navigation/AppNavigator.tsx` — adicionar tab "Cartões" na TabNavigator ou como rota no stack principal; adicionar rotas `CriarCartao` e `FaturaDetalhe`
- [X] T113 [US5] Atualizar `finance-app-mobile/src/screens/Transacoes/CriarTransacaoScreen.tsx` — ao selecionar cartão de crédito como forma de pagamento: mostrar seletor de cartão; exibir `LimiteDisponivel` do cartão selecionado; campos de parcela se aplicável

**Checkpoint**: Compra parcelada no cartão → limite reduz → faturas criadas por mês → pagamento de fatura → conta debitada + limite restaurado → pagamento duplicado bloqueado.

---

## Phase 9: US7 — Dashboard com Navegação Mensal (P2)

**Goal**: Usuário analisa qualquer um dos últimos 12 meses no dashboard.

**Independent Test**: Com transações em meses diferentes, navegar para mês M-2 → apenas dados de M-2 aparecem → transferências excluídas dos totais.

### Backend — US7

- [X] T114 [US7] Refatorar `FinanceApp.Application/Services/DashboardService.cs` — aceitar parâmetros `int mes` e `int ano` (default: mês/ano atual); filtrar todas as queries por `DataTransacao.Month == mes && DataTransacao.Year == ano`; excluir transferências dos totais; historico6Meses usa os 6 meses antes do mês selecionado
- [X] T115 [US7] Criar `FinanceApp.Application/DTOs/Dashboard/DashboardRequestDTO.cs` — `int? Mes`, `int? Ano`
- [X] T116 [US7] Atualizar `FinanceApp.API/Controllers/DashboardController.cs` — endpoint `GET /` aceitar `[FromQuery] DashboardRequestDTO`, validar que `Ano >= 2020` e `Mes entre 1-12`, validar que período não é futuro

### Frontend — US7

- [X] T117 [US7] Atualizar `finance-app-mobile/src/screens/Home/HomeScreen.tsx` — adicionar estado `{ mes: number, ano: number }` inicializado com mês/ano atual; adicionar seletor de mês no topo: `◄ [Mês Ano] ►`; seta esquerda: decrementa mês (min: 12 meses atrás); seta direita: incrementa mês (max: mês atual); ao mudar, refazer fetch do dashboard com `?mes=X&ano=Y`
- [X] T118 [US7] Atualizar `finance-app-mobile/src/screens/Home/HomeScreen.tsx` — garantir que transferências não aparecem nos totais (backend já exclui, verificar que o frontend não soma manualmente); adicionar label "(sem transferências)" no card de resumo para deixar claro

**Checkpoint**: Navegar 3 meses para trás → dados corretos de 3 meses atrás → transferências não aparecem nos totais → seta esquerda desabilitada em M-12.

---

## Phase 10: US11 — Notificações In-App (P2)

**Goal**: Central de notificações com badge e geração automática por eventos financeiros.

**Independent Test**: Criar orçamento R$100, lançar R$85 → notificação criada no BD → `GET /api/notificacoes/nao-lidas/count` retorna 1 → badge visível no app → tocar notificação → marcar lida → count = 0.

### Backend — US11

- [X] T119 [US11] Criar `FinanceApp.Application/DTOs/Notificacoes/NotificacaoDTOs.cs` (completar) — `NotificacaoDTO`, `ListaNotificacoesDTO`, `CountNaoLidasDTO`
- [X] T120 [US11] Criar `FinanceApp.Application/Interfaces/INotificacaoService.cs` — `GerarAsync(int usuarioId, string titulo, string mensagem, TipoNotificacao tipo)`, `ListarAsync(int usuarioId, bool apenasNaoLidas, int pagina)`, `CountNaoLidasAsync(int usuarioId)`, `MarcarLidaAsync(int id, int usuarioId)`, `MarcarTodasLidasAsync(int usuarioId)`
- [X] T121 [US11] Criar `FinanceApp.Application/Services/NotificacaoService.cs` — implementar todos os métodos da interface
- [X] T122 [US11] Atualizar `FinanceApp.Application/Services/TransacaoService.cs` — ao criar/editar transação EFETIVADA do tipo DESPESA: verificar orçamentos da categoria no mês → se `ValorUtilizado >= AlertaEm%`: chamar `NotificacaoService.GerarAsync` com tipo `ALERTA_ORCAMENTO_80`; se `ValorUtilizado >= 100%`: gerar `ALERTA_ORCAMENTO_100` (apenas uma vez por mês — verificar se já gerou)
- [X] T123 [US11] Atualizar `FinanceApp.Application/Services/MetaEconomiaService.cs` — ao registrar aporte: se `ValorAtual >= ValorAlvo`: chamar `NotificacaoService.GerarAsync` com tipo `META_ATINGIDA`
- [X] T124 [US11] Atualizar `FinanceApp.Application/Services/FaturaCartaoService.cs` — ao mudar fatura para FECHADA: chamar `NotificacaoService.GerarAsync` com tipo `FATURA_FECHADA`
- [X] T125 [US11] Atualizar `FinanceApp.API/Controllers/NotificacoesController.cs` — implementar endpoints conforme `contracts/notificacoes.md`: `GET /`, `GET /nao-lidas/count`, `PUT /{id}/marcar-lida`, `PUT /marcar-todas-lidas`, `DELETE /{id}`

### Frontend — US11

- [X] T126 [US11] Criar `finance-app-mobile/src/screens/Notificacoes/NotificacoesScreen.tsx` — lista de notificações (mais recentes primeiro) com: ícone por tipo, título, mensagem, data/hora, indicador visual de não-lida (ponto azul ou fundo destacado); tocar → chamar `PUT /{id}/marcar-lida`; botão "Marcar todas como lidas"; swipe para deletar (opcional)
- [X] T127 [US11] Atualizar `finance-app-mobile/src/navigation/AppNavigator.tsx` — adicionar rota `Notificacoes` no stack principal; adicionar ícone de sino no header do HomeScreen ou TabBar
- [X] T128 [US11] Atualizar `finance-app-mobile/src/screens/Home/HomeScreen.tsx` — adicionar ícone de sino no header com badge numérico (chamar `GET /api/notificacoes/nao-lidas/count` no `useEffect` e após cada navegação); ao tocar, navegar para `NotificacoesScreen`

**Checkpoint**: Lançar despesa que supera 80% do orçamento → badge aparece no sino → tela de notificações mostra alerta → marcar lida → badge remove.

---

## Phase 11: US8 — Orçamentos CRUD (P2)

**Goal**: Usuário cria, edita e exclui orçamentos mensais por categoria.

**Independent Test**: Criar orçamento R$500 para Alimentação em junho → aparece no dashboard com 0% → lançar despesa R$250 em Alimentação → progresso 50%.

### Backend — US8

- [X] T129 [US8] Criar `FinanceApp.Application/DTOs/Orcamentos/OrcamentoDTOs.cs` (completar) — `CriarOrcamentoDTO`, `EditarOrcamentoDTO`, `OrcamentoResponseDTO` (com `ValorUtilizado`, `PercentualUtilizado`, `Status` calculados)
- [X] T130 [US8] Completar `FinanceApp.Application/Interfaces/IOrcamentoService.cs` — métodos `CriarAsync`, `EditarAsync`, `DeletarAsync`, `ListarAsync(int usuarioId, int mes, int ano)`
- [X] T131 [US8] Completar `FinanceApp.Application/Services/OrcamentoService.cs` — `ListarAsync`: calcular `ValorUtilizado` via `SUM` de despesas EFETIVADAS da categoria no período; derivar `Status` (OK/ALERTA/EXCEDIDO); `CriarAsync`: verificar unicidade `(usuarioId, categoriaId, mes, ano)` (422 se duplicado)
- [X] T132 [US8] Atualizar `FinanceApp.API/Controllers/OrcamentosController.cs` — implementar CRUD completo conforme `contracts/orcamentos.md`

### Frontend — US8

- [X] T133 [US8] Criar `finance-app-mobile/src/screens/Orcamentos/OrcamentosScreen.tsx` — lista de orçamentos do mês atual com barra de progresso colorida (verde/amarelo/vermelho por status); FAB para criar novo orçamento; toque longo para editar ou deletar; seletor de mês/ano para visualizar outros meses
- [X] T134 [US8] Criar formulário de orçamento inline ou como modal em `OrcamentosScreen` — seletor de categoria (apenas DESPESA), campo de valor limite, percentual de alerta (default 80%), mês e ano; validar que não existe orçamento duplicado (exibir erro do backend)
- [X] T135 [US8] Atualizar `finance-app-mobile/src/navigation/AppNavigator.tsx` — adicionar rota `Orcamentos` acessível via tab ou menu

**Checkpoint**: Criar orçamento → aparece na listagem → lançar despesa → progresso atualiza → duplicata bloqueada com mensagem.

---

## Phase 12: US9 — Metas de Economia (P2)

**Goal**: Usuário cria metas financeiras e registra aportes progressivos.

**Independent Test**: Criar meta "Viagem" R$10.000 → aparece com 0% → registrar aporte R$2.000 → progresso 20% → registrar R$8.000 → progresso 100% → notificação gerada.

### Backend — US9

- [X] T136 [US9] Criar `FinanceApp.Application/DTOs/Metas/MetaDTOs.cs` (completar) — `CriarMetaDTO`, `EditarMetaDTO`, `AporteMetaDTO`, `MetaResponseDTO` (com `Percentual`, `Expirada`, `Atingida`, lista de `LancamentoMeta`)
- [X] T137 [US9] Completar `FinanceApp.Application/Interfaces/IMetaEconomiaService.cs` — `CriarAsync`, `EditarAsync`, `DeletarAsync` (soft), `ListarAsync`, `AdicionarAporteAsync`, `RemoverAporteAsync`
- [X] T138 [US9] Completar `FinanceApp.Application/Services/MetaEconomiaService.cs` — `AdicionarAporteAsync`: criar `LancamentoMeta`, incrementar `MetaEconomia.ValorAtual`, se atingida gerar notificação via `NotificacaoService`; `DeletarAsync`: soft delete
- [X] T139 [US9] Atualizar `FinanceApp.API/Controllers/MetasController.cs` — implementar CRUD + aportes conforme `contracts/metas.md`

### Frontend — US9

- [X] T140 [US9] Criar `finance-app-mobile/src/screens/Metas/MetasScreen.tsx` — lista de metas com: barra de progresso, valor atual / valor alvo, percentual, prazo restante (ou "Expirada" em vermelho); FAB para criar nova meta; toque para ver detalhes e registrar aporte
- [X] T141 [US9] Criar modal de aporte em `MetasScreen` — campo de valor e descrição opcional; ao confirmar, chamar `POST /api/metas/{id}/aportes`; atualizar progresso na tela
- [X] T142 [US9] Criar formulário de nova/editar meta em `MetasScreen` — campos: título, descrição (opcional), valor alvo, prazo (date picker opcional); ao salvar, chamar `POST` ou `PUT /api/metas`
- [X] T143 [US9] Atualizar `finance-app-mobile/src/navigation/AppNavigator.tsx` — adicionar rota `Metas`

**Checkpoint**: Criar meta → registrar aportes → progresso atualiza → ao atingir 100% notificação gerada → meta expirada destacada em vermelho.

---

## Phase 13: US12 — Dark Mode Completo (P3)

**Goal**: Todas as telas do app suportam tema escuro, persistido entre sessões.

**Independent Test**: Ativar dark mode → fechar app → reabrir → tema escuro mantido → navegar por todas as telas → nenhuma com fundo branco.

- [X] T144 [US12] Criar `finance-app-mobile/src/contexts/ThemeContext.tsx` — `ThemeProvider` com `React.createContext`; estado `'light' | 'dark'`; função `toggleTheme()`; persistir em `AsyncStorage` com chave `'app_theme'`; ler na inicialização
- [X] T145 [US12] Atualizar `finance-app-mobile/src/theme/colors.ts` — adicionar paleta dark: `dark.background='#121212'`, `dark.surface='#1E1E1E'`, `dark.textPrimary='#FFFFFF'`, `dark.textSecondary='#AAAAAA'`, `dark.primary='#7C73FF'`, `dark.success='#2ECC71'`, `dark.danger='#E74C3C'`, `dark.warning='#F39C12'`
- [X] T146 [US12] Criar hook `finance-app-mobile/src/theme/useTheme.ts` — retorna `{ colors, isDark, toggleTheme }` a partir do `ThemeContext`
- [X] T147 [US12] Envolver `finance-app-mobile/App.tsx` com `ThemeProvider`
- [X] T148 [P] [US12] Atualizar `finance-app-mobile/src/screens/Home/HomeScreen.tsx` — substituir cores hardcoded por `colors.background`, `colors.surface`, `colors.textPrimary` etc. via `useTheme()`
- [X] T149 [P] [US12] Atualizar `finance-app-mobile/src/screens/Transacoes/TransacoesScreen.tsx` — mesma refatoração de cores
- [X] T150 [P] [US12] Atualizar `finance-app-mobile/src/screens/Transacoes/CriarTransacaoScreen.tsx` — mesma refatoração de cores
- [X] T151 [P] [US12] Atualizar `finance-app-mobile/src/screens/Contas/ContasScreen.tsx` — mesma refatoração de cores
- [X] T152 [P] [US12] Atualizar `finance-app-mobile/src/screens/Cartoes/CartoesScreen.tsx` — mesma refatoração de cores
- [X] T153 [P] [US12] Atualizar `finance-app-mobile/src/screens/Cartoes/CriarCartaoScreen.tsx` — mesma refatoração de cores
- [X] T154 [P] [US12] Atualizar `finance-app-mobile/src/screens/Cartoes/FaturaDetalheScreen.tsx` — mesma refatoração de cores
- [X] T155 [P] [US12] Atualizar `finance-app-mobile/src/screens/Orcamentos/OrcamentosScreen.tsx` — mesma refatoração de cores
- [X] T156 [P] [US12] Atualizar `finance-app-mobile/src/screens/Metas/MetasScreen.tsx` — mesma refatoração de cores
- [X] T157 [P] [US12] Atualizar `finance-app-mobile/src/screens/Notificacoes/NotificacoesScreen.tsx` — mesma refatoração de cores
- [X] T158 [P] [US12] Atualizar `finance-app-mobile/src/screens/Config/ConfigScreen.tsx` — dark mode toggle agora chama `toggleTheme()` do context em vez de não fazer nada; mesma refatoração de cores
- [X] T159 [P] [US12] Atualizar `finance-app-mobile/src/screens/Auth/LoginScreen.tsx` — mesma refatoração de cores
- [X] T160 [P] [US12] Atualizar `finance-app-mobile/src/screens/Auth/RegistroScreen.tsx` — mesma refatoração de cores
- [X] T161 [P] [US12] Atualizar `finance-app-mobile/src/screens/Auth/EsqueciSenhaScreen.tsx`, `VerificarCodigoScreen.tsx`, `NovaSenhaScreen.tsx` — mesma refatoração de cores
- [X] T162 [P] [US12] Atualizar `finance-app-mobile/src/screens/Transferencias/TransferenciaScreen.tsx` — mesma refatoração de cores
- [X] T163 [US12] Atualizar `finance-app-mobile/src/navigation/AppNavigator.tsx` — aplicar `backgroundColor` da TabBar e headers via `useTheme()`

**Checkpoint**: Toggle em ConfigScreen → todas as telas mudam imediatamente → fechar/abrir app → tema persiste → nenhuma tela com contraste inválido no dark mode.

---

## Phase 14: US13 — Exportação em PDF (P3)

**Goal**: Usuário gera e compartilha extrato financeiro em PDF por período de até 3 meses.

**Independent Test**: Com transações no mês, chamar `POST /api/exportacao/pdf` com período válido → receber arquivo PDF com lista de transações, totais e saldo.

### Backend — US13

- [X] T164 [US13] Criar `FinanceApp.Application/DTOs/Exportacao/ExportacaoDTO.cs` — `ExportacaoPdfRequestDTO { DateTime DataInicio, DateTime DataFim }`, `ExportacaoPdfResponseDTO { byte[] Bytes, string NomeArquivo }`
- [X] T165 [US13] Criar `FinanceApp.Application/Interfaces/IExportacaoService.cs` — `GerarExtratoPdfAsync(int usuarioId, DateTime dataInicio, DateTime dataFim): Task<ExportacaoPdfResponseDTO>`
- [X] T166 [US13] Criar `FinanceApp.Application/Services/ExportacaoService.cs` — validar que `dataFim - dataInicio <= 92 dias` (3 meses); buscar transações do período; usando QuestPDF: gerar documento com: cabeçalho (nome usuário, período), tabela de transações (data, descrição, categoria, valor), rodapé (total receitas, despesas, saldo); retornar `byte[]` com o PDF
- [X] T167 [US13] Criar `FinanceApp.API/Controllers/ExportacaoController.cs` — `POST /api/exportacao/pdf`: chamar serviço, retornar `File(bytes, "application/pdf", nomeArquivo)` com `Content-Disposition: attachment`

### Frontend — US13

- [X] T168 [US13] Criar componente/modal `ExportacaoModal` em `finance-app-mobile/src/screens/` — seletor de data início e data fim, botão "Exportar PDF"; validar que período ≤ 3 meses; ao confirmar, chamar `POST /api/exportacao/pdf`; ao receber response bytes, usar `expo-sharing` ou `expo-file-system` para salvar e compartilhar o PDF
- [X] T169 [US13] Adicionar entrada de "Exportar PDF" em `finance-app-mobile/src/screens/Config/ConfigScreen.tsx` ou no header da tela de transações

---

## Phase 15: Polish — Categorias Customizadas e Melhorias Finais

**Purpose**: Completar funcionalidades de menor prioridade e garantir qualidade geral.

- [X] T170 [P] Criar `FinanceApp.Application/DTOs/Categorias/CategoriaDTOs.cs` (completar) — `CriarCategoriaDTO { Nome, Icone, Cor, Tipo }`, `EditarCategoriaDTO`, `CategoriaResponseDTO (com Editavel)`
- [X] T171 [P] Implementar CRUD de categorias customizadas em `FinanceApp.Application/Services/CategoriaService.cs` — `CriarAsync`: UsuarioId = autenticado; `DeletarAsync`: bloquear se `Padrao=true` (400) ou transações vinculadas (422 com count); `EditarAsync`: bloquear se `Padrao=true`
- [X] T172 Atualizar `FinanceApp.API/Controllers/CategoriasController.cs` — adicionar endpoints `POST /`, `PUT /{id}`, `DELETE /{id}` conforme `contracts/categorias.md`
- [X] T173 Criar `finance-app-mobile/src/screens/Categorias/CategoriasScreen.tsx` — lista separada por tipo (DESPESA / RECEITA); categorias padrão exibidas sem ações; categorias customizadas com botão de editar e deletar; FAB para criar nova; formulário com nome, ícone (seletor de ícones Ionicons), cor (color picker), tipo
- [X] T174 Adicionar rota `Categorias` em `finance-app-mobile/src/navigation/AppNavigator.tsx` — acessível via `ConfigScreen` (nova entrada "Gerenciar Categorias")
- [X] T175 Atualizar `finance-app-mobile/src/screens/Auth/LoginScreen.tsx` — adicionar link para `EsqueciSenhaScreen` (se não feito na Phase 6)
- [X] T176 [P] Adicionar `expo-file-system` e `expo-sharing` em `finance-app-mobile/package.json` para exportação PDF (se não instalados)
- [X] T177 [P] Adicionar `MailKit` NuGet em `FinanceApp.API/FinanceApp.API.csproj` para envio de e-mail (alternativa ao SmtpClient nativo)
- [X] T178 Executar checklist de aceitação completo de `specs/001-finance-app-v2/quickstart.md` — verificar todos os 13 cenários
- [X] T179 Revisar todos os endpoints no Swagger — verificar que IDs são `int`, respostas de erro têm mensagens claras em português
- [X] T180 Revisar `LogAuditoria` no banco — confirmar que operações de transação (criar, editar, cancelar, mudar status) geram registros com `ValorAnterior` e `ValorNovo` preenchidos

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Sem dependências — iniciar imediatamente
- **Phase 2 (Foundational)**: Depende de Phase 1 — **BLOQUEIA todas as User Stories**
- **Phases 3-4 (US1/US2/US3 — P1)**: Dependem de Phase 2 — iniciar assim que Foundational concluir
- **Phases 5-12 (P2 stories)**: Dependem de Phase 2 — podem iniciar em paralelo após Foundational
- **Phases 13-14 (P3 stories)**: Dependem das fases anteriores da mesma área (dark mode: todas as telas existentes; PDF: backend)
- **Phase 15 (Polish)**: Depende de todas as fases anteriores

### User Story Dependencies

- **US1 + US3 (Phase 3)**: Dependem apenas de Phase 2 (Foundational)
- **US2 (Phase 4)**: Depende de US1 (requer `ContaService.RecalcularSaldo`)
- **US6 (Phase 5)**: Independente — apenas backend de listagem
- **US10 (Phase 6)**: Independente — auth flow separado
- **US4 (Phase 7)**: Depende de US1 (saldo recalculado nas contas)
- **US5 (Phase 8)**: Depende de US1 (limite do cartão afeta saldo) e US3 (status de parcelas)
- **US7 (Phase 9)**: Independente — apenas filtro temporal no dashboard
- **US11 (Phase 10)**: Depende de US8 e US9 (geração de notificações por orçamento/meta)
- **US8 (Phase 11)**: Depende de US1 (cálculo usa transações EFETIVADAS)
- **US9 (Phase 12)**: Independente (aportes não afetam saldo de contas)
- **US12 (Phase 13)**: Depende de todas as telas existentes (refatoração de cores)
- **US13 (Phase 14)**: Depende de US1 (transações precisam ter saldo consistente para o relatório)

### Parallel Opportunities (dentro de cada Phase)

```
Phase 2 — Entities (T007-T026): Todos em paralelo (arquivos diferentes)
Phase 2 — DTOs (T034-T042): Todos em paralelo (arquivos diferentes)
Phase 3 — Backend (T046-T056): T046+T047 em paralelo; T048+T049+T050 após T046; sequencial na ordem listada
Phase 8 — Frontend Cartões (T109+T110+T111): Em paralelo (telas diferentes)
Phase 13 — Dark Mode (T148-T162): Todos em paralelo (telas diferentes)
```

---

## Parallel Example: Phase 2 (Foundational)

```
# Todas as entidades em paralelo (T007-T026):
Task: "Atualizar Usuario.cs — int Id"
Task: "Atualizar Conta.cs — int Id + IsDeleted"
Task: "Atualizar CartaoCredito.cs — int Id + IsDeleted"
Task: "Atualizar Transacao.cs — int Id + TransferenciaContaId"
[... mais 18 entidades em paralelo ...]

# Depois que entidades estiverem prontas (T027-T033 — sequencial):
Task: "Atualizar DbContext — global query filters + cascades"
Task: "Criar AuditInterceptor"
Task: "Gerar nova migration"
Task: "Executar database update"
```

---

## Implementation Strategy

### MVP (Phases 1-4: Setup + Foundational + US1 + US2 + US3)

1. Completar Phase 1 (Setup) — 30 min
2. Completar Phase 2 (Foundational) — estimativa 4-6h (migração + interceptor + types)
3. Completar Phase 3 (US1 + US3: saldo + status) — estimativa 3-4h
4. Completar Phase 4 (US2: edição) — estimativa 2-3h
5. **VALIDAR MVP**: Saldo sempre correto, transações editáveis, status machine funcional
6. **PONTO DE ENTREGA**: App funcional com núcleo financeiro confiável

### Incremental Delivery (Phases 5-12)

- Cada phase é um incremento independente entregável
- Ordem sugerida (por impacto/dependência): US6 → US4 → US5 → US10 → US7 → US8 → US9 → US11
- Validar cada story individualmente via `quickstart.md` antes de avançar

### Single Developer Strategy

1. Phases 1-2: Setup + Foundational (mais crítico, fazer primeiro)
2. Phases 3-4: P1 stories (US1/US2/US3)
3. Phase 5: US6 Filtros (quick win — só backend refactor)
4. Phase 6: US10 Reset Senha (independente)
5. Phase 7: US4 Transferências
6. Phase 8: US5 Cartão de Crédito (mais complexo)
7. Phases 9-12: Demais P2 stories
8. Phases 13-14: P3 stories (dark mode e PDF)
9. Phase 15: Polish

---

## Notes

- `[P]` = podem ser feitas em paralelo (arquivos diferentes, sem dependências entre si)
- `[USN]` = label que mapeia a task para a User Story N em `spec.md`
- Cada phase é independentemente testável via `quickstart.md`
- **Após Phase 2**: fazer `dotnet build` e garantir zero erros antes de prosseguir
- **Após Phase 3**: testar manualmente os 3 primeiros cenários do quickstart.md
- Tasks de entidade (T007-T026) são mecânicas e seguras para paralelo — apenas troca de tipo de ID e adição de campos
- Tasks de serviço (T046-T056, T091-T095, etc.) são onde está a lógica crítica — revisar cuidadosamente
- Commits recomendados: ao final de cada Phase (não de cada task individual)
