# Implementation Plan: app-Finance v2.0 — Revamp Completo

**Branch**: `001-finance-app-v2` | **Date**: 2026-06-11 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-finance-app-v2/spec.md`

---

## Summary

Evolução do app-Finance de um MVP funcional parcial para um produto financeiro pessoal completo e confiável. O plano aborda duas dimensões simultâneas: (1) **correção de fundação** — migração de IDs GUID→INT, implementação de soft delete, auditoria e reconciliação de saldo; e (2) **expansão de funcionalidades** — cartão de crédito com ciclo de fatura, transferências atômicas, filtros, dashboard temporal, CRUD de orçamentos/metas, reset de senha, notificações in-app, dark mode e exportação PDF.

A arquitetura é **Mobile + API REST**: backend ASP.NET Core 8 (Clean Architecture) e frontend React Native/Expo. Toda a camada de domínio existe — o trabalho é corrigir a camada de aplicação (serviços), completar os controllers e construir as telas mobile ausentes.

---

## Technical Context

**Language/Version**:
- Backend: C# 12 / .NET 8
- Frontend: TypeScript 5 / React Native 0.81.5 / Expo 54

**Primary Dependencies**:
- Backend: ASP.NET Core 8, Entity Framework Core 8, ASP.NET Core Identity, JWT Bearer 8, Swashbuckle 6.6
- Frontend: React Navigation 7, Axios 1.15, AsyncStorage 2.2, Expo Vector Icons 13

**Storage**: SQL Server (EF Core 8 com migrations)

**Testing**: Nenhum framework de testes configurado atualmente — escopo desta versão não inclui criação de suíte de testes, mas implementações devem ser testáveis manualmente via Swagger e pelo app.

**Target Platform**: iOS e Android (React Native cross-platform)

**Project Type**: Mobile App (frontend) + REST API (backend) — Opção 3 do template

**Performance Goals**:
- Listagens paginadas em < 500ms
- Dashboard em < 1s
- Geração de PDF em < 10s para períodos de até 3 meses

**Constraints**:
- Sem suporte offline nesta versão
- Uso individual (sem multi-tenant)
- Sem publicação em lojas nesta versão
- WhatsApp Bot e Anexos fora de escopo
- Dados existentes são de teste e podem ser descartados na migração

**Scale/Scope**:
- ~20 telas no frontend (7 atuais + ~13 novas)
- ~46 requisitos funcionais distribuídos em 8 grupos
- 21 entidades no banco, todas impactadas pela migração GUID→INT

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

O arquivo `.specify/memory/constitution.md` contém apenas o template padrão sem princípios específicos definidos para este projeto. Nenhuma restrição constitucional específica foi ratificada.

**Gates aplicados (boas práticas de arquitetura):**

| Gate | Status | Observação |
|---|---|---|
| Separação de responsabilidades (Domain/Application/Infrastructure/API) | PASS | Arquitetura Clean existente mantida |
| Operações financeiras atômicas | PASS | Todas as ops de saldo usarão transações de BD |
| Sem lógica de negócio nos Controllers | PASS | Controllers delegam para Services |
| IDs como INT sequencial | PASS | Migração GUID→INT é Fase 0 |
| Soft delete em entidades com histórico valioso | PASS | Conta, CartaoCredito com IsDeleted |
| Auditoria de operações sensíveis | PASS | LogAuditoria via EF Core interceptor |

**Sem violações que requeiram justificativa.**

---

## Project Structure

### Documentation (this feature)

```text
specs/001-finance-app-v2/
├── plan.md              ← este arquivo
├── research.md          ← decisões técnicas e padrões adotados
├── data-model.md        ← modelo de dados completo com mudanças
├── quickstart.md        ← guia de validação end-to-end
├── contracts/
│   ├── transacoes.md    ← CRUD + status transitions + filtros
│   ├── contas.md        ← account management + soft delete
│   ├── cartoes.md       ← cartão de crédito + faturas
│   ├── transferencias.md ← transferências atômicas
│   ├── orcamentos.md    ← budget CRUD
│   ├── metas.md         ← goals CRUD + aportes
│   ├── auth.md          ← login + reset de senha
│   ├── dashboard.md     ← dashboard com parâmetro mes/ano
│   ├── notificacoes.md  ← central de notificações
│   └── categorias.md    ← categorias customizadas
├── checklists/
│   └── requirements.md  ← checklist de qualidade da spec
└── tasks.md             ← gerado por /speckit-tasks
```

### Source Code (repository root)

```text
FinanceApp.Domain/
├── Entities/
│   ├── Usuario.cs                  [existente — migrar ID]
│   ├── Transacao.cs                [existente — migrar ID, adicionar TransferenciaId]
│   ├── Conta.cs                    [existente — migrar ID, adicionar IsDeleted/DeletedAt]
│   ├── CartaoCredito.cs            [existente — migrar ID, adicionar IsDeleted/DeletedAt]
│   ├── FaturaCartao.cs             [existente — migrar ID, adicionar DataPagamento]
│   ├── Categoria.cs                [existente — migrar ID]
│   ├── Orcamento.cs                [existente — migrar ID]
│   ├── MetaEconomia.cs             [existente — migrar ID, adicionar IsDeleted/DeletedAt]
│   ├── LancamentoMeta.cs           [existente — migrar ID]
│   ├── TransferenciaConta.cs       [existente — migrar ID, completar implementação]
│   ├── Parcelamento.cs             [existente — migrar ID]
│   ├── Notificacao.cs              [existente — migrar ID]
│   ├── LogAuditoria.cs             [existente — migrar ID, implementar preenchimento]
│   ├── ConfiguracaoUsuario.cs      [existente — migrar FK UsuarioId]
│   ├── TokenAtualizacao.cs         [existente — migrar ID]
│   └── CodigoVerificacao.cs        [existente — migrar ID, adicionar Canal/TentativasErradas]
├── Enums/
│   └── Enums.cs                    [existente — sem alterações de enum necessárias]
└── Interfaces/
    └── ISoftDeletable.cs           [NOVO — interface de soft delete]

FinanceApp.Application/
├── Interfaces/
│   ├── ITransacaoService.cs        [existente — ampliar com status, cancelamento]
│   ├── IContaService.cs            [existente — ampliar com soft delete, reconciliação]
│   ├── ICartaoCreditoService.cs    [existente — implementar ciclo de fatura]
│   ├── IFaturaCartaoService.cs     [NOVO — geração, pagamento de fatura]
│   ├── ITransferenciaService.cs    [NOVO — operação atômica]
│   ├── IOrcamentoService.cs        [existente — implementar CRUD completo]
│   ├── IMetaEconomiaService.cs     [existente — implementar CRUD + aportes]
│   ├── INotificacaoService.cs      [existente — implementar geração e listagem]
│   ├── IDashboardService.cs        [existente — adicionar parâmetro mes/ano]
│   ├── ICategoriaService.cs        [existente — adicionar CRUD de customizadas]
│   ├── IAuditService.cs            [NOVO — registrar operações em LogAuditoria]
│   └── IExportacaoService.cs       [NOVO — geração de PDF]
├── Services/
│   ├── TransacaoService.cs         [existente — refatorar completamente]
│   ├── ContaService.cs             [existente — refatorar com reconciliação]
│   ├── CartaoCreditoService.cs     [existente — implementar lógica de limite]
│   ├── FaturaCartaoService.cs      [NOVO]
│   ├── TransferenciaService.cs     [NOVO]
│   ├── OrcamentoService.cs         [existente — implementar CRUD]
│   ├── MetaEconomiaService.cs      [existente — implementar CRUD + aportes]
│   ├── NotificacaoService.cs       [existente — implementar geração]
│   ├── DashboardService.cs         [existente — refatorar com mes/ano]
│   ├── CategoriaService.cs         [existente — adicionar CRUD]
│   ├── AuditService.cs             [NOVO]
│   └── ExportacaoService.cs        [NOVO]
└── DTOs/
    ├── Transacoes/TransacaoDTOs.cs [existente — adicionar campos status, filtros]
    ├── Contas/ContaDTOs.cs         [existente — IDs int]
    ├── Cartoes/CartaoCreditoDTOs.cs[existente — adicionar fatura DTOs]
    ├── Faturas/FaturaDTOs.cs       [NOVO]
    ├── Transferencias/TransferenciaDTO.cs [NOVO]
    ├── Orcamentos/OrcamentoDTOs.cs [existente — IDs int]
    ├── Metas/MetaDTOs.cs           [existente — adicionar aporte DTO]
    ├── Auth/AuthDTOs.cs            [existente — adicionar reset senha DTOs]
    ├── Notificacoes/NotificacaoDTOs.cs [existente — IDs int]
    ├── Dashboard/DashboardDTOs.cs  [existente — adicionar mes/ano params]
    ├── Categorias/CategoriaDTOs.cs [existente — adicionar create/update DTOs]
    └── Exportacao/ExportacaoDTOs.cs[NOVO]

FinanceApp.Infrastructure/
├── Data/
│   ├── FinanceDbContext.cs         [existente — adicionar global query filters, interceptor]
│   └── Interceptors/
│       └── AuditInterceptor.cs     [NOVO — EF Core SaveChanges interceptor]
└── Migrations/
    ├── 20260419175534_Inicial.cs   [existente — não modificar]
    └── YYYYMMDDHHMMSS_GuidToInt_SoftDelete_Audit.cs [NOVA migration]

FinanceApp.API/
└── Controllers/
    ├── BaseController.cs           [existente — atualizar UsuarioId para int]
    ├── AuthController.cs           [existente — adicionar reset senha endpoints]
    ├── TransacoesController.cs     [existente — adicionar filtros, PUT status, DELETE]
    ├── ContasController.cs         [existente — soft delete]
    ├── CartoesController.cs        [existente — ampliar com faturas]
    ├── FaturasController.cs        [NOVO]
    ├── TransferenciasController.cs [NOVO]
    ├── CategoriasController.cs     [existente — adicionar CRUD customizadas]
    ├── OrcamentosController.cs     [existente — implementar CRUD]
    ├── MetasController.cs          [existente — implementar CRUD + aportes]
    ├── DashboardController.cs      [existente — adicionar mes/ano query params]
    ├── NotificacoesController.cs   [existente — implementar listagem + marcar lida]
    └── ExportacaoController.cs     [NOVO]

finance-app-mobile/src/
├── api/
│   ├── client.ts                   [existente — remover IP hardcoded, usar env var]
│   └── config.ts                   [existente — ler EXPO_PUBLIC_API_URL]
├── contexts/
│   ├── AuthContext.tsx             [existente — atualizar IDs para number]
│   └── ThemeContext.tsx            [NOVO — dark mode provider]
├── navigation/
│   └── AppNavigator.tsx            [existente — adicionar novas telas e tabs]
├── screens/
│   ├── Auth/
│   │   ├── LoginScreen.tsx         [existente — adicionar link "Esqueci senha"]
│   │   ├── RegistroScreen.tsx      [existente — sem mudanças]
│   │   ├── EsqueciSenhaScreen.tsx  [NOVO]
│   │   ├── VerificarCodigoScreen.tsx [NOVO]
│   │   └── NovaSenhaScreen.tsx     [NOVO]
│   ├── Home/
│   │   └── HomeScreen.tsx          [existente — seletor de mês, excluir transferências]
│   ├── Transacoes/
│   │   ├── TransacoesScreen.tsx    [existente — filtros, botão editar, marcar paga]
│   │   └── CriarTransacaoScreen.tsx [existente — modo edição + tipo transferência]
│   ├── Contas/
│   │   └── ContasScreen.tsx        [existente — soft delete com aviso, bloquear se cartão]
│   ├── Cartoes/
│   │   ├── CartoesScreen.tsx       [NOVO — lista de cartões com limite]
│   │   ├── CriarCartaoScreen.tsx   [NOVO — formulário de cartão]
│   │   └── FaturaDetalheScreen.tsx [NOVO — detalhe da fatura + pagamento]
│   ├── Transferencias/
│   │   └── TransferenciaScreen.tsx [NOVO — formulário de transferência]
│   ├── Orcamentos/
│   │   └── OrcamentosScreen.tsx    [NOVO — lista + CRUD de orçamentos]
│   ├── Metas/
│   │   └── MetasScreen.tsx         [NOVO — lista + CRUD + aportes]
│   ├── Categorias/
│   │   └── CategoriasScreen.tsx    [NOVO — lista + CRUD de customizadas]
│   ├── Notificacoes/
│   │   └── NotificacoesScreen.tsx  [NOVO — central in-app]
│   └── Config/
│       └── ConfigScreen.tsx        [existente — dark mode funcional, link reset senha]
├── theme/
│   ├── colors.ts                   [existente — adicionar paleta dark]
│   └── spacing.ts                  [existente — sem mudanças]
├── types/
│   └── index.ts                    [existente — IDs como number, novos tipos]
└── utils/
    └── formatters.ts               [existente — sem mudanças]
```

**Structure Decision**: Opção 3 (Mobile + API). A estrutura existente é mantida e expandida. Novos arquivos seguem o padrão de nomenclatura e localização já estabelecido no projeto.

---

## Complexity Tracking

> Sem violações constitucionais identificadas. Nenhuma justificativa necessária.

---

## Implementation Phases

### Phase 0 — Fundação (Pré-requisito absoluto)
**Objetivo**: Estabilizar a base de dados e a infraestrutura antes de qualquer nova feature.

| Item | Arquivo(s) | Risco |
|---|---|---|
| GUID → INT em todas as 21 entidades | `FinanceApp.Domain/Entities/*.cs` + nova migration | Alto |
| Interface `ISoftDeletable` + campos nas entidades | `ISoftDeletable.cs`, `Conta.cs`, `CartaoCredito.cs` | Baixo |
| Global Query Filter (EF) para soft delete | `FinanceDbContext.cs` | Médio |
| Remover cascade deletes perigosos | `FinanceDbContext.cs` | Médio |
| `AuditInterceptor` → preenche `LogAuditoria` | `AuditInterceptor.cs`, `FinanceDbContext.cs` | Médio |
| Reconciliação de `SaldoAtual` → cálculo derivado | `ContaService.cs`, `TransacaoService.cs` | Alto |
| Invalidar todos os tokens após migração | `TokenAtualizacao` tabela truncada | Baixo |
| `EXPO_PUBLIC_API_URL` via `.env` (remover IP hardcoded) | `client.ts`, `config.ts`, `.env.example` | Baixo |

### Phase 1 — Correções Críticas do Núcleo
**Objetivo**: O núcleo funcional (transações + saldo) deve funcionar de forma impecável.

| Item | Backend | Frontend |
|---|---|---|
| Máquina de estados de transação | `TransacaoService.cs` | `TransacoesScreen` — botão "Marcar pago", status badge |
| Edição de transação (não-parcelada) | `PUT /api/transacoes/{id}` | `CriarTransacaoScreen` — modo edição |
| Cancelamento com reversão de saldo | `DELETE /api/transacoes/{id}` | Confirmação + toast de estorno |
| Cancelamento de parcelamento | `DELETE /api/transacoes/{id}/parcelamento` | Modal de confirmação |
| Filtros na listagem | `GET /api/transacoes?...` com query params | `TransacoesScreen` — drawer de filtros |
| PENDENTE → VENCIDA automático | Verificação no `GET /api/transacoes` | Badge visual VENCIDA |

### Phase 2 — Funcionalidades de Alto Impacto
**Objetivo**: Implementar as funcionalidades marcadas como prioridade 1 no F.7.

| Item | Backend | Frontend |
|---|---|---|
| Reset de senha | 3 novos endpoints em `AuthController` | 3 novas telas de Auth |
| Transferências | `TransferenciaService` + `TransferenciasController` | `TransferenciaScreen` |
| Cartões de Crédito (tela completa) | `CartaoCreditoService` + `FaturaCartaoService` + `FaturasController` | `CartoesScreen`, `CriarCartaoScreen`, `FaturaDetalheScreen` |
| Notificações in-app | `NotificacaoService` + `NotificacoesController` | `NotificacoesScreen` + badge |
| Dashboard com navegação de mês | `DashboardService` com parâmetro mes/ano | `HomeScreen` — seletor de mês |

### Phase 3 — CRUD de Suporte e Orçamentos/Metas
**Objetivo**: Completar as funcionalidades de planejamento financeiro.

| Item | Backend | Frontend |
|---|---|---|
| CRUD de Orçamentos | `OrcamentoService` CRUD + alertas | `OrcamentosScreen` |
| CRUD de Metas + Aportes | `MetaEconomiaService` CRUD + `LancamentoMeta` | `MetasScreen` |
| CRUD de Categorias Customizadas | `CategoriaService` create/update/delete | `CategoriasScreen` |

### Phase 4 — Polimento e Features Baixa Prioridade
**Objetivo**: Experiência completa do produto.

| Item | Arquivo(s) |
|---|---|
| Dark Mode | `ThemeContext.tsx`, `colors.ts` (dark palette), todos os screens |
| Exportação PDF | `ExportacaoService.cs`, `ExportacaoController.cs`, `ExportacaoScreen.tsx` |
