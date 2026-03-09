# Relatório de Análise SonarQube — ArquitetoMovel_customer-services

**Data:** 8 de março de 2026
**Session ID:** 20260308-433a
**Quality Gate:** NONE (Quality Gate não configurado para o projeto)

---

## Resumo de Métricas

| Métrica                  | Valor  |
|--------------------------|--------|
| Linhas de Código (ncloc) | 2.140  |
| Cobertura                | N/D    |
| Duplicações              | 1,1%   |
| Issues totais            | 36     |
| Bugs                     | 0      |
| Vulnerabilidades         | 6      |
| Code Smells              | 30     |
| Security Hotspots        | 6      |
| Complexidade Ciclomática | 185    |
| Complexidade Cognitiva   | 61     |
| Rating de Confiabilidade | A      |
| Rating de Segurança      | E (🔴) |
| Rating de Manutenção     | A      |

> ⚠️ **Atenção:** O Rating de Segurança está em **E**, o pior nível possível, devido às 6 vulnerabilidades abertas (credenciais hard-coded).

---

## Visão Geral por Severidade

| Severidade  | Quantidade |
|-------------|-----------|
| 🔴 BLOCKER  | 8          |
| 🟠 CRITICAL | 2          |
| 🟡 MAJOR    | 20         |
| 🔵 MINOR    | 6          |
| ⚪ INFO      | 0          |

---

## Relatórios por Camada

| Status | Arquivo de Relatório | Descrição |
|--------|---------------------|-----------|
| [ ] | [issues-report-security-ArquitetoMovel_customer-services-20260308-433a.plan.md](issues-report-security-ArquitetoMovel_customer-services-20260308-433a.plan.md) | Issues de Segurança (BLOCKER + Security Hotspots) |
| [ ] | [issues-report-app-ArquitetoMovel_customer-services-20260308-433a.plan.md](issues-report-app-ArquitetoMovel_customer-services-20260308-433a.plan.md) | Issues da Camada de Aplicação, API e UI |
| [ ] | [issues-report-infra-ArquitetoMovel_customer-services-20260308-433a.plan.md](issues-report-infra-ArquitetoMovel_customer-services-20260308-433a.plan.md) | Issues da Camada de Infraestrutura e Docker |
| [ ] | [issues-report-tests-ArquitetoMovel_customer-services-20260308-433a.plan.md](issues-report-tests-ArquitetoMovel_customer-services-20260308-433a.plan.md) | Issues nos Testes (ausência de asserções) |

---

## Conclusão Executiva

O projeto `customer-services` apresenta **problemas críticos de segurança** que exigem atenção imediata:

1. **🔴 Credenciais hard-coded** em múltiplos arquivos de configuração (`appsettings.json`, factories de DbContext, arquivo IDE `dataSources.xml`). Este é o problema de maior risco — expõe senhas de banco de dados no código-fonte.
2. **🔴 Testes sem asserções** nos projetos `NotificationService` e `UserManagementService`, o que significa que os testes não validam nenhum comportamento real.
3. **🟠 Complexidade cognitiva elevada** no `RabbitMqMessageBroker` — dificulta manutenção e testes.
4. **🟠 Escrita em campo `static` a partir de método de instância** no `AttendanceTicketService` — potencial condição de corrida.
5. **Docker Hotspots** em múltiplos Dockerfiles: cópia recursiva de contexto, execução como root e permissões permissivas.

### Prioridade de Resolução Recomendada
1. Remover imediatamente credenciais hard-coded e rotacionar as senhas expostas
2. Corrigir testes sem asserções
3. Resolver issues críticos de concorrência (campo estático)
4. Revisar Security Hotspots de Docker
5. Resolver demais code smells da camada de infraestrutura e aplicação
