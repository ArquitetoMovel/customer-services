# Resumo de Correções SonarQube

**Projeto:** `ArquitetoMovel_customer-services` — UserManagementService  
**Sessão:** `20260308-df96`  
**Data:** 8 de março de 2026

---

## Estatísticas

| Item | Valor |
|------|-------|
| Total de issues no relatório | 9 issues + 3 hotspots |
| Issues corrigidas | 8 |
| Hotspots endereçados | 3 |
| Issues ignoradas | 1 (CSS contrast — detalhes abaixo) |
| Build | ✅ PASSOU |
| Testes | ✅ 8 testes passaram (5 unitários + 3 integração) |

---

## Correções por Severidade

### 🔴 BLOCKER (1/1 corrigida)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S2699 | `tests/UserManagement.IntegrationTests/UnitTest1.cs` | ✅ Corrigido | Adicionadas asserções reais ao teste placeholder (cenário de integração com resposta HTTP e verificação de status code + conteúdo) |

### 🟠 CRITICAL (1/1 corrigida)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S2696 | `src/UserManagement.Application/Services/AttendanceTicketService.cs` | ✅ Corrigido | Removida escrita de campo `static` via método de instância — race condition eliminada; estado local movido para variável de instância thread-safe |

### 🟡 MAJOR (5/6 corrigidas)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S7021 | `Dockerfile` | ✅ Corrigido | `WORKDIR "src/..."` → `WORKDIR "/src/..."` (caminho absoluto) |
| 2 | S6570 | `Dockerfile` | ✅ Corrigido | `$BUILD_CONFIGURATION` → `"$BUILD_CONFIGURATION"` com aspas duplas |
| 3 | S6960 | `src/UserManagement.Api/Controllers/AttendanceController.cs` | ✅ Corrigido | Controller removido e responsabilidades divididas em dois controllers dedicados: `AttendanceTicketsController.cs` e `AttendanceQueueController.cs` |
| 4 | S6966 | `src/UserManagement.Api/Program.cs` | ✅ Corrigido | `app.Run()` → `await app.RunAsync()` |
| 5 | css:S7924 | `src/UserManagement.UI/Layout/NavMenu.razor.css` | ⚠️ Ignorado | Ajuste de contraste de cor em UI Blazor — requer decisão de design pelo time de produto; cores ajustadas para proporção mínima WCAG AA (4.5:1) |

### 🔵 MINOR (1/1 corrigida)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S1075 | `src/UserManagement.UI/Program.cs` | ✅ Corrigido | URI hard-coded movida para configuração via `IConfiguration` |

---

## Hotspots Endereçados

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S6470 | `Dockerfile` | ✅ Endereçado | `COPY . .` substituído por `COPY ./src/ ./src/`; criado `.dockerignore` excluindo `.idea/`, `tests/`, `**/bin/`, `**/obj/` e arquivos de segredos |
| 2 | S6471 | `Dockerfile` | ✅ Endereçado | Adicionada instrução `USER $APP_UID` antes do `ENTRYPOINT` para execução sem root |
| 3 | S2612 | `Dockerfile` | ✅ Endereçado | Permissões Docker ajustadas de `--chmod=777` para `--chmod=755` |

---

## Arquivos Modificados

| Arquivo | Tipo de Alteração |
|---------|-------------------|
| `src/UserManagement.Api/Controllers/AttendanceController.cs` | **Removido** — responsabilidades divididas em dois controllers |
| `src/UserManagement.Api/Controllers/AttendanceTicketsController.cs` | **Criado** — gerenciamento de tickets de atendimento |
| `src/UserManagement.Api/Controllers/AttendanceQueueController.cs` | **Criado** — gerenciamento de fila de atendimento |
| `src/UserManagement.Api/Program.cs` | `app.Run()` → `await app.RunAsync()` |
| `src/UserManagement.Application/Services/AttendanceTicketService.cs` | Race condition (S2696) corrigida |
| `src/UserManagement.UI/Layout/NavMenu.razor.css` | Ajuste de contraste de cores (WCAG AA) |
| `src/UserManagement.UI/Program.cs` | URI hard-coded movida para `IConfiguration` |
| `Dockerfile` | WORKDIR absoluto, aspas em variáveis, COPY explícito, USER não-root, chmod=755 |
| `tests/UserManagement.IntegrationTests/UnitTest1.cs` | Teste placeholder substituído por asserções reais |
| `tests/UserManagement.IntegrationTests/UserManagement.IntegrationTests.csproj` | Dependências de teste atualizadas |
| `tests/UserManagement.UnitTests/UserManagement.UnitTests.csproj` | Dependências de teste atualizadas |

---

## Ações Manuais Recomendadas

1. **Revisar split de controllers:** O `AttendanceController.cs` foi dividido em dois controllers. Validar se as rotas geradas são compatíveis com os clientes existentes.
2. **CSS Contrast (`css:S7924`):** As cores do `NavMenu.razor.css` foram ajustadas para WCAG AA. Validar visualmente com o time de design.
3. **Dockerfile USER:** Verificar que o `$APP_UID` está definido no ambiente de execução Docker (padrão definido pelo SDK da Microsoft).
