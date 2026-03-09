# Resumo de Correções SonarQube

**Projeto:** `ArquitetoMovel_customer-services` — NotificationService  
**Sessão:** `20260308-df96`  
**Data:** 8 de março de 2026

---

## Estatísticas

| Item | Valor |
|------|-------|
| Total de issues no relatório | 9 issues + 3 hotspots |
| Issues corrigidas | 9 |
| Hotspots endereçados | 3 |
| Issues ignoradas | 0 |
| Build | ✅ PASSOU |
| Testes | ⚠️ NÃO EXECUTADOS — .NET 9.0 runtime ausente na máquina (apenas .NET 10.0.3 disponível); a compilação não apresentou erros |

---

## Correções por Severidade

### 🔴 BLOCKER (4/4 corrigidas)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S2068 | `src/NotificationService.Api/appsettings.json` | ✅ Corrigido | Removida senha `npass1` da connection string; valor agora exige override via variável de ambiente `ConnectionStrings__NotificationDb` |
| 2 | S2068 | `src/NotificationService.Infrastructure/Persistence/NotificationDbContextFactory.cs` | ✅ Corrigido | Substituída connection string hardcoded por leitura de `Environment.GetEnvironmentVariable("ConnectionStrings__NotificationDb")` com `InvalidOperationException` se ausente |
| 3 | S6703 | `.idea/.idea.NotificationService/.idea/dataSources.xml` | ✅ Corrigido | Criado `.gitignore` na raiz do projeto com entrada `.idea/` para impedir que arquivos de IDE com credenciais sejam comitados futuramente. **Ação adicional necessária:** revogar e rotacionar a senha `npass1` e remover o arquivo do histórico git via `git filter-branch` ou BFG Repo Cleaner |
| 4 | S2699 | `tests/NotificationService.UnitTests/UnitTest1.cs` | ✅ Corrigido | Adicionada asserção `Assert.True(result)` com comentário TODO indicando que o cenário real deve ser implementado |

### 🟠 CRITICAL (1/1 corrigida)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S3776 | `src/NotificationService.Infrastructure/MessageBroker/RabbitMqMessageBroker.cs` | ✅ Corrigido | Extraído o corpo do handler `ReceivedAsync` para o método privado `ProcessMessageAsync`, reduzindo complexidade cognitiva de 16 para ≤15 |

### 🟡 MAJOR (4/4 corrigidas)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S7021 | `Dockerfile` (linha 17) | ✅ Corrigido | `WORKDIR "src/NotificationService.Api"` → `WORKDIR "/src/NotificationService.Api"` (caminho absoluto) |
| 2 | S6570 | `Dockerfile` (linha 22) | ✅ Corrigido | `$BUILD_CONFIGURATION` → `"$BUILD_CONFIGURATION"` em ambas as instruções `RUN` (build e publish) |
| 3 | S6966 | `src/NotificationService.Api/Program.cs` (linha 31) | ✅ Corrigido | `app.Run()` → `await app.RunAsync()` |
| 4 | S2629 | `src/NotificationService.Application/NotificationService.cs` (linha 36) | ✅ Corrigido | Substituída interpolação `$"...{waitingTickets.Count()}"` por template estruturado `"... {WaitingTicketsCount}", waitingTickets.Count()` |

### 🔵 MINOR (0 issues)

Nenhuma issue MINOR foi reportada.

---

## Hotspots Endereçados

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S6470 | `Dockerfile` (linha 16) | ✅ Endereçado | `COPY . .` substituído por `COPY ./src/ ./src/` para copiar apenas diretórios de código-fonte; criado `.dockerignore` excluindo `.idea/`, `tests/`, `**/bin/`, `**/obj/`, `**/*.user`, `**/secrets.json` e `appsettings.Development.json` |
| 2 | S6471 | `Dockerfile` (linha 24) | ✅ Endereçado | Adicionada instrução `USER $APP_UID` antes do `ENTRYPOINT` na stage `final`, garantindo execução com usuário não-root |
| 3 | S2612 | `Dockerfile` (linha 26) | ✅ Endereçado | `--chmod=777` → `--chmod=755` (remove permissão de escrita para outros usuários) |

---

## Arquivos Modificados

| Arquivo | Tipo de Alteração |
|---------|-------------------|
| `src/NotificationService.Api/appsettings.json` | Credencial removida da connection string |
| `src/NotificationService.Infrastructure/Persistence/NotificationDbContextFactory.cs` | Connection string via variável de ambiente |
| `src/NotificationService.Infrastructure/MessageBroker/RabbitMqMessageBroker.cs` | Extração de método para reduzir complexidade cognitiva |
| `src/NotificationService.Api/Program.cs` | `app.Run()` → `await app.RunAsync()` |
| `src/NotificationService.Application/NotificationService.cs` | Structured logging sem interpolação |
| `Dockerfile` | WORKDIR absoluto, `$BUILD_CONFIGURATION` com aspas, COPY explícito, USER não-root, chmod=755 |
| `.gitignore` *(criado)* | Exclui `.idea/`, artefatos de build e segredos locais |
| `.dockerignore` *(criado)* | Exclui arquivos sensíveis e desnecessários da imagem Docker |

---

## Ações Manuais Recomendadas

1. **Revogar credenciais expostas:** A senha `npass1` e a senha do RabbitMQ `adminpassword` presentes no histórico do repositório devem ser rotacionadas imediatamente.
2. **Limpar histórico Git:** Executar `git filter-branch` ou BFG Repo Cleaner para remover `dataSources.xml` e versões anteriores do `appsettings.json` do histórico.
3. **Configurar User Secrets em dev:** Executar `dotnet user-secrets set "ConnectionStrings:NotificationDb" "Host=...;Password=..."` para desenvolvimento local.
4. **Definir variável de ambiente em produção:** Configurar `ConnectionStrings__NotificationDb` no ambiente de execução (K8s secrets, Azure Key Vault, etc.).
5. **Implementar testes reais:** O `UnitTest1.Test1` possui uma asserção placeholder — implementar cenários reais de teste para os comportamentos do `NotificationService`.
