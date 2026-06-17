# Feature Specification: app-Finance v2.0 — Revamp Completo

**Feature Branch**: `001-finance-app-v2`

**Created**: 2026-06-11

**Status**: Draft

**Input**: Análise SDD completa conduzida via entrevista estruturada com o Product Owner. O escopo cobre a evolução do núcleo do app-Finance para um produto financeiro pessoal completo, corrigindo inconsistências críticas de dados e implementando todas as funcionalidades planejadas no backend mas ausentes no frontend.

---

## Contexto do Produto

O app-Finance é um aplicativo mobile de gestão financeira pessoal que permite ao usuário controlar receitas, despesas, contas bancárias, cartões de crédito, orçamentos e metas de economia. O produto possui uma base técnica bem estruturada, mas apresenta uma lacuna significativa entre o que foi modelado no servidor e o que está acessível ao usuário no aplicativo. Esta especificação formaliza os requisitos para fechar essa lacuna e elevar a confiabilidade financeira do produto.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Integridade Financeira: Saldos Sempre Corretos (Priority: P1)

O usuário lança uma despesa de R$150 no supermercado. O saldo da sua conta corrente cai imediatamente de R$1.000 para R$850. Mais tarde, percebe que lançou na conta errada e cancela a transação: o saldo volta a R$1.000 com uma notificação visual de que o valor foi estornado. Ele então relança corretamente na conta certa.

**Why this priority**: Sem saldo confiável, o produto não tem credibilidade financeira. Qualquer outra funcionalidade sobre uma base de saldo incorreto gera decisões erradas para o usuário.

**Independent Test**: Criar conta com saldo inicial R$1.000, criar transação de despesa R$150, verificar saldo = R$850, cancelar transação, verificar saldo = R$1.000.

**Acceptance Scenarios**:

1. **Dado** que uma conta tem saldo R$1.000, **Quando** o usuário criar uma despesa EFETIVADA de R$150, **Então** o saldo da conta deve ser R$850 imediatamente.
2. **Dado** que uma transação EFETIVADA de R$150 existe, **Quando** o usuário cancelar essa transação, **Então** o saldo da conta deve voltar a R$1.000 e uma mensagem de confirmação de estorno deve ser exibida.
3. **Dado** que uma transação está com status PENDENTE, **Quando** ela for criada, **Então** o saldo da conta NÃO deve ser afetado.
4. **Dado** que uma transação PENDENTE existe, **Quando** o usuário marcá-la como EFETIVADA, **Então** o saldo da conta deve ser deduzido nesse momento.
5. **Dado** que uma conta é excluída, **Quando** o sistema processar a exclusão, **Então** o histórico de transações dessa conta deve ser preservado para consulta futura.

---

### User Story 2 — Correção de Lançamentos: Editar uma Transação (Priority: P1)

O usuário lançou uma compra de R$89,90 na categoria "Lazer" mas deveria ser "Alimentação". Ele toca o ícone de edição ao lado da transação, corrige a categoria e salva. O dashboard e os relatórios são atualizados automaticamente.

**Why this priority**: Erros de lançamento são inevitáveis em qualquer app de finanças. Sem edição, o usuário acumula dados incorretos permanentes que distorcem todos os relatórios e orçamentos.

**Independent Test**: Criar transação com categoria X, editar para categoria Y, verificar que os resumos refletem a categoria Y.

**Acceptance Scenarios**:

1. **Dado** que uma transação comum existe, **Quando** o usuário tocar no ícone de edição, **Então** um formulário pré-preenchido com os dados atuais deve ser exibido.
2. **Dado** que o usuário edita o valor de uma transação EFETIVADA de R$100 para R$80, **Então** o saldo da conta deve ser ajustado em +R$20 (diferença).
3. **Dado** que a transação é uma parcela de um parcelamento, **Quando** o usuário tentar editá-la, **Então** o sistema deve exibir mensagem informando que transações parceladas não podem ser editadas individualmente, oferecendo apenas a opção de cancelar o parcelamento completo.
4. **Dado** que uma transação de transferência existe, **Quando** o usuário tentar editá-la, **Então** ambas as transações vinculadas (origem e destino) devem refletir a alteração de forma atômica.

---

### User Story 3 — Ciclo de Status: Controle de Contas a Pagar (Priority: P1)

O usuário agenda o aluguel de R$1.200 para o dia 5 do mês próximo como PENDENTE. No dia 5, o app exibe a transação como VENCIDA (pois a data passou sem confirmação). O usuário então toca em "Marcar como pago", confirma o pagamento e o saldo da conta cai R$1.200.

**Why this priority**: O controle de contas a pagar/receber é um dos fluxos mais usados em gestão financeira pessoal. Sem essa máquina de estados funcional, o produto não entrega o prometido.

**Independent Test**: Criar transação PENDENTE com data no passado, verificar que aparece como VENCIDA, marcar como paga, verificar efeito no saldo.

**Acceptance Scenarios**:

1. **Dado** que uma transação PENDENTE tem data anterior a hoje, **Quando** o usuário abrir a tela de transações, **Então** ela deve aparecer com status VENCIDA e destaque visual (ex: cor vermelha).
2. **Dado** que uma transação está VENCIDA, **Quando** o usuário tocar em "Marcar como pago", **Então** o status muda para EFETIVADA e o saldo da conta é deduzido nesse momento.
3. **Dado** que uma transação está PENDENTE, **Quando** o usuário tocar em "Marcar como pago", **Então** o status muda para EFETIVADA e o saldo é afetado.
4. **Dado** que uma transação EFETIVADA existe, **Quando** o usuário cancelá-la, **Então** o saldo é revertido, uma notificação de estorno é exibida e a transação é removida do histórico.

---

### User Story 4 — Transferência entre Contas (Priority: P2)

O usuário quer mover R$500 da conta corrente para a poupança. Ele acessa a opção de transferência, seleciona as contas de origem e destino, informa o valor e confirma. O saldo da corrente cai R$500, o da poupança sobe R$500, e o saldo total consolidado permanece igual. No resumo mensal, a transferência não aparece como despesa nem como receita.

**Why this priority**: Transferências são operações comuns e críticas. Se tratadas como despesa/receita, inflam artificialmente os relatórios e distorcem a percepção financeira do usuário.

**Independent Test**: Com conta A (R$1.000) e conta B (R$200), transferir R$300 de A para B, verificar A=R$700, B=R$500, total=R$1.200 (igual ao antes), e que o dashboard de despesas/receitas não mudou.

**Acceptance Scenarios**:

1. **Dado** que o usuário tem conta A com R$1.000 e conta B com R$200, **Quando** ele transferir R$300 de A para B, **Então** A deve ter R$700, B deve ter R$500 e o saldo total consolidado deve ser R$1.200.
2. **Dado** que uma transferência foi realizada, **Quando** o usuário visualizar o resumo mensal de receitas e despesas, **Então** o valor da transferência NÃO deve aparecer em nenhuma dessas categorias.
3. **Dado** que uma transferência é criada, **Quando** o usuário visualizar as transações da conta de origem, **Então** deve aparecer "Transferência para [Nome da Conta Destino]" com ícone diferenciado.
4. **Dado** que a criação da transferência falha parcialmente (débito feito mas crédito não), **Então** o sistema deve reverter completamente a operação e informar o erro ao usuário.

---

### User Story 5 — Cartão de Crédito: Compra Parcelada e Fatura (Priority: P2)

O usuário compra um celular de R$1.200 parcelado em 3x no cartão Nubank (fecha dia 10, vence dia 17). O limite disponível cai R$1.200 imediatamente. No mês seguinte, a fatura de R$400 aparece como "Fatura Junho — R$400". O usuário toca em "Pagar fatura", confirma e R$400 são debitados da conta corrente vinculada ao cartão. O limite disponível sobe R$400.

**Why this priority**: Cartão de crédito é o instrumento financeiro mais usado pelos brasileiros. Sem esse módulo, o app não representa a realidade financeira do usuário.

**Independent Test**: Criar cartão com limite R$2.000, fazer compra parcelada R$1.200 em 3x, verificar limite disponível = R$800, verificar fatura do mês = R$400, pagar fatura, verificar conta debitada e limite = R$1.200.

**Acceptance Scenarios**:

1. **Dado** que um cartão tem limite R$2.000, **Quando** o usuário registrar uma compra parcelada de R$1.200 em 3x, **Então** o limite disponível deve ser R$800 imediatamente.
2. **Dado** que a compra parcelada foi registrada no mês de junho, **Quando** o usuário visualizar as faturas, **Então** deve existir uma parcela de R$400 nas faturas de junho, julho e agosto.
3. **Dado** que a fatura de junho está fechada e vale R$400, **Quando** o usuário pagá-la, **Então** R$400 devem ser debitados da conta vinculada ao cartão e o limite disponível deve subir de R$800 para R$1.200.
4. **Dado** que uma fatura já foi marcada como PAGA, **Quando** o usuário tentar pagá-la novamente, **Então** o sistema deve bloquear a operação com mensagem de alerta.
5. **Dado** que uma compra é feita após o dia de fechamento do cartão (ex: dia 11 quando o fechamento é dia 10), **Então** essa compra deve ser alocada na fatura do mês seguinte.

---

### User Story 6 — Filtros e Busca em Transações (Priority: P2)

O usuário quer saber quanto gastou em "Alimentação" no mês de abril. Ele acessa a tela de transações, filtra por categoria "Alimentação", seleciona o período abril/2026 e visualiza apenas as transações relevantes com o total no topo.

**Why this priority**: Sem filtros, o usuário não consegue extrair informação útil de sua lista de transações, tornando o histórico financeiro inútil.

**Independent Test**: Criar 10 transações de categorias variadas, aplicar filtro por uma categoria específica, verificar que apenas as transações daquela categoria aparecem com total correto.

**Acceptance Scenarios**:

1. **Dado** que o usuário aplica filtro por categoria "Alimentação", **Quando** a lista carregar, **Então** apenas transações dessa categoria devem aparecer, com o total filtrado exibido no topo.
2. **Dado** que o usuário define um período de 01/04/2026 a 30/04/2026, **Quando** o filtro for aplicado, **Então** apenas transações dentro desse período devem aparecer.
3. **Dado** que o usuário digita "mercado" no campo de busca, **Quando** a busca for executada, **Então** apenas transações cuja descrição contém "mercado" devem aparecer (busca case-insensitive).
4. **Dado** que múltiplos filtros estão ativos (categoria + período + tipo), **Quando** a lista carregar, **Então** apenas transações que satisfazem TODOS os critérios devem aparecer.
5. **Dado** que filtros estão ativos, **Quando** o usuário tocar em "Limpar filtros", **Então** todos os filtros devem ser removidos e a lista completa deve reaparecer.

---

### User Story 7 — Dashboard com Navegação Mensal (Priority: P2)

O usuário quer analisar seus gastos de março de 2026. No dashboard, ele toca na seta "◄" e navega de mês em mês até chegar a março/2026. Todos os gráficos, resumos e percentuais atualizam para refletir o período selecionado.

**Why this priority**: O dashboard fixo no mês atual impede análise histórica, que é um dos principais valores de um app de finanças pessoais.

**Independent Test**: Com transações em meses diferentes, navegar para um mês específico e verificar que apenas os dados daquele mês são exibidos.

**Acceptance Scenarios**:

1. **Dado** que o usuário está no dashboard do mês atual, **Quando** ele tocar na seta "◄", **Então** o dashboard deve exibir os dados do mês anterior.
2. **Dado** que o usuário está navegando pelos meses, **Quando** ele chegar a 12 meses atrás, **Então** a seta "◄" deve ser desabilitada (limite de 12 meses).
3. **Dado** que o usuário está em um mês anterior, **Quando** ele tocar na seta "►", **Então** o dashboard deve avançar para o próximo mês, sem ultrapassar o mês atual.
4. **Dado** que o usuário está em um mês específico, **Quando** o dashboard carregar, **Então** transferências não devem aparecer nos totais de receitas e despesas.

---

### User Story 8 — Orçamentos: Criar e Acompanhar Limites (Priority: P2)

O usuário define um orçamento de R$800/mês para "Alimentação". Durante o mês, conforme vai lançando compras de supermercado, o dashboard mostra uma barra de progresso verde que vai evoluindo para amarelo (80%) e vermelho (100%). Ao ultrapassar R$800, uma notificação in-app alerta que o limite foi excedido.

**Why this priority**: Orçamentos são a principal ferramenta de controle proativo do usuário. Sem CRUD de orçamentos, o usuário não pode personalizar seus limites.

**Independent Test**: Criar orçamento de R$100 para categoria X, lançar despesa de R$85 na categoria X, verificar barra em 85% amarelo. Lançar mais R$20, verificar barra em 105% vermelho e notificação.

**Acceptance Scenarios**:

1. **Dado** que o usuário cria um orçamento de R$800 para "Alimentação" no mês de junho, **Quando** ele navegar para o dashboard de junho, **Então** o orçamento deve aparecer com barra de progresso em 0%.
2. **Dado** que o usuário atingiu 80% do orçamento de uma categoria, **Quando** o dashboard for exibido, **Então** a barra de progresso deve mudar para amarelo/laranja.
3. **Dado** que o usuário ultrapassou 100% do orçamento, **Quando** o evento ocorrer, **Então** a barra deve ficar vermelha e uma notificação in-app deve ser gerada.
4. **Dado** que o usuário tenta criar dois orçamentos para a mesma categoria no mesmo mês, **Quando** o segundo for salvo, **Então** o sistema deve bloquear e informar que já existe um orçamento para esse período.

---

### User Story 9 — Metas de Economia: Criar e Aportar (Priority: P2)

O usuário cria uma meta "Viagem Europa" com valor alvo R$10.000 e prazo dezembro/2026. A cada mês, ele registra um aporte de R$800. O app exibe o progresso (ex: "R$2.400 de R$10.000 — 24%") e o prazo restante. Quando atingir 100%, uma notificação celebra o objetivo.

**Why this priority**: Metas de economia aumentam o engajamento e entregam o valor principal do planejamento financeiro de longo prazo.

**Independent Test**: Criar meta R$1.000, fazer aporte R$400, verificar progresso 40%, fazer aporte R$600, verificar progresso 100% e notificação.

**Acceptance Scenarios**:

1. **Dado** que uma meta "Viagem" com valor alvo R$10.000 foi criada, **Quando** o usuário visualizar a lista de metas, **Então** deve aparecer com progresso 0% e prazo correto.
2. **Dado** que a meta existe, **Quando** o usuário registrar um aporte de R$800, **Então** o valor atual deve subir para R$800 e a barra de progresso deve refletir 8%.
3. **Dado** que o usuário atinge 100% da meta, **Quando** o último aporte for registrado, **Então** uma notificação in-app deve ser gerada parabenizando o usuário.
4. **Dado** que a data limite de uma meta passou sem atingir 100%, **Quando** o usuário visualizar a meta, **Então** ela deve ser destacada visualmente como "Prazo expirado".

---

### User Story 10 — Reset de Senha (Priority: P2)

O usuário esqueceu sua senha. Na tela de login, toca em "Esqueci minha senha", informa o e-mail. O sistema pergunta se deseja receber o código por e-mail ou WhatsApp (opção aparece apenas se tiver número cadastrado). O usuário escolhe e-mail, recebe um código de 6 dígitos, insere no app, define nova senha e faz login.

**Why this priority**: Usuários que perdem acesso e não conseguem recuperar abandonam o app permanentemente com perda de todo o histórico.

**Independent Test**: Na tela de login, iniciar fluxo de reset, inserir e-mail válido, escolher canal, inserir código recebido, definir nova senha, fazer login com a nova senha.

**Acceptance Scenarios**:

1. **Dado** que o usuário acessa "Esqueci minha senha" e insere e-mail cadastrado, **Quando** confirmar, **Então** um código de 6 dígitos deve ser enviado pelo canal escolhido dentro de 60 segundos.
2. **Dado** que o usuário recebeu o código, **Quando** inserir um código correto dentro de 15 minutos, **Então** deve ser redirecionado para definir nova senha.
3. **Dado** que o código expirou (mais de 15 minutos), **Quando** o usuário tentar usá-lo, **Então** o sistema deve informar que o código expirou e oferecer o reenvio.
4. **Dado** que o usuário não tem WhatsApp cadastrado, **Quando** acessar o fluxo de reset, **Então** a opção WhatsApp NÃO deve aparecer.
5. **Dado** que o usuário insere 3 códigos errados consecutivos, **Quando** a terceira tentativa falhar, **Então** o fluxo deve ser bloqueado por 5 minutos.

---

### User Story 11 — Notificações In-App (Priority: P2)

O usuário abre o app e vê um ícone de sino com badge "2". Ao tocar, vê duas notificações: "Orçamento Alimentação: 87% do limite atingido" e "Meta Viagem Europa: 50% alcançada!". Ele pode marcar como lidas individualmente ou todas de uma vez.

**Why this priority**: Notificações in-app fecham o ciclo de feedback do usuário sobre o estado de suas finanças sem depender de infraestrutura de push externo.

**Independent Test**: Disparar evento de 80% de orçamento, verificar que notificação aparece na central com status "não lida" e badge no ícone.

**Acceptance Scenarios**:

1. **Dado** que um orçamento atinge 80%, **Quando** o evento ocorrer, **Então** uma notificação deve ser gerada e o badge do sino deve incrementar.
2. **Dado** que existem notificações não lidas, **Quando** o usuário abrir a central de notificações, **Então** deve ver a lista com destaque visual para as não lidas.
3. **Dado** que o usuário toca em uma notificação, **Quando** a ação for registrada, **Então** ela deve ser marcada como lida e o badge deve decrementar.
4. **Dado** que o usuário toca em "Marcar todas como lidas", **Quando** a ação for confirmada, **Então** todas as notificações devem mudar para "lida" e o badge deve zerar.

---

### User Story 12 — Dark Mode Completo (Priority: P3)

O usuário acessa Configurações e ativa o dark mode. Imediatamente, todas as telas do app mudam para a paleta escura: fundos escuros, textos claros, ícones ajustados. A preferência é salva e mantida após fechar e reabrir o app.

**Why this priority**: Conforto visual e acessibilidade, especialmente em uso noturno. Ainda que de menor prioridade funcional, afeta todas as telas.

**Independent Test**: Ativar dark mode, fechar o app, reabrir, verificar que todas as telas mantêm o tema escuro.

**Acceptance Scenarios**:

1. **Dado** que o usuário está no app com tema claro, **Quando** ativar o dark mode nas configurações, **Então** TODAS as telas devem mudar imediatamente para o tema escuro sem reiniciar o app.
2. **Dado** que o dark mode está ativo, **Quando** o usuário fechar e reabrir o app, **Então** o tema escuro deve ser mantido automaticamente.
3. **Dado** que o dark mode está ativo, **Quando** qualquer tela for aberta, **Então** nenhuma tela deve exibir fundo branco ou texto escuro sobre fundo escuro (contraste inválido).

---

### User Story 13 — Exportação em PDF (Priority: P3)

O usuário quer enviar seu extrato de maio para sua planilha contábil. Ele acessa a opção "Exportar", seleciona o período maio/2026, e o app gera e disponibiliza um PDF com todas as transações do período, totais por categoria e saldo final.

**Why this priority**: Portabilidade de dados é valorizada por usuários que precisam integrar com outros processos (declaração IR, contabilidade).

**Independent Test**: Com transações cadastradas no mês, gerar PDF, verificar que o arquivo contém lista de transações, totais e saldo.

**Acceptance Scenarios**:

1. **Dado** que o usuário seleciona um período para exportação, **Quando** confirmar, **Então** um PDF deve ser gerado com: lista de transações, categoria, valor, data, total de receitas, total de despesas e saldo do período.
2. **Dado** que o PDF foi gerado, **Quando** ele estiver disponível, **Então** o usuário deve poder compartilhá-lo via e-mail, WhatsApp ou salvar no dispositivo.
3. **Dado** que o período selecionado tem mais de 3 meses, **Quando** o usuário tentar exportar, **Então** o sistema deve alertar sobre possível tempo de processamento maior.

---

### Edge Cases

- O que acontece se o usuário deletar uma conta que tem cartão de crédito ativo vinculado? → Bloqueio com mensagem explicativa.
- O que acontece se o usuário tentar pagar uma fatura de cartão já marcada como PAGA? → Bloqueio com mensagem "Fatura já paga".
- O que acontece se a transferência for criada com origem e destino iguais? → Validação bloqueia antes de salvar.
- O que acontece ao cancelar um parcelamento em que metade das parcelas já foi paga? → Apenas as parcelas com status diferente de EFETIVADA são canceladas; as pagas permanecem no histórico.
- O que acontece se a data de uma transação PENDENTE for hoje? → Ainda permanece PENDENTE; só vira VENCIDA a partir do dia seguinte.
- O que acontece se o saldo de uma conta ficar negativo após uma despesa? → Permitido, exibir em vermelho com aviso de saldo negativo.
- O que acontece se o usuário tenta excluir uma categoria padrão do sistema? → Bloqueio — categorias padrão são protegidas.
- O que acontece se o usuário tenta excluir uma categoria customizada que tem transações? → Bloqueio com contagem de transações vinculadas.
- O que acontece com transferências no cálculo do dashboard se o usuário navegar para um mês histórico? → Transferências continuam excluídas do cálculo de receitas/despesas em qualquer mês.
- O que acontece se o código de reset de senha for inserido 3 vezes incorretamente? → Bloqueio por 5 minutos antes de nova tentativa.
- O que acontece ao gerar PDF de um mês sem transações? → PDF é gerado com uma mensagem de "Nenhuma transação no período".

---

## Requirements *(mandatory)*

### Functional Requirements

#### FR-FOUNDATION — Infraestrutura e Integridade de Dados

- **FR-001**: O sistema DEVE migrar todos os identificadores de entidades de GUID para INT sequencial auto-incremental, mantendo todos os relacionamentos entre entidades.
- **FR-002**: O sistema DEVE implementar exclusão lógica (soft delete) nas entidades: Conta, CartaoCredito, MetaEconomia e Orcamento, preservando o histórico ao invés de remover fisicamente.
- **FR-003**: O sistema DEVE registrar em log de auditoria toda operação de criação, edição, mudança de status e cancelamento de transações financeiras.
- **FR-004**: O sistema DEVE calcular o saldo atual de uma conta como: `SaldoInicial + SOMA(receitas EFETIVADAS) - SOMA(despesas EFETIVADAS)`, recalculando após qualquer operação que afete transações.
- **FR-005**: O sistema DEVE garantir que operações que afetam saldo (criar, editar, cancelar, mudar status) sejam atômicas — ou tudo é salvo, ou nada é alterado.

#### FR-TRANSACTIONS — Gestão de Transações

- **FR-006**: O sistema DEVE implementar a máquina de estados de transações: PENDENTE → EFETIVADA (ação manual), PENDENTE → VENCIDA (automático quando data < hoje), VENCIDA → EFETIVADA (ação manual), EFETIVADA → cancelada (com reversão de saldo).
- **FR-007**: O sistema DEVE permitir ao usuário editar os seguintes campos de uma transação não-parcelada: valor, descrição, categoria, conta, data, status e forma de pagamento.
- **FR-008**: O sistema NÃO DEVE permitir edição de parcelas individuais de um parcelamento; a única ação permitida é cancelar todas as parcelas não-EFETIVADAS do parcelamento.
- **FR-009**: Ao cancelar uma transação EFETIVADA, o sistema DEVE reverter o efeito no saldo da conta vinculada e exibir confirmação visual do estorno ao usuário.
- **FR-010**: O sistema DEVE exibir filtros na tela de transações para: período (data início e fim), categoria (seleção múltipla), conta, tipo (despesa/receita), status, e busca textual por descrição.
- **FR-011**: Transações do tipo PENDENTE NÃO DEVEM afetar o saldo da conta até serem marcadas como EFETIVADA.
- **FR-012**: O sistema DEVE verificar e atualizar automaticamente o status de transações PENDENTE com data vencida para VENCIDA no carregamento da listagem.

#### FR-TRANSFERS — Transferências entre Contas

- **FR-013**: O sistema DEVE permitir ao usuário realizar transferências entre suas contas, criando atomicamente: uma transação de saída na conta de origem e uma de entrada na conta de destino.
- **FR-014**: Transferências NÃO DEVEM alterar o saldo total consolidado do usuário — apenas redistribuir entre contas.
- **FR-015**: Transações de transferência DEVEM ser exibidas com identificação clara do vínculo (ex: "↔ Transferência para [Nome da Conta]") em ambas as contas.
- **FR-016**: Transferências NÃO DEVEM ser contabilizadas nos totais de receitas e despesas do dashboard e relatórios mensais.
- **FR-017**: O sistema NÃO DEVE permitir transferências entre a mesma conta de origem e destino.

#### FR-CREDIT-CARDS — Cartões de Crédito e Faturas

- **FR-018**: O sistema DEVE permitir ao usuário cadastrar cartões de crédito informando: nome, bandeira, limite total, dia de fechamento e dia de vencimento da fatura.
- **FR-019**: Ao registrar uma compra parcelada no cartão, o sistema DEVE reduzir o limite disponível pelo valor total da compra imediatamente e distribuir as parcelas nas faturas dos meses correspondentes.
- **FR-020**: O sistema DEVE gerar e manter faturas mensais por cartão, agrupando todas as compras do período de cada fatura (considerando o dia de fechamento para alocação de compras).
- **FR-021**: O sistema DEVE permitir ao usuário pagar uma fatura, o que deve: debitar o valor da conta bancária vinculada ao cartão, marcar a fatura como PAGA e restaurar o limite proporcional.
- **FR-022**: O sistema NÃO DEVE permitir o pagamento de uma fatura já marcada como PAGA.
- **FR-023**: O sistema NÃO DEVE permitir a exclusão de um cartão que possua faturas com status ABERTA ou FECHADA.
- **FR-024**: O sistema NÃO DEVE permitir a exclusão de uma conta bancária que possua cartão de crédito ativo vinculado.

#### FR-BUDGETS — Orçamentos

- **FR-025**: O sistema DEVE permitir ao usuário criar, editar e excluir orçamentos mensais por categoria, com um valor limite definido.
- **FR-026**: O sistema NÃO DEVE permitir mais de um orçamento ativo para a mesma categoria no mesmo mês/ano.
- **FR-027**: O sistema DEVE calcular automaticamente o percentual utilizado de cada orçamento com base nas despesas EFETIVADAS da categoria no período.
- **FR-028**: O sistema DEVE gerar uma notificação in-app quando um orçamento atingir 80% do limite e outra quando atingir 100%.

#### FR-GOALS — Metas de Economia

- **FR-029**: O sistema DEVE permitir ao usuário criar, editar e excluir metas de economia com: título, valor alvo, prazo e valor atual.
- **FR-030**: O sistema DEVE permitir ao usuário registrar aportes manuais a uma meta, incrementando o valor atual.
- **FR-031**: O sistema DEVE gerar uma notificação in-app quando uma meta atingir 100% do valor alvo.
- **FR-032**: O sistema DEVE destacar visualmente metas cujo prazo expirou sem atingir o valor alvo.

#### FR-DASHBOARD — Dashboard com Navegação Temporal

- **FR-033**: O sistema DEVE permitir ao usuário navegar entre meses no dashboard, com limite de 12 meses para trás a partir do mês atual.
- **FR-034**: O dashboard DEVE exibir, para o mês selecionado: total de receitas, total de despesas, saldo do período, gastos por categoria, transações recentes e progresso de orçamentos.
- **FR-035**: Transferências entre contas NÃO DEVEM ser contabilizadas nos totais de receitas e despesas do dashboard em nenhum mês.

#### FR-AUTH — Recuperação de Acesso

- **FR-036**: O sistema DEVE permitir ao usuário solicitar reset de senha informando seu e-mail cadastrado.
- **FR-037**: O sistema DEVE oferecer ao usuário a escolha do canal de entrega do código: e-mail (sempre disponível) ou WhatsApp (apenas se número cadastrado).
- **FR-038**: O código de verificação para reset DEVE ter 6 dígitos numéricos, expirar em 15 minutos e ser bloqueado após 3 tentativas incorretas.

#### FR-NOTIFICATIONS — Notificações In-App

- **FR-039**: O sistema DEVE manter uma central de notificações in-app com status de lida/não lida para cada notificação.
- **FR-040**: O sistema DEVE exibir um badge com a contagem de notificações não lidas no ícone de notificações.
- **FR-041**: Notificações devem ser geradas pelos eventos: orçamento a 80%, orçamento a 100%, meta atingida, fatura de cartão próxima do vencimento.

#### FR-UX — Experiência do Usuário

- **FR-042**: O sistema DEVE implementar dark mode funcional em todas as telas, com preferência persistida entre sessões.
- **FR-043**: O sistema DEVE permitir ao usuário exportar um extrato de transações em PDF, com seleção de período máximo de 3 meses.
- **FR-044**: O sistema DEVE permitir ao usuário criar, editar e excluir categorias customizadas de despesas e receitas com nome, ícone e cor.
- **FR-045**: O sistema NÃO DEVE permitir a exclusão de categorias com transações vinculadas, exibindo a contagem antes de bloquear.
- **FR-046**: O sistema NÃO DEVE permitir a exclusão de categorias padrão (pré-cadastradas pelo sistema).

### Key Entities *(include if feature involves data)*

- **Transacao**: Representa um lançamento financeiro (receita ou despesa). Possui tipo, status, valor, data, categoria, conta vinculada e pode ter vínculo com parcelamento ou transferência.
- **Conta**: Conta bancária, carteira ou investimento do usuário. Possui saldo inicial e saldo atual calculado. Suporta exclusão lógica preservando histórico.
- **CartaoCredito**: Cartão de crédito do usuário com limite total e disponível. Vinculado a uma conta bancária para débito de faturas.
- **FaturaCartao**: Agrupamento mensal de compras realizadas no cartão. Possui ciclo de status ABERTA → FECHADA → PAGA.
- **TransferenciaConta**: Registro de transferência entre contas. Vincula par de transações (saída + entrada) atomicamente.
- **Orcamento**: Limite mensal definido pelo usuário por categoria. Único por (usuário, categoria, mês, ano).
- **MetaEconomia**: Objetivo financeiro de longo prazo com valor alvo, prazo e aportes progressivos.
- **Notificacao**: Alerta gerado por eventos do sistema (orçamento, meta, fatura). Possui estado lida/não lida.
- **LogAuditoria**: Registro imutável de todas as operações sensíveis do sistema (criação, edição, cancelamento, mudança de status de transações financeiras).
- **Categoria**: Classificação de transações. Pode ser padrão (sistema) ou customizada (usuário). Possui tipo (despesa/receita), ícone e cor.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: O saldo exibido de qualquer conta deve corresponder 100% à soma matemática de todas as suas transações EFETIVADAS acrescida do saldo inicial, em qualquer momento após uma operação.
- **SC-002**: O usuário consegue localizar qualquer transação específica em sua lista aplicando no máximo 2 filtros, em menos de 30 segundos.
- **SC-003**: O usuário consegue realizar uma transferência entre contas e verificar os saldos atualizados em ambas as contas em menos de 60 segundos.
- **SC-004**: O usuário consegue registrar uma compra parcelada no cartão e verificar o impacto no limite disponível em menos de 60 segundos.
- **SC-005**: 100% dos eventos de orçamento atingindo 80% e 100% devem gerar notificações in-app correspondentes.
- **SC-006**: O usuário consegue recuperar acesso à conta via reset de senha em menos de 5 minutos do início do fluxo ao novo login.
- **SC-007**: Todas as telas do app devem ser visualmente corretas (contraste adequado, sem elementos cortados) tanto no tema claro quanto no tema escuro.
- **SC-008**: A exportação de um extrato PDF de 1 mês deve estar disponível para o usuário em menos de 10 segundos.
- **SC-009**: Nenhuma operação de cancelamento, edição ou exclusão de transação deve deixar qualquer saldo de conta em estado inconsistente com o histórico de transações.
- **SC-010**: A migração de GUID para INT deve ser executável sem perda de dados, com todos os relacionamentos entre entidades preservados após a migração.

---

## Assumptions

- O aplicativo é destinado a uso pessoal individual — sem suporte a contas compartilhadas, multi-usuário familiar ou permissões diferenciadas.
- Os dados existentes no banco de dados são exclusivamente dados de teste e podem ser descartados durante a migração de GUID para INT.
- A funcionalidade de push notifications externas (FCM/APNs) está fora do escopo desta versão; notificações serão apenas in-app.
- A integração com WhatsApp Bot e upload de anexos/comprovantes estão fora do escopo desta versão.
- A funcionalidade de transações recorrentes automáticas (via job/schedule) está fora do escopo desta versão; o registro de transações permanece manual.
- O envio de e-mail para reset de senha requer configuração de servidor SMTP pelo time técnico; a especificação cobre o fluxo funcional, não a infraestrutura de envio.
- A exportação em PDF é gerada sob demanda pelo usuário, não agendada automaticamente.
- A publicação nas lojas (App Store / Google Play) não é um objetivo desta versão.
- O limite de navegação histórica no dashboard é de 12 meses para trás a partir do mês atual; navegação para meses futuros não é suportada nesta versão.
- Transações parceladas não podem ser editadas individualmente após o registro; o parcelamento é definido uma única vez no lançamento.
- A priorização de funcionalidades segue: integridade financeira (saldo, status, edição) > novas telas de alto impacto (cartão, transferência, filtros) > CRUD de suporte (orçamento, metas) > experiência (dark mode, PDF, categorias).
