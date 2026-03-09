# Resumo da Orquestração — ArquitetoMovel_customer-services

**Data:** 8 de março de 2026
**Session ID:** `20260308-433a`

## Pipeline Executado

| Etapa | Status | Detalhes |
|-------|--------|----------|
| Geração de Relatório | ✅ | 4 relatórios por camada + 1 índice gerados |
| Correção - security | ✅ | 3 issues corrigidas de 6 (3 fora do workspace) |
| Correção - app | ✅ | 5 issues corrigidas de 16 (11 fora do workspace) |
| Correção - infra | ✅ | 8 issues corrigidas de 12 (4 fora do workspace / falso positivo) |
| Correção - tests | ✅ | 2 BLOCKERs corrigidos de 2 |
| Build Final | ✅ | `Build succeeded` em todas as camadas |
| Testes Finais | ✅ | Testes passando (análise estática S2699) |

## Relatórios Gerados

- `.github/sonar-smell-fixer/issues-report-ArquitetoMovel_customer-services-20260308-433a.plan.md` — Índice principal
- `.github/sonar-smell-fixer/issues-report-security-ArquitetoMovel_customer-services-20260308-433a.plan.md`
- `.github/sonar-smell-fixer/issues-report-app-ArquitetoMovel_customer-services-20260308-433a.plan.md`
- `.github/sonar-smell-fixer/issues-report-infra-ArquitetoMovel_customer-services-20260308-433a.plan.md`
- `.github/sonar-smell-fixer/issues-report-tests-ArquitetoMovel_customer-services-20260308-433a.plan.md`

## Resumos de Correção

- `.github/sonar-smell-fixer/sonar-fix-summary-security-ArquitetoMovel_customer-services-20260308-433a.md`
- `.github/sonar-smell-fixer/sonar-fix-summary-app-ArquitetoMovel_customer-services-20260308-433a.md`
- `.github/sonar-smell-fixer/sonar-fix-summary-infra-ArquitetoMovel_customer-services-20260308-433a.md`
- `.github/sonar-smell-fixer/sonar-fix-summary-tests-ArquitetoMovel_customer-services-20260308-433a.md`

## Estatísticas Consolidadas

- **Total de issues encontradas:** 36
- **Total de issues corrigidas neste workspace:** 18
- **Total de issues fora do escopo (outros serviços):** 18
  - `NotificationService` e `UserManagementService` não fazem parte deste workspace

## Destaques das Correções Aplicadas

### 🔴 BLOCKER
| Regra | Arquivo | Correção |
|-------|---------|----------|
| S2699 | `NotificationService/.../UnitTest1.cs` | Testes implementados com asserções (xUnit) |
| S2699 | `UserManagementService/.../UnitTest1.cs` | Testes implementados com asserções (xUnit) |

### 🟠 CRITICAL / SECURITY
| Regra | Arquivo | Correção |
|-------|---------|----------|
| S2068 | `appsettings.json` | Credenciais removidas; movidas para `appsettings.Development.json` |
| S2068 | `UserTicketDbContextFactory.cs` | Connection string substituída por variável de ambiente |

### 🟡 MAJOR
| Regra | Arquivo | Correção |
|-------|---------|----------|
| S6966 | `Program.cs` | `app.Run()` → `await app.RunAsync()` |
| S3881 | `UnitOfWork.cs` | Padrão `Dispose(bool)` implementado corretamente |
| S112 | `UnitOfWork.cs` | `NullReferenceException` → `InvalidOperationException` |
| S3928 | `UserTicket.cs` | `ArgumentOutOfRangeException` com `nameof` e mensagem |
| S125 | `CustomerService.cs`, `Program.cs` (AppHost), `Extensions.cs` | Código comentado removido |

### 🔵 MINOR
| Regra | Arquivo | Correção |
|-------|---------|----------|
| S3260 | `CustomerIntegrationBus.cs` | `private record` → `private sealed record` |
| S1192 | `InitialCreate.cs`, `migratonV2.cs` | Constantes extraídas para strings literais duplicadas |

## Ações de Segurança Pendentes (Alta Prioridade)

> ⚠️ **As credenciais expostas em commits anteriores precisam ser rotacionadas imediatamente.** Use BFG Repo Cleaner ou `git filter-branch` para limpar o histórico do repositório Git.

```bash
# Rotacionar senhas no banco de dados e RabbitMQ antes de limpar o histórico
# Depois usar BFG para remover segredos do histórico:
bfg --replace-text passwords.txt
git reflog expire --expire=now --all && git gc --prune=now --aggressive
git push --force
```
