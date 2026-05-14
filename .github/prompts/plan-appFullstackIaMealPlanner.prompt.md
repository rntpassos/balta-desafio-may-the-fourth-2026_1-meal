## Plan: App Fullstack + IA Meal Planner

Construir o Nível 3 como uma solução multi-projeto em .NET 10 com Backend API, Application/Core/Infra/Ai separados e Frontend em Blazor WebAssembly. A recomendação é tratar a IA como um subsistema controlado por contratos, ferramentas permitidas, saídas estruturadas e fallback determinístico, usando Microsoft.Extensions.AI com Ollama local, PostgreSQL com EF Core e uma V1 sem autenticação.

**Steps**
1. Fase 1 - Definição do escopo funcional da V1
Definir a jornada principal: usuário informa ingredientes disponíveis, restrições opcionais, objetivo nutricional e janela de tempo livre; o sistema cruza os dados com agenda e retorna uma ou mais sugestões de receita adequadas. Fixar o que entra na V1: sugestão de receita, justificativa da escolha, tempo total estimado, lista de ingredientes faltantes, passos de preparo e indicadores nutricionais básicos. Excluir da V1: autenticação, edição colaborativa, upload de imagem, compra automática e múltiplos provedores de agenda.

2. Fase 2 - Estruturação da solução e projetos base
Criar a solução em camadas com dependências unidirecionais: Frontend -> Api -> Application -> Core; Api -> Ai e Infra via abstrações da Application/Core; Infra e Ai implementam contratos definidos internamente. Projetos recomendados: src/OneMeal.Api, src/OneMeal.Application, src/OneMeal.Core, src/OneMeal.Infra, src/OneMeal.Ai, src/OneMeal.Web, tests/OneMeal.UnitTests, tests/OneMeal.IntegrationTests, tests/OneMeal.ArchitectureTests. Esta fase bloqueia as demais.

3. Fase 3 - Modelagem de domínio e contratos de caso de uso
Modelar agregados e value objects mínimos: Ingredient, PantrySnapshot, AvailabilityWindow, Recipe, RecipeSuggestion, NutritionProfile, UserPreferences, AgendaSlot, SuggestMealRequest e SuggestMealResponse. Separar regra determinística de regra probabilística: filtros por tempo, restrições e disponibilidade ficam na Application/Core; composição criativa da receita e adaptação textual ficam na Ai. Definir contratos de saída estruturada para IA com versionamento explícito. Esta fase depende da fase 2.

4. Fase 4 - Plataforma de agentes e governança de IA
Aplicar a arquitetura base do plain-ia-agent-clean-code: definir contrato padrão do agente com entrada, contexto, ferramentas permitidas, formato de saída, timeout e política de erro; criar catálogo de ferramentas com permissão explícita por agente; implementar guardrails de entrada/saída, validação de schema, proteção contra prompt injection e prevenção de vazamento de dados sensíveis; registrar trilha de auditoria por execução; prever fallback seguro quando a IA não retornar estrutura válida. Esta fase depende da fase 3.

5. Fase 5 - Agentes e ferramentas do domínio
Criar pelo menos três agentes/coordenadores com responsabilidades separadas: agente de interpretação da solicitação do usuário, agente de composição/sugestão de receita e agente de explicação/resumo para a UI. Definir ferramentas permitidas por agente: consulta a agenda, consulta a repositório de receitas/base local, cálculo nutricional, normalização de ingredientes e classificador de restrições. Persistir as instruções dos agentes em markdown na pasta agentes, conforme o desafio, e manter implementações .cs separadas para execução. Esta fase depende da fase 4.

6. Fase 6 - Integrações e infraestrutura
Implementar Infra para PostgreSQL via EF Core com migrations, repositórios e seed inicial de receitas/ingredientes. Integrar agenda por adapter desacoplado, começando com provider mock ou local calendar service para a primeira entrega e deixando o contrato pronto para futura integração externa. Configurar Ollama local via Microsoft.Extensions.AI com opções externas em configuração, timeout, retry controlado e roteamento simples por tarefa. Esta fase pode avançar em paralelo com parte da fase 5 após os contratos estarem definidos.

7. Fase 7 - API HTTP e observabilidade
Expor endpoints mínimos: POST /api/meal-suggestions, GET /api/recipes/{id}, GET /api/health e endpoint opcional para catálogo auxiliar de ingredientes. Padronizar DTOs, problem details, validação, correlação de requisição e logs estruturados. Adicionar observabilidade mínima com logs por caso de uso, duração de tool calls, sucesso/fallback da IA e razão de rejeição de sugestões. Esta fase depende das fases 3 e 6.

8. Fase 8 - Frontend Blazor WebAssembly
Construir uma dashboard responsiva inspirada no design.md, preservando a estética NASA-punk: grade fixa, painéis modulares, fundo estrelado, tipografia Space Grotesk/Manrope, cards isométricos e feedback visual em neon green/tech red. Telas da V1: dashboard principal, formulário de entrada, painel de agenda/tempo livre, lista de sugestões, detalhe da receita e estados de carregamento/erro/fallback. O frontend deve consumir apenas a API pública, sem lógica de domínio duplicada. Esta fase depende da fase 7, mas o design system e componentes base podem começar em paralelo após a fase 2.

9. Fase 9 - Design system e componentes reutilizáveis
Transformar os tokens do design.md em CSS variables e componentes reutilizáveis do Blazor: TechPanel, TechButton, StatusBadge, SegmentedProgressBar, RecipeCardIsometric, TerminalInput e HUDOverlay. Aplicar regras visuais concretas: cantos retos, borda dourada de 1px, contêineres com chamfer em áreas principais, overlays translúcidos, hover com inner-glow e indicadores segmentados para energia/nutrientes. Garantir experiência desktop e mobile sem perder a leitura de dashboard. Esta fase pode rodar em paralelo com a fase 8.

10. Fase 10 - Qualidade automatizada e avaliação de IA
Cobrir quatro níveis de qualidade previstos no plano base: testes unitários para domínio e filtros determinísticos; testes de integração para API, banco, adapters e tools; testes de regressão comportamental para prompts/saídas estruturadas; testes adversariais para entradas maliciosas, prompt injection e respostas inválidas do modelo. Manter um conjunto dourado de cenários de sugestão de receitas com resultados esperados mínimos. Esta fase acompanha as implementações e bloqueia o fechamento da V1.

11. Fase 11 - Operação local, segurança e entrega incremental
Definir configuração local reproduzível com appsettings, user secrets, connection string PostgreSQL, endpoint do Ollama e dados seed. Estabelecer política de evolução: piloto local -> validação funcional -> endurecimento de observabilidade e testes -> refinamento do frontend. Documentar fallback humano/manual para quando a IA falhar: retornar uma sugestão baseada apenas em filtros determinísticos e receitas previamente cadastradas. Esta fase depende das fases 6 a 10.

12. Fase 12 - Documentação do desafio
Atualizar README com visão do problema, arquitetura final, instruções de execução .NET 10, como subir PostgreSQL e Ollama, estrutura dos projetos, catálogo de agentes, prints da UI e aprendizados do desafio. Documentar também os artefatos pedidos pela base genérica: arquitetura de referência, catálogo de ferramentas/permissões, catálogo de prompts, plano de testes de IA e plano operacional. Esta fase depende do fechamento técnico da V1.

**Relevant files**
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/README.md — fonte do escopo do desafio, descrição dos níveis e documentação a ser atualizada
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/design.md — fonte dos tokens visuais, componentes e direção estética do frontend
- c:/Users/renat/Projetos/plain-ia-agent-clean-code.md — base arquitetural de agentes, governança, testes e operação a ser seguida
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/balta-desafio-may-the-fourth-2026_1-meal.slnx — solução a expandir com os projetos .NET 10
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/src/OneMeal.Core — entidades, value objects, contratos de domínio e invariantes
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/src/OneMeal.Application — casos de uso, orquestração, validações e interfaces
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/src/OneMeal.Ai — agentes, prompts, schemas de saída e adapters de modelo
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/src/OneMeal.Infra — EF Core, PostgreSQL, adapters externos, observabilidade e implementações de ferramentas
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/src/OneMeal.Api — endpoints HTTP, composição de DI, validação e contratos públicos
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/src/OneMeal.Web — Blazor WebAssembly, design system e telas da dashboard
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/agentes — markdowns dos agentes e catálogo de permissões conforme exigência do desafio
- c:/Users/renat/Projetos/balta-desafio-may-the-fourth-2026_1-meal/tests — testes unitários, integração, arquitetura e avaliação comportamental

**Verification**
1. Validar a arquitetura por testes de dependência entre projetos para garantir que Core não dependa de Infra/Ai/UI.
2. Executar testes unitários dos filtros determinísticos: tempo disponível, ingredientes presentes, restrições e prioridade nutricional.
3. Executar testes de integração da API com PostgreSQL e provider de IA configurado para resposta controlada/mockada.
4. Validar regressão dos prompts com um conjunto dourado de cenários: pouco tempo, poucos ingredientes, restrição alimentar e agenda sem janela útil.
5. Executar testes adversariais de prompt injection e payloads inválidos para confirmar guardrails e fallback.
6. Verificar manualmente a UI em desktop e mobile, incluindo loading, erro, fallback e visual NASA-punk coerente com design.md.
7. Confirmar que, com falha do Ollama, a API ainda retorna resposta útil via estratégia determinística de degradação.
8. Confirmar logs estruturados, correlação por requisição e métricas básicas de latência e erro por tool/agent.

**Decisions**
- Stack alvo: .NET 10 em todos os projetos, incluindo API, bibliotecas e Blazor WebAssembly.
- IA: Microsoft.Extensions.AI com Ollama local como provedor inicial, mantendo adapter para futura troca de modelo/provedor.
- Persistência: PostgreSQL com EF Core desde a primeira entrega.
- Autenticação: fora da V1; o sistema será single-user/local no primeiro ciclo.
- Incluído na V1: sugestão de receita baseada em ingredientes + tempo livre + regras nutricionais básicas + explicação do resultado.
- Excluído da V1: autenticação, multiusuário, agenda real com OAuth, compras, upload de mídia e painel administrativo.
- Regra central: toda decisão irreversível ou externa deve ter fallback seguro; toda saída de IA que impacta o domínio deve ser estruturada e validada.

**Further Considerations**
1. Agenda externa: a recomendação é iniciar com adapter fake/local e só integrar Google Calendar ou Microsoft Graph após a V1 estar estável.
2. Base de receitas: a recomendação é manter seed local versionado no repositório para previsibilidade de testes e demos.
3. Frontend: a recomendação é priorizar uma única dashboard excelente e responsiva, em vez de várias telas superficiais.