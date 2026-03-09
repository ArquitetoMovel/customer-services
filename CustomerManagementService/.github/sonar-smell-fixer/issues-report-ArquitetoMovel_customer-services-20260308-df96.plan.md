# Relatório de Análise SonarQube — ArquitetoMovel_customer-services · 20260308-df96

**Data:** 8 de março de 2026  
**Quality Gate:** NONE (sem quality gate configurado)

---

## Resumo de Métricas

| Métrica                   | Valor   |
|---------------------------|---------|
| Linhas de Código (ncloc)  | 2.140   |
| Complexidade Ciclomática  | 185     |
| Duplicações               | 1,1%    |
| Issues totais             | 36      |
| Bugs                      | 0       |
| Vulnerabilidades          | 6       |
| Code Smells               | 30      |
| Security Hotspots         | 6       |
| Rating de Confiabilidade  | A (1.0) |
| Rating de Segurança       | E (5.0) |
| Rating de Manutenção      | A (1.0) |

---

## Relatórios por Serviço / Camada

| Status | Relatório | Serviço | Issues | Hotspots |
|--------|-----------|---------|--------|----------|
| [ ] | [issues-report-customer-management-ArquitetoMovel_customer-services-20260308-df96.plan.md](issues-report-customer-management-ArquitetoMovel_customer-services-20260308-df96.plan.md) | CustomerManagementService | 18 | 0 |
| [ ] | [issues-report-notification-ArquitetoMovel_customer-services-20260308-df96.plan.md](issues-report-notification-ArquitetoMovel_customer-services-20260308-df96.plan.md) | NotificationService | 9 | 3 |
| [ ] | [issues-report-user-management-ArquitetoMovel_customer-services-20260308-df96.plan.md](issues-report-user-management-ArquitetoMovel_customer-services-20260308-df96.plan.md) | UserManagementService | 9 | 3 |

---

## Distribuição de Issues por Severidade

| Severidade | CustomerManagement | Notification | UserManagement | Total |
|------------|-------------------|--------------|----------------|-------|
| 🔴 BLOCKER  | 3                 | 4            | 1              | **8** |
| 🟠 CRITICAL | 0                 | 1            | 1              | **2** |
| 🟡 MAJOR    | 10                | 4            | 6              | **20**|
| 🔵 MINOR    | 5                 | 0            | 1              | **6** |
| **Total**  | **18**            | **9**        | **8**          | **36**|

> *Nota: a linha UserManagement tem 8 issues regulares + 1 hotspot de baixa prioridade (S5122 CORS).*

---

## Security Hotspots Resumo

| # | Serviço | Arquivo | Linha | Categoria | Regra | Prioridade |
|---|---------|---------|-------|-----------|-------|------------|
| 1 | NotificationService | `NotificationService/Dockerfile` | 16 | Permission | `docker:S6470` – Recursive COPY | MEDIUM |
| 2 | NotificationService | `NotificationService/Dockerfile` | 24 | Permission | `docker:S6471` – Root user | MEDIUM |
| 3 | NotificationService | `NotificationService/Dockerfile` | 26 | Permission | `docker:S2612` – Write perms for others | MEDIUM |
| 4 | UserManagementService | `UserManagementService/Dockerfile` | 15 | Permission | `docker:S6470` – Recursive COPY | MEDIUM |
| 5 | UserManagementService | `UserManagementService/Dockerfile` | 23 | Permission | `docker:S6471` – Root user | MEDIUM |
| 6 | UserManagementService | `UserManagementService/src/UserManagement.Api/Program.cs` | 18 | Insecure Config | `csharpsquid:S5122` – Permissive CORS | LOW |

---

## Conclusão Executiva

O projeto apresenta **36 issues abertas** e **6 security hotspots**. Os principais problemas identificados são:

1. **Credenciais hard-coded (BLOCKER)** — 6 ocorrências de senhas e URIs com credenciais fixas em `appsettings.json` e factories de DbContext (`csharpsquid:S2068`, `secrets:S6703`). Risco imediato de exposição. **Prioridade máxima.**

2. **Testes sem asserções (BLOCKER)** — 2 testes completamente vazios (`csharpsquid:S2699`), tornando o suite de testes inútil para detectar regressões.

3. **Race condition potencial (CRITICAL)** — Campo `static` sendo modificado por método de instância no `UserManagementService` (`csharpsquid:S2696`), suscetível a condições de corrida em ambientes concorrentes.

4. **Complexidade cognitiva excessiva (CRITICAL)** — Método no `RabbitMqMessageBroker` com complexidade 16 (limite: 15), dificultando manutenção.

5. **Dockerfiles inseguros (HOTSPOTS)** — Ambos os serviços rodam como `root` e usam `COPY . .` recursivo, potencialmente expondo dados sensíveis na imagem.

6. **Código comentado e strings literais duplicadas (MAJOR/MINOR)** — Dívida técnica espalhada por múltiplos arquivos.

### Ordem de Priorização Recomendada

1. Remover todas as credenciais hard-coded → usar `IConfiguration` / secrets management
2. Adicionar asserções nos testes vazios
3. Corrigir race condition no `AttendanceTicketService`
4. Refatorar `RabbitMqMessageBroker` para reduzir complexidade cognitiva
5. Corrigir Dockerfiles (usuário não-root, COPY explícito, permissões corretas)
6. Resolver Code Smells (S125, S3881, S112, S6966, S3928, etc.)
