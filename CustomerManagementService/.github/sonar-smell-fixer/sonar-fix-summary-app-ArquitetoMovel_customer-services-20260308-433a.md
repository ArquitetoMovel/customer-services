# Resumo de Correções — ArquitetoMovel_customer-services

**Report:** `issues-report-app-ArquitetoMovel_customer-services-20260308-433a.plan.md`  
**Session ID:** 20260308-433a  
**Camada:** Aplicação, API e UI  
**Data de execução:** 8 de março de 2026

---

## Estatísticas

| Item | Valor |
|------|-------|
| Total de issues no relatório | 16 |
| Issues neste workspace | 5 |
| Issues corrigidas | 5 |
| Issues ignoradas (fora do workspace) | 11 |
| Build | ✅ PASSOU |
| Testes | ✅ PASSARAM |

---

## Correções por Severidade

### 🟡 MAJOR (4/4 corrigidas neste workspace)

| # | Regra | Arquivo | Linha | Status | Observação |
|---|-------|---------|-------|--------|------------|
| 1 | S6966 | `src/CustomerManagementApp/Program.cs` | 29 | ✅ Corrigido | `app.Run()` → `await app.RunAsync()` |
| 2 | S125 | `src/CustomerManagementApp/Service/CustomerService.cs` | 19 | ✅ Corrigido | Removido `// await unitOfWork.BeginTransactionAsync();` |
| 3 | S125 | `src/CustomerManagementAppHost/Program.cs` | 4, 6 | ✅ Corrigido | Removido bloco comentado `rabbitmq` (3 linhas) |
| 4 | S125 | `src/ServiceDefaults/Extensions.cs` | 37, 84 | ✅ Corrigido | Removidos 2 blocos comentados: `ServiceDiscoveryOptions` e Azure Monitor |

---

## Issues Fora deste Workspace (Ignoradas)

As issues abaixo pertencem a outros serviços do repositório monorepo e não estão no escopo do workspace `CustomerManagementService`:

| # | Regra | Serviço / Arquivo | Justificativa |
|---|-------|-------------------|---------------|
| 1 | S3776 | `NotificationService/.../RabbitMqMessageBroker.cs` | Pertence ao `NotificationService` |
| 2 | S2696 | `UserManagementService/.../AttendanceTicketService.cs` | Pertence ao `UserManagementService` |
| 3 | S6966 | `NotificationService/.../Program.cs` | Pertence ao `NotificationService` |
| 4 | S6966 | `UserManagementService/.../Program.cs` | Pertence ao `UserManagementService` |
| 5 | S2629 | `NotificationService/.../NotificationService.cs` | Pertence ao `NotificationService` |
| 6 | S6960 | `UserManagementService/.../AttendanceController.cs` | Pertence ao `UserManagementService` |
| 7 | S7924 | `UserManagementService/.../NavMenu.razor.css` (linha 60) | Pertence ao `UserManagementService` |
| 8 | S7924 | `UserManagementService/.../NavMenu.razor.css` (linha 65) | Pertence ao `UserManagementService` |
| 9 | S1075 | `UserManagementService/.../Program.cs` | Pertence ao `UserManagementService` |

---

## Detalhes das Correções

### Fix 1 — S6966: `app.Run()` → `await app.RunAsync()`

**Arquivo:** [src/CustomerManagementApp/Program.cs](../../src/CustomerManagementApp/Program.cs)

```diff
- app.Run();
+ await app.RunAsync();
```

**Motivo:** Em contextos assíncronos (top-level statements em .NET), `app.RunAsync()` deve ser aguardado para não bloquear a thread do servidor.

---

### Fix 2 — S125: Código comentado em `CustomerService.cs`

**Arquivo:** [src/CustomerManagementApp/Service/CustomerService.cs](../../src/CustomerManagementApp/Service/CustomerService.cs)

```diff
  private async Task<List<UserTicket>?> GetAndUpdateNextCustomers()
  {
-    //  await unitOfWork.BeginTransactionAsync();
-
     try
```

**Motivo:** Linha de código comentado sem utilidade. O histórico Git preserva o código caso precise ser recuperado.

---

### Fix 3 — S125: Código comentado em `CustomerManagementAppHost/Program.cs`

**Arquivo:** [src/CustomerManagementAppHost/Program.cs](../../src/CustomerManagementAppHost/Program.cs)

```diff
  var builder = DistributedApplication.CreateBuilder(args);
- //var rabbitmq = builder.AddRabbitMQ("messaging")
- //    .WithManagementPlugin();
  builder.AddProject<Projects.CustomerManagementApp>("CustomerManagementApp");
- //    .WithReference(rabbitmq);
  builder.Build().Run();
```

**Motivo:** Bloco de código RabbitMQ comentado — não está em uso.

---

### Fix 4 — S125: Dois blocos comentados em `Extensions.cs`

**Arquivo:** [src/ServiceDefaults/Extensions.cs](../../src/ServiceDefaults/Extensions.cs)

**Bloco 1 (linha 37) — ServiceDiscoveryOptions:**
```diff
-         // Uncomment the following to restrict the allowed schemes for service discovery.
-         // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
-         // {
-         //     options.AllowedSchemes = ["https"];
-         // });
-
          return builder;
```

**Bloco 2 (linha 84) — Azure Monitor Exporter:**
```diff
-         // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
-         //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
-         //{
-         //    builder.Services.AddOpenTelemetry()
-         //       .UseAzureMonitor();
-         //}
-
          return builder;
```

**Motivo:** Blocos de código comentados sem utilidade ativa. Podem ser adicionados via referência à documentação oficial quando necessários.
