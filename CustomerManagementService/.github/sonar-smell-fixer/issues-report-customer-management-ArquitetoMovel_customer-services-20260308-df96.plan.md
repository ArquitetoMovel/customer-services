# Relatório SonarQube — CustomerManagementService
## Projeto: `ArquitetoMovel_customer-services` · Sessão: `20260308-df96`

**Data:** 8 de março de 2026  
**Serviço:** `CustomerManagementService/`  
**Issues:** 18 abertas (3 BLOCKER · 0 CRITICAL · 10 MAJOR · 5 MINOR)  
**Hotspots:** 0

---

## Resumo por Regra

| Regra | Descrição | Severidade | Ocorrências |
|-------|-----------|------------|-------------|
| `csharpsquid:S2068` | Hard-coded credentials | 🔴 BLOCKER | 3 |
| `csharpsquid:S125` | Commented-out code | 🟡 MAJOR | 5 |
| `csharpsquid:S112` | General exception thrown | 🟡 MAJOR | 2 |
| `csharpsquid:S6966` | Use awaitable method | 🟡 MAJOR | 1 |
| `csharpsquid:S3881` | Fix IDisposable pattern | 🟡 MAJOR | 1 |
| `csharpsquid:S3928` | ArgumentException param name | 🟡 MAJOR | 1 |
| `csharpsquid:S1192` | Duplicate string literals | 🔵 MINOR | 3 |
| `csharpsquid:S3260` | Mark record class as `sealed` | 🔵 MINOR | 1 |
| `csharpsquid:S2325` | Make method `static` | 🔵 MINOR | 1 |

---

## Issues por Severidade

### 🔴 BLOCKER

---

#### Regra: `csharpsquid:S2068` — Credentials should not be hard-coded

**Tipo:** Vulnerabilidade  
**Descrição:** Credenciais hard-coded no código-fonte permitem que atacantes extraiam informações sensíveis. Este é um risco de segurança crítico relacionado ao CWE-798 e CWE-259 (OWASP A07:2021 – Identification and Authentication Failures).

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementApp/appsettings.json` | 9 | `"password"` detected here, make sure this is not a hard-coded credential. |
| 2 | `CustomerManagementService/src/CustomerManagementApp/appsettings.json` | 10 | Review this hard-coded URI, which may contain a credential. |
| 3 | `CustomerManagementService/src/CustomerManagementInfra/Database/UserTicketDbContextFactory.cs` | 11 | `"password"` detected here, make sure this is not a hard-coded credential. |

**Como corrigir:**

```csharp
// ❌ Não-conforme
string password = "Admin123";
string url = "scheme://user:Admin123@domain.com";

// ✅ Conforme
string password = GetEncryptedPassword();
// Ou via IConfiguration / User Secrets / Environment Variables:
string connectionString = configuration.GetConnectionString("DefaultConnection");
```

**Ação:** Mover credenciais para `dotnet user-secrets` (desenvolvimento) e variáveis de ambiente / Azure Key Vault / AWS Secrets Manager (produção). Revogar as senhas expostas **imediatamente**.

---

### 🟡 MAJOR

---

#### Regra: `csharpsquid:S125` — Sections of code should not be commented out

**Tipo:** Code Smell  
**Descrição:** Código comentado distrai da lógica executada, aumenta ruído de manutenção e rapidamente fica desatualizado. Deve ser removido; o histórico do controle de versão preserva as revisões anteriores.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementApp/Service/CustomerService.cs` | 19 | Remove this commented out code. |
| 2 | `CustomerManagementService/src/CustomerManagementAppHost/Program.cs` | 4 | Remove this commented out code. |
| 3 | `CustomerManagementService/src/CustomerManagementAppHost/Program.cs` | 6 | Remove this commented out code. |
| 4 | `CustomerManagementService/src/ServiceDefaults/Extensions.cs` | 37 | Remove this commented out code. |
| 5 | `CustomerManagementService/src/ServiceDefaults/Extensions.cs` | 84 | Remove this commented out code. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
// if (s.StartsWith('A'))
// {
//     s = s.Substring(1);
// }

// ✅ Conforme — simplesmente remover o bloco comentado
```

---

#### Regra: `csharpsquid:S3881` — Fix this implementation of `IDisposable` to conform to the dispose pattern

**Tipo:** Code Smell  
**Descrição:** A implementação de `IDisposable` não segue o padrão recomendado pela Microsoft: método `protected virtual void Dispose(bool disposing)` + `GC.SuppressFinalize(this)`.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementInfra/Database/UnitOfWork.cs` | 7 | Fix this implementation of `IDisposable` to conform to the dispose pattern. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
public class UnitOfWork : IDisposable
{
    public void Dispose() { /* lógica diretamente aqui */ }
}

// ✅ Conforme
public class UnitOfWork : IDisposable
{
    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // liberar recursos gerenciados
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

---

#### Regra: `csharpsquid:S112` — General or reserved exceptions should never be thrown

**Tipo:** Code Smell  
**Descrição:** Lançar `System.NullReferenceException` manualmente é incorreto — esta é uma exceção reservada do runtime. Deve ser usada uma exceção específica e relevante ao contexto.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementInfra/Database/UnitOfWork.cs` | 22 | `'System.NullReferenceException'` should not be thrown by user code. |
| 2 | `CustomerManagementService/src/CustomerManagementInfra/Database/UnitOfWork.cs` | 37 | `'System.NullReferenceException'` should not be thrown by user code. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
if (obj == null)
    throw new NullReferenceException("obj");

// ✅ Conforme
if (obj == null)
    throw new ArgumentNullException(nameof(obj));
```

---

#### Regra: `csharpsquid:S6966` — Awaitable method should be used

**Tipo:** Code Smell  
**Descrição:** Em um método `async`, usar um método síncrono ao invés de sua contraparte assíncrona bloqueia a thread, reduzindo escalabilidade e performance.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementApp/Program.cs` | 29 | Await `RunAsync` instead. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
app.Run();

// ✅ Conforme
await app.RunAsync();
```

---

#### Regra: `csharpsquid:S3928` — Parameter names used into ArgumentException constructors should match an existing one

**Tipo:** Code Smell  
**Descrição:** O construtor de `ArgumentException` e similares (`ArgumentNullException`, `ArgumentOutOfRangeException`) deve receber um nome de parâmetro existente na assinatura do método. Usar a sobrecarga sem parâmetros ou um nome incorreto dificulta o diagnóstico.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementDomain/Entity/UserTicket.cs` | 30 | Use a constructor overload that allows a more meaningful exception message to be provided. |

**Como corrigir:**
```csharp
// ❌ Não-conforme — construtor sem mensagem
throw new ArgumentException();

// ✅ Conforme — nome do parâmetro + mensagem descritiva
throw new ArgumentException("O valor informado é inválido.", nameof(parameterName));
```

---

### 🔵 MINOR

---

#### Regra: `csharpsquid:S1192` — String literals should not be duplicated

**Tipo:** Code Smell  
**Descrição:** Literais string duplicadas dificultam refatoração — qualquer mudança precisa ser propagada em todos os lugares. Usar constantes resolve isso.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementInfra/Migrations/20241127155959_InitialCreate.cs` | 23 | Define a constant instead of using this literal `'timestamp with time zone'` 4 times. |
| 2 | `CustomerManagementService/src/CustomerManagementInfra/Migrations/20241202000212_migratonV2.cs` | 16 | Define a constant instead of using this literal `'UserTickets'` 6 times. |
| 3 | `CustomerManagementService/src/CustomerManagementInfra/Migrations/20241202000212_migratonV2.cs` | 17 | Define a constant instead of using this literal `'timestamp with time zone'` 12 times. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
migrationBuilder.AddColumn<DateTime>(type: "timestamp with time zone", ...);
migrationBuilder.AddColumn<DateTime>(type: "timestamp with time zone", ...);

// ✅ Conforme
private const string TimestampType = "timestamp with time zone";
migrationBuilder.AddColumn<DateTime>(type: TimestampType, ...);
```

---

#### Regra: `csharpsquid:S3260` — Private record classes not derived should be marked as `sealed`

**Tipo:** Code Smell  
**Descrição:** Records privados que não são herdados devem ser marcados como `sealed` para otimização de performance e clareza de intenção.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementInfra/Broker/CustomerIntegrationBus.cs` | 15 | Private record classes which are not derived in the current assembly should be marked as `sealed`. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
private record CustomerCreatedEvent(Guid Id, string Name);

// ✅ Conforme
private sealed record CustomerCreatedEvent(Guid Id, string Name);
```

---

#### Regra: `csharpsquid:S2325` — Methods that don't access instance data should be made `static`

**Tipo:** Code Smell  
**Descrição:** Métodos que não acessam estado de instância devem ser `static` para maior clareza e leve melhoria de performance (evita passagem do ponteiro `this`).

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementInfra/Database/UserTicketDbContextFactory.cs` | 8 | Make `CreateDbContext` a static method. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
public UserTicketDbContext CreateDbContext(string[] args) { ... }

// ✅ Conforme
public static UserTicketDbContext CreateDbContext(string[] args) { ... }
```

---

## Checklist de Correções

- [ ] **[BLOCKER]** Remover senha hard-coded de `appsettings.json` linha 9 → usar `IConfiguration` / secrets
- [ ] **[BLOCKER]** Remover URI com credencial de `appsettings.json` linha 10 → usar variável de ambiente
- [ ] **[BLOCKER]** Remover senha hard-coded de `UserTicketDbContextFactory.cs` linha 11 → usar `IConfiguration`
- [ ] **[MAJOR]** Remover código comentado em `CustomerService.cs` linha 19
- [ ] **[MAJOR]** Remover código comentado em `CustomerManagementAppHost/Program.cs` linhas 4 e 6
- [ ] **[MAJOR]** Remover código comentado em `ServiceDefaults/Extensions.cs` linhas 37 e 84
- [ ] **[MAJOR]** Corrigir `IDisposable` em `UnitOfWork.cs` linha 7 → implementar padrão correto
- [ ] **[MAJOR]** Substituir `NullReferenceException` em `UnitOfWork.cs` linhas 22 e 37 → `ArgumentNullException`
- [ ] **[MAJOR]** Usar `await app.RunAsync()` em `Program.cs` linha 29
- [ ] **[MAJOR]** Usar construtor com mensagem em `UserTicket.cs` linha 30 → `throw new ArgumentException("msg", nameof(param))`
- [ ] **[MINOR]** Extrair constante `"timestamp with time zone"` em `InitialCreate.cs` linha 23
- [ ] **[MINOR]** Extrair constante `"UserTickets"` em `migratonV2.cs` linha 16
- [ ] **[MINOR]** Extrair constante `"timestamp with time zone"` em `migratonV2.cs` linha 17
- [ ] **[MINOR]** Marcar record como `sealed` em `CustomerIntegrationBus.cs` linha 15
- [ ] **[MINOR]** Marcar `CreateDbContext` como `static` em `UserTicketDbContextFactory.cs` linha 8
