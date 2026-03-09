## Resumo de Correções

**Projeto:** `ArquitetoMovel_customer-services`  
**Sessão:** `20260308-df96`  
**Serviço:** `CustomerManagementService`  
**Data:** 8 de março de 2026

---

### Estatísticas
- Total de issues no relatório: 18
- Issues corrigidas: 18
- Issues ignoradas (com justificativa): 0
- Build: ✅ PASSOU
- Testes: N/A (nenhum projeto de testes encontrado)

---

### Correções por Severidade

#### 🔴 BLOCKER (3/3 corrigidas)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 1 | S2068 | `src/CustomerManagementApp/appsettings.json` linha 9 | ✅ Corrigido | Senha removida da connection string → `Password=` (valor via env `ConnectionStrings__customer_db`) |
| 2 | S2068 | `src/CustomerManagementApp/appsettings.json` linha 10 | ✅ Corrigido | URI do broker substituída por `amqp://localhost:5672` sem credenciais embutidas |
| 3 | S2068 | `src/CustomerManagementInfra/Database/UserTicketDbContextFactory.cs` linha 11 | ✅ Corrigido | Connection string hard-coded substituída por `Environment.GetEnvironmentVariable("ConnectionStrings__customer_db")` |

#### 🟡 MAJOR (10/10 corrigidas)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 4 | S125 | `src/CustomerManagementApp/Service/CustomerService.cs` linha 19 | ✅ Corrigido | Removida linha `//  await unitOfWork.BeginTransactionAsync();` |
| 5 | S125 | `src/CustomerManagementAppHost/Program.cs` linha 4 | ✅ Corrigido | Removidas linhas comentadas do RabbitMQ (`//var rabbitmq = builder.AddRabbitMQ(...)` e `//    .WithManagementPlugin()`) |
| 6 | S125 | `src/CustomerManagementAppHost/Program.cs` linha 6 | ✅ Corrigido | Removida linha comentada `//    .WithReference(rabbitmq)` |
| 7 | S125 | `src/ServiceDefaults/Extensions.cs` linha 37 | ✅ Corrigido | Removido bloco comentado de `ServiceDiscoveryOptions` (linhas 35–39) |
| 8 | S125 | `src/ServiceDefaults/Extensions.cs` linha 84 | ✅ Corrigido | Removido bloco comentado do Azure Monitor exporter (linhas 82–87) |
| 9 | S3881 | `src/CustomerManagementInfra/Database/UnitOfWork.cs` linha 7 | ✅ Corrigido | Implementado padrão IDisposable completo com `Dispose(bool disposing)` e `GC.SuppressFinalize(this)` |
| 10 | S112 | `src/CustomerManagementInfra/Database/UnitOfWork.cs` linha 22 | ✅ Corrigido | `NullReferenceException` substituída por `InvalidOperationException` (exceção adequada para estado inválido da instância) |
| 11 | S112 | `src/CustomerManagementInfra/Database/UnitOfWork.cs` linha 37 | ✅ Corrigido | `NullReferenceException` substituída por `InvalidOperationException` |
| 12 | S6966 | `src/CustomerManagementApp/Program.cs` linha 29 | ✅ Corrigido | `app.Run()` → `await app.RunAsync()` |
| 13 | S3928 | `src/CustomerManagementDomain/Entity/UserTicket.cs` linha 30 | ✅ Corrigido | `throw new ArgumentOutOfRangeException()` → `throw new ArgumentOutOfRangeException(nameof(Status), $"Unexpected status value: {Status}")` |

#### 🔵 MINOR (5/5 corrigidas)

| # | Regra | Arquivo | Status | Observação |
|---|-------|---------|--------|------------|
| 14 | S1192 | `src/CustomerManagementInfra/Migrations/20241127155959_InitialCreate.cs` linha 23 | ✅ Corrigido | Extraída constante `TimestampWithTimeZone = "timestamp with time zone"` e aplicada às 4 ocorrências |
| 15 | S1192 | `src/CustomerManagementInfra/Migrations/20241202000212_migratonV2.cs` linha 16 | ✅ Corrigido | Extraída constante `UserTicketsTable = "UserTickets"` e aplicada às 6 ocorrências |
| 16 | S1192 | `src/CustomerManagementInfra/Migrations/20241202000212_migratonV2.cs` linha 17 | ✅ Corrigido | Extraída constante `TimestampWithTimeZone = "timestamp with time zone"` e aplicada às 12 ocorrências |
| 17 | S3260 | `src/CustomerManagementInfra/Broker/CustomerIntegrationBus.cs` linha 15 | ✅ Corrigido | `private record Ticket` → `private sealed record Ticket` |
| 18 | S2325 | `src/CustomerManagementInfra/Database/UserTicketDbContextFactory.cs` linha 8 | ✅ Corrigido | Adicionada implementação explícita da interface + método `public static CreateDbContext` |

---

### Issues Ignoradas

Nenhuma issue foi ignorada. Todas as 18 issues foram corrigidas.

---

### Notas Técnicas

- **S112 / `NullReferenceException` em `UnitOfWork`:** Como `_transaction` é um campo privado de instância (não um parâmetro de método), a substituição mais semanticamente correta é `InvalidOperationException` em vez de `ArgumentNullException`. Isso comunica claramente que o método foi chamado em estado inválido ("transação não iniciada").
- **S2325 / `CreateDbContext` estático:** A interface `IDesignTimeDbContextFactory<T>` define o método como instância; foi adicionada uma implementação explícita de interface que delega para o método estático, satisfazendo ambas as restrições.
- **S2068 / Credenciais removidas de `appsettings.json`:** Os valores de produção/desenvolvimento devem ser fornecidos via variáveis de ambiente (`ConnectionStrings__customer_db`, `ConnectionStrings__customer_broker`) ou `dotnet user-secrets`. As senhas expostas anteriormente devem ser **revogadas imediatamente**.
