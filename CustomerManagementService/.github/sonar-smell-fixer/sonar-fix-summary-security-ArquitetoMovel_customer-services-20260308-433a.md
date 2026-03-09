# Resumo de Correções de Segurança

**Projeto:** ArquitetoMovel_customer-services  
**Session ID:** 20260308-433a  
**Camada:** Segurança (BLOCKER + Security Hotspots)  
**Data da correção:** 8 de março de 2026  
**Escopo:** `CustomerManagementService` (workspace atual)

---

## Estatísticas

| Métrica | Valor |
|---------|-------|
| Total de issues no relatório | 12 (6 BLOCKER + 6 Security Hotspots) |
| Issues dentro do escopo deste workspace | 3 |
| Issues corrigidas | 3 |
| Issues fora do escopo (outros serviços) | 9 |
| Build | ✅ PASSOU |
| Testes | ✅ PASSOU (sem projetos de teste na solução) |

---

## Correções por Severidade

### 🔴 BLOCKER (3/3 corrigidas dentro do escopo)

| # | Regra | Arquivo | Linha | Status | Observação |
|---|-------|---------|-------|--------|------------|
| 1 | S2068 | `src/CustomerManagementApp/appsettings.json` | 9 | ✅ Corrigido | Senha removida da connection string `customer_db`; valor movido para `appsettings.Development.json` (gitignored) |
| 2 | S2068 | `src/CustomerManagementApp/appsettings.json` | 10 | ✅ Corrigido | URI com credencial removida da connection string `customer_broker`; valor movido para `appsettings.Development.json` (gitignored) |
| 3 | S2068 | `src/CustomerManagementInfra/Database/UserTicketDbContextFactory.cs` | 11 | ✅ Corrigido | Connection string hard-coded substituída por `Environment.GetEnvironmentVariable("ConnectionStrings__customer_db")` |
| 4 | S2068 | `NotificationService/src/NotificationService.Api/appsettings.json` | 9 | ⚠️ Fora do escopo | Arquivo pertence ao serviço `NotificationService`, não ao workspace atual |
| 5 | S2068 | `NotificationService/src/NotificationService.Infrastructure/Persistence/NotificationDbContextFactory.cs` | 11 | ⚠️ Fora do escopo | Arquivo pertence ao serviço `NotificationService`, não ao workspace atual |
| 6 | S6703 | `NotificationService/.idea/.idea.NotificationService/.idea/dataSources.xml` | 9 | ⚠️ Fora do escopo | Arquivo pertence ao serviço `NotificationService`, não ao workspace atual |

---

### 🔒 Security Hotspots (0/6 corrigidas dentro do escopo)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S6470 | `NotificationService/Dockerfile` | ⚠️ Fora do escopo | Arquivo pertence ao serviço `NotificationService` |
| 2 | S6470 | `UserManagementService/Dockerfile` | ⚠️ Fora do escopo | Arquivo pertence ao serviço `UserManagementService` |
| 3 | S6471 | `NotificationService/Dockerfile` | ⚠️ Fora do escopo | Arquivo pertence ao serviço `NotificationService` |
| 4 | S6471 | `UserManagementService/Dockerfile` | ⚠️ Fora do escopo | Arquivo pertence ao serviço `UserManagementService` |
| 5 | S2612 | `NotificationService/Dockerfile` | ⚠️ Fora do escopo | Arquivo pertence ao serviço `NotificationService` |
| 6 | S5122 | `UserManagementService/src/UserManagement.Api/Program.cs` | ⚠️ Fora do escopo | Arquivo pertence ao serviço `UserManagementService` |

---

## Detalhes das Correções

### Fix 1 & 2 — `appsettings.json` (S2068)

**Antes:**
```json
"ConnectionStrings": {
  "customer_db": "Host=localhost:15432;Database=customerdb;Username=nuser;Password=npass1",
  "customer_broker": "amqp://admin:adminpassword@localhost:5672"
}
```

**Depois:**
```json
"ConnectionStrings": {
  "customer_db": "",
  "customer_broker": ""
}
```

Os valores de desenvolvimento foram movidos para `appsettings.Development.json`, que agora está coberto pelo `.gitignore` raiz do repositório. Em produção, as connection strings devem ser fornecidas via variáveis de ambiente (`ConnectionStrings__customer_db`, `ConnectionStrings__customer_broker`) ou gerenciadores de segredos (Azure Key Vault, AWS Secrets Manager).

---

### Fix 3 — `UserTicketDbContextFactory.cs` (S2068)

**Antes:**
```csharp
optionsBuilder.UseNpgsql("Host=localhost:15432;Database=customerdb;Username=nuser;Password=npass1");
```

**Depois:**
```csharp
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__customer_db")
    ?? throw new InvalidOperationException(
        "Connection string not configured. Set the 'ConnectionStrings__customer_db' environment variable.");

optionsBuilder.UseNpgsql(connectionString);
```

A connection string agora é lida da variável de ambiente `ConnectionStrings__customer_db`. Isso é compatível com o padrão de configuração do .NET (override via env var) e com a utilização pelo `dotnet ef migrations`.

---

### Fix 4 — `.gitignore` (medida preventiva)

Adicionadas as seguintes entradas ao `.gitignore` raiz do repositório (`/Users/alexandre/Developer/customer-services/.gitignore`):

```gitignore
# ASP.NET Core — Sensitive local configuration (never commit credentials)
**/appsettings.Development.json
**/.idea/dataSources.xml
**/.idea/dataSources.local.xml
```

---

## Issues Fora do Escopo (outros serviços)

Os seguintes serviços contêm issues de segurança que devem ser corrigidas em seus respectivos workspaces:

| Serviço | Issues |
|---------|--------|
| `NotificationService` | S2068 em `appsettings.json` e `NotificationDbContextFactory.cs`; S6703 em `dataSources.xml`; Hotspots Docker S6470, S6471, S2612 |
| `UserManagementService` | Hotspot Docker S6470, S6471; Hotspot CORS S5122 |

> ⚠️ **AÇÃO IMEDIATA NECESSÁRIA:** Todas as senhas expostas nos commits anteriores devem ser **rotacionadas imediatamente**, independentemente das correções de código aplicadas. Utilize `BFG Repo Cleaner` ou `git filter-branch` para remover os segredos do histórico do repositório Git.
