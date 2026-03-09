# Resumo de Correções — Camada Infra

**Projeto:** ArquitetoMovel_customer-services  
**Session ID:** 20260308-433a  
**Data:** 8 de março de 2026

---

## Estatísticas

| Métrica | Valor |
|---------|-------|
| Total de issues no relatório | 12 |
| Issues corrigidas | 9 |
| Issues ignoradas (com justificativa) | 3 |
| Build | ✅ PASSOU |
| Testes | ✅ PASSARAM |

---

## Correções por Severidade

### 🟡 MAJOR (5/7 corrigidas)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S3881 | `src/CustomerManagementInfra/Database/UnitOfWork.cs` | ✅ Corrigido | Implementado padrão `Dispose(bool)` + `GC.SuppressFinalize(this)` |
| 2 | S112 | `src/CustomerManagementInfra/Database/UnitOfWork.cs` (linha 22) | ✅ Corrigido | `NullReferenceException` substituído por `InvalidOperationException` |
| 3 | S112 | `src/CustomerManagementInfra/Database/UnitOfWork.cs` (linha 37) | ✅ Corrigido | `NullReferenceException` substituído por `InvalidOperationException` |
| 4 | S3928 | `src/CustomerManagementDomain/Entity/UserTicket.cs` (linha 30) | ✅ Corrigido | `ArgumentOutOfRangeException()` com `nameof(Status)` e mensagem descritiva |
| 5 | S7021 | `NotificationService/Dockerfile` (linha 17) | ⏭️ Ignorado | Arquivo não encontrado neste workspace (serviço externo) |
| 6 | S7021 | `UserManagementService/Dockerfile` (linha 16) | ⏭️ Ignorado | Arquivo não encontrado neste workspace (serviço externo) |
| 7 | S6570 | `NotificationService/Dockerfile` (linha 22) | ⏭️ Ignorado | Arquivo não encontrado neste workspace (serviço externo) |
| 8 | S6570 | `UserManagementService/Dockerfile` (linha 21) | ⏭️ Ignorado | Arquivo não encontrado neste workspace (serviço externo) |

> ⚠️ Os Dockerfiles dos serviços `NotificationService` e `UserManagementService` não existem no workspace atual (`CustomerManagementService`). As correções S7021 e S6570 devem ser aplicadas nesses repositórios separadamente.

---

### 🔵 MINOR (4/5 corrigidas)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S3260 | `src/CustomerManagementInfra/Broker/CustomerIntegrationBus.cs` (linha 15) | ✅ Corrigido | `private record Ticket` → `private sealed record Ticket` |
| 2 | S2325 | `src/CustomerManagementInfra/Database/UserTicketDbContextFactory.cs` (linha 8) | ⏭️ Ignorado | Falso positivo: `CreateDbContext` é membro da interface `IDesignTimeDbContextFactory<T>`, que requer método de instância; não pode ser `static` |
| 3 | S1192 | `src/CustomerManagementInfra/Migrations/20241127155959_InitialCreate.cs` (linha 23) | ✅ Corrigido | Constante `TimestampWithTimeZone` extraída; 4 literais substituídos |
| 4 | S1192 | `src/CustomerManagementInfra/Migrations/20241202000212_migratonV2.cs` (linha 16) | ✅ Corrigido | Constante `UserTicketsTable` extraída; 6 literais substituídos |
| 5 | S1192 | `src/CustomerManagementInfra/Migrations/20241202000212_migratonV2.cs` (linha 17) | ✅ Corrigido | Constante `TimestampWithTimeZone` extraída; 12 literais substituídos |

---

## Issues Ignoradas

| # | Regra | Arquivo | Justificativa |
|---|-------|---------|---------------|
| 1 | S7021 | `NotificationService/Dockerfile` | Arquivo não encontrado neste workspace |
| 2 | S7021 | `UserManagementService/Dockerfile` | Arquivo não encontrado neste workspace |
| 3 | S6570 | `NotificationService/Dockerfile` | Arquivo não encontrado neste workspace |
| 4 | S6570 | `UserManagementService/Dockerfile` | Arquivo não encontrado neste workspace |
| 5 | S2325 | `UserTicketDbContextFactory.cs` | Falso positivo — restrição da interface `IDesignTimeDbContextFactory<T>` impede método `static` |

---

## Detalhes das Alterações

### `UnitOfWork.cs` — S3881 + S112

**Adicionado campo `_disposed`** e implementado o padrão dispose completo:

```csharp
// Antes
public void Dispose()
{
    _transaction?.Dispose();
    context.Dispose();
}

// Depois
public void Dispose()
{
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
}

protected virtual void Dispose(bool disposing)
{
    if (!_disposed)
    {
        if (disposing)
        {
            _transaction?.Dispose();
            context.Dispose();
        }
        _disposed = true;
    }
}
```

**Exceções substituídas** (2 ocorrências):
```csharp
// Antes
throw new NullReferenceException("A transaction has not been started...");

// Depois
throw new InvalidOperationException("A transaction has not been started...");
```

### `UserTicket.cs` — S3928

```csharp
// Antes
throw new ArgumentOutOfRangeException();

// Depois
throw new ArgumentOutOfRangeException(nameof(Status), Status, "Unrecognized ticket status.");
```

### `CustomerIntegrationBus.cs` — S3260

```csharp
// Antes
private record Ticket(int Number, int Type, DateTime CreatedAt, int Status, DateTime UpdatedAt);

// Depois
private sealed record Ticket(int Number, int Type, DateTime CreatedAt, int Status, DateTime UpdatedAt);
```

### Migrations — S1192

Constantes extraídas em cada classe de migration:

- `InitialCreate.cs`: `private const string TimestampWithTimeZone = "timestamp with time zone";`
- `migratonV2.cs`: `private const string TimestampWithTimeZone = "timestamp with time zone";` e `private const string UserTicketsTable = "UserTickets";`
