# Relatório SonarQube — NotificationService
## Projeto: `ArquitetoMovel_customer-services` · Sessão: `20260308-df96`

**Data:** 8 de março de 2026  
**Serviço:** `NotificationService/`  
**Issues:** 9 abertas (4 BLOCKER · 1 CRITICAL · 4 MAJOR · 0 MINOR)  
**Hotspots:** 3 (todos com status `TO_REVIEW`)

---

## Resumo por Regra

| Regra | Descrição | Severidade | Ocorrências |
|-------|-----------|------------|-------------|
| `csharpsquid:S2068` | Hard-coded credentials | 🔴 BLOCKER | 2 |
| `secrets:S6703` | Database password in source | 🔴 BLOCKER | 1 |
| `csharpsquid:S2699` | Test without assertion | 🔴 BLOCKER | 1 |
| `csharpsquid:S3776` | Cognitive complexity too high | 🟠 CRITICAL | 1 |
| `docker:S7021` | WORKDIR with relative path | 🟡 MAJOR | 1 |
| `docker:S6570` | Variable not double-quoted | 🟡 MAJOR | 1 |
| `csharpsquid:S6966` | Use awaitable method | 🟡 MAJOR | 1 |
| `csharpsquid:S2629` | String interpolation in logging | 🟡 MAJOR | 1 |

---

## Issues por Severidade

### 🔴 BLOCKER

---

#### Regra: `csharpsquid:S2068` — Credentials should not be hard-coded

**Tipo:** Vulnerabilidade  
**Descrição:** Credenciais hard-coded são extraíveis por qualquer um com acesso ao repositório ou binário distribuído. CWE-798, CWE-259. OWASP A07:2021.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/src/NotificationService.Api/appsettings.json` | 9 | `"password"` detected here, make sure this is not a hard-coded credential. |
| 2 | `NotificationService/src/NotificationService.Infrastructure/Persistence/NotificationDbContextFactory.cs` | 11 | `"password"` detected here, make sure this is not a hard-coded credential. |

**Como corrigir:**
```csharp
// ❌ Não-conforme — appsettings.json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Username=postgres;Password=mysecretpassword;Database=notifications"
  }
}

// ✅ Conforme — usar User Secrets em dev, variável de ambiente em produção
// dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Password=..."
// Em produção: definir variável ConnectionStrings__Default no ambiente ou Key Vault

// ❌ Não-conforme — DbContextFactory
var connectionString = "Host=localhost;Password=hardcoded";

// ✅ Conforme
var connectionString = configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");
```

**Ação:** Revogar a senha exposta imediatamente, rotacionar credenciais do banco, mover para gestão de segredos.

---

#### Regra: `secrets:S6703` — Make sure this database password gets changed and removed from the code

**Tipo:** Vulnerabilidade  
**Descrição:** Senha de banco de dados detectada em arquivo de configuração de IDE (`.idea/dataSources.xml`) comitado no repositório. Qualquer pessoa com acesso ao histórico do repositório pode recuperar essa senha.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/.idea/.idea.NotificationService/.idea/dataSources.xml` | 9 | Make sure this database password gets changed and removed from the code. |

**Como corrigir:**
1. Revogar e rotacionar imediatamente a senha exposta  
2. Adicionar o diretório `.idea/` ao `.gitignore` para evitar recorrência:
```gitignore
# JetBrains IDE files
.idea/
*.iml
```
3. Remover o arquivo do histórico Git com `git filter-branch` ou `BFG Repo Cleaner`  
4. Nunca armazenar credenciais em arquivos de configuração de IDE

---

#### Regra: `csharpsquid:S2699` — Tests should include assertions

**Tipo:** Code Smell (BLOCKER)  
**Descrição:** Um teste sem nenhuma asserção não valida nada. É essencialmente um teste que sempre passa, dando falsa sensação de segurança e não detectando regressões.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/tests/NotificationService.UnitTests/UnitTest1.cs` | 6 | Add at least one assertion to this test case. |

**Como corrigir:**
```csharp
// ❌ Não-conforme — teste sem asserção
[Fact]
public void Test1()
{
    // sem Assert
}

// ✅ Conforme — implementar o cenário real sendo testado
[Fact]
public void SendNotification_WhenCalledWithValidData_ShouldPublishMessage()
{
    // Arrange
    var service = new NotificationService(mockBroker.Object);

    // Act
    service.Send(new Notification("userId", "message"));

    // Assert
    mockBroker.Verify(b => b.Publish(It.IsAny<Notification>()), Times.Once);
}
```

---

### 🟠 CRITICAL

---

#### Regra: `csharpsquid:S3776` — Cognitive Complexity of methods should not be too high

**Tipo:** Code Smell  
**Descrição:** A complexidade cognitiva mede o quão difícil um trecho de código é de entender. Um valor alto indica que o método precisa ser refatorado em métodos menores e mais focados. O limite configurado é 15; o método detectado tem complexidade 16.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/src/NotificationService.Infrastructure/MessageBroker/RabbitMqMessageBroker.cs` | 23 | Refactor this method to reduce its Cognitive Complexity from 16 to the 15 allowed. |

**Como corrigir:**
- Extrair blocos `if/else` aninhados em métodos privados com nomes descritivos  
- Dividir o método com múltiplas responsabilidades em métodos menores e coesos  
- Considerar uso de polimorfismo ou `Strategy Pattern` para substituir condicionais complexas

```csharp
// ❌ Não-conforme — método com múltiplos aninhamentos
public async Task ProcessMessage(IModel channel, BasicDeliverEventArgs args)
{
    if (args != null)
    {
        if (args.Body.Length > 0)
        {
            try
            {
                var message = Deserialize(args.Body);
                if (message.Type == "A") { /* ... */ }
                else if (message.Type == "B") { /* ... */ }
                // ...
            }
            catch (Exception) { /* ... */ }
        }
    }
}

// ✅ Conforme — responsabilidades separadas
public async Task ProcessMessage(IModel channel, BasicDeliverEventArgs args)
{
    if (!IsValidMessage(args)) return;
    var message = DeserializeMessage(args.Body);
    await DispatchByType(message);
}

private bool IsValidMessage(BasicDeliverEventArgs args) => args?.Body.Length > 0;
private async Task DispatchByType(NotificationMessage message) { /* ... */ }
```

---

### 🟡 MAJOR

---

#### Regra: `docker:S7021` — WORKDIR instruction should only be used with absolute path

**Tipo:** Code Smell  
**Descrição:** Usar caminho relativo em `WORKDIR` pode causar comportamento inesperado se o diretório de trabalho atual mudar em instruções anteriores. Caminhos absolutos garantem clareza e confiabilidade.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/Dockerfile` | 17 | Use an absolute path instead of this relative path when defining the WORKDIR. |

**Como corrigir:**
```dockerfile
# ❌ Não-conforme
WORKDIR app

# ✅ Conforme
WORKDIR /app
```

---

#### Regra: `docker:S6570` — Double quote to prevent globbing and word splitting

**Tipo:** Code Smell  
**Descrição:** Referências a variáveis em comandos shell sem aspas duplas estão sujeitas a globbing (expansão de `*`) e word splitting (divisão por espaços), podendo causar comportamento incorreto.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/Dockerfile` | 22 | Surround this variable with double quotes; otherwise, it can lead to unexpected behavior. |

**Como corrigir:**
```dockerfile
# ❌ Não-conforme
RUN echo $APP_NAME

# ✅ Conforme
RUN echo "$APP_NAME"
```

---

#### Regra: `csharpsquid:S6966` — Awaitable method should be used

**Tipo:** Code Smell  
**Descrição:** Usar método síncrono em contexto `async` bloqueia a thread do pool, reduzindo a escalabilidade do serviço.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/src/NotificationService.Api/Program.cs` | 31 | Await `RunAsync` instead. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
app.Run();

// ✅ Conforme
await app.RunAsync();
```

---

#### Regra: `csharpsquid:S2629` — Logging templates should not use string interpolation

**Tipo:** Code Smell  
**Descrição:** Interpolação de strings em templates de log causa avaliação eager (independente do nível de log ativo), desperdiçando memória e CPU. Além disso, impede que ferramentas de APM (Application Insights, Seq) estruturem os logs por parâmetro.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/src/NotificationService.Application/NotificationService.cs` | 36 | Don't use string interpolation in logging message templates. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
_logger.LogInformation($"Processing notification for user {userId} with type {notificationType}");

// ✅ Conforme — structured logging com parâmetros nomeados
_logger.LogInformation(
    "Processing notification for user {UserId} with type {NotificationType}",
    userId,
    notificationType);
```

---

## Security Hotspots

### Hotspots de `NotificationService/Dockerfile`

| # | Linha | Regra | Categoria | Probabilidade | Mensagem |
|---|-------|-------|-----------|---------------|----------|
| 1 | 16 | `docker:S6470` | Permission | MEDIUM | Copying recursively might inadvertently add sensitive data to the container. |
| 2 | 24 | `docker:S6471` | Permission | MEDIUM | This image might run with "root" as the default user. |
| 3 | 26 | `docker:S2612` | Permission | MEDIUM | Make sure granting write access to others is safe here. |

---

#### Hotspot: `docker:S6470` — Recursively copying context directories is security-sensitive (Linha 16)

**Pergunte-se:**
- Os arquivos copiados podem conter dados sensíveis (segredos, chaves, configurações)?
- O contexto de build contém arquivos sem propósito funcional para a imagem final?

**Como corrigir:**
```dockerfile
# ❌ Não-conforme — copia tudo recursivamente
COPY . .

# ✅ Conforme — listar explicitamente apenas o necessário
COPY ./src/NotificationService.Api/ ./src/NotificationService.Api/
COPY ./src/NotificationService.Application/ ./src/NotificationService.Application/
COPY ./src/NotificationService.Infrastructure/ ./src/NotificationService.Infrastructure/
```
Também é recomendado usar `.dockerignore` para filtrar arquivos sensíveis.

---

#### Hotspot: `docker:S6471` — Running containers as a privileged user is security-sensitive (Linha 24)

**Pergunte-se:**
- O serviço é acessível pela internet?
- O container não requer usuário privilegiado para funcionar?

**Como corrigir:**
```dockerfile
# ❌ Não-conforme — sem instrução USER (executa como root)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]

# ✅ Conforme — criar e usar usuário não-root
FROM mcr.microsoft.com/dotnet/aspnet:9.0
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
```

---

#### Hotspot: `docker:S2612` — Setting loose POSIX file permissions is security-sensitive (Linha 26)

**Pergunte-se:**
- O container é um ambiente multi-usuário?
- Serviços rodam com usuários dedicados de baixo privilégio?

**Como corrigir:**
```dockerfile
# ❌ Não-conforme
RUN chmod +x /app/scripts

# ✅ Conforme — permissão mínima necessária
RUN chmod u+x /app/scripts
# Ou com --chown para definir owner específico
COPY --chown=appuser:appgroup --chmod=744 ./scripts /app/scripts
```

---

## Checklist de Correções

- [ ] **[BLOCKER]** Remover senha de `appsettings.json` linha 9 → usar User Secrets / variável de ambiente
- [ ] **[BLOCKER]** Remover senha de `NotificationDbContextFactory.cs` linha 11 → usar `IConfiguration`
- [ ] **[BLOCKER]** Remover `dataSources.xml` do repositório, adicionar `.idea/` ao `.gitignore`, revogar senha exposta
- [ ] **[BLOCKER]** Implementar asserção real em `UnitTest1.cs` linha 6
- [ ] **[CRITICAL]** Refatorar `RabbitMqMessageBroker.cs` linha 23 → extrair métodos privados para reduzir complexidade cognitiva de 16 → ≤15
- [ ] **[MAJOR]** Corrigir `WORKDIR` em `Dockerfile` linha 17 → usar caminho absoluto `/app`
- [ ] **[MAJOR]** Adicionar aspas duplas à variável em `Dockerfile` linha 22 → `"$VARIABLE"`
- [ ] **[MAJOR]** Usar `await app.RunAsync()` em `Program.cs` linha 31
- [ ] **[MAJOR]** Substituir interpolação por structured logging em `NotificationService.cs` linha 36
- [ ] **[HOTSPOT]** Revisar `COPY . .` em `Dockerfile` linha 16 → especificar arquivos/diretórios explicitamente
- [ ] **[HOTSPOT]** Adicionar instrução `USER nonroot` em `Dockerfile` linha 24 → criar usuário dedicado
- [ ] **[HOTSPOT]** Revisar permissão de escrita para outros em `Dockerfile` linha 26 → usar permissão mínima
