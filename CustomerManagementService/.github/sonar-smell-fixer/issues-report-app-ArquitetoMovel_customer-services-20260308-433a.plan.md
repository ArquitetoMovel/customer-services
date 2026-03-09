# Relatório da Camada de Aplicação e API — ArquitetoMovel_customer-services

**Data:** 8 de março de 2026
**Session ID:** 20260308-433a
**Camada:** Aplicação, API e UI (Application, Controllers, Programs, UI)

---

## Resumo

| Categoria              | Quantidade |
|------------------------|-----------|
| Issues CRITICAL        | 2          |
| Issues MAJOR           | 13         |
| Issues MINOR           | 1          |
| **Total desta camada** | **16**     |

---

## Issues por Severidade

### 🟠 CRITICAL

---

#### Regra: `csharpsquid:S3776` — Complexidade Cognitiva deve ser reduzida

**Descrição:**  
A complexidade cognitiva mede o quão difícil é entender o fluxo de um método. Ao contrário da complexidade ciclomática, ela penaliza estruturas de controle aninhadas e incrementos de complexidade não-linearmente. Um método com complexidade cognitiva acima de 15 torna-se difícil de ler, testar e manter.

**Atributo de Código Limpo:** FOCUSED (categoria ADAPTABLE)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/src/NotificationService.Infrastructure/MessageBroker/RabbitMqMessageBroker.cs` | 23 | Refactor this method to reduce its Cognitive Complexity from 16 to the 15 allowed. |

**Como corrigir:**

Técnicas para reduzir complexidade cognitiva:
1. **Extração de método:** Mover blocos lógicos para métodos privados com nomes descritivos
2. **Guard clauses:** Retornar cedo (early return) ao invés de `if/else` profundamente aninhados
3. **Decomposição de condições complexas:** Extrair expressões booleanas para variáveis com nomes descritivos

```csharp
// ❌ NÃO CONFORME — Alta complexidade aninhada
public void ProcessMessage(Message msg)
{
    if (msg != null)
    {
        if (msg.IsValid)
        {
            if (msg.Type == "notification")
            {
                // ... lógica profundamente aninhada
                foreach (var recipient in msg.Recipients)
                {
                    if (recipient.IsActive)
                    {
                        // ainda mais lógica...
                    }
                }
            }
        }
    }
}

// ✅ CONFORME — Complexidade reduzida com guard clauses e extração de método
public void ProcessMessage(Message msg)
{
    if (msg is null || !msg.IsValid) return;
    if (msg.Type != "notification") return;

    SendToActiveRecipients(msg.Recipients);
}

private void SendToActiveRecipients(IEnumerable<Recipient> recipients)
{
    foreach (var recipient in recipients.Where(r => r.IsActive))
    {
        Send(recipient);
    }
}
```

---

#### Regra: `csharpsquid:S2696` — Métodos de instância não devem escrever em campos `static`

**Descrição:**  
Atualizar um campo `static` a partir de um método não-`static` introduz problemas sérios de concorrência. Múltiplas instâncias da classe e múltiplas threads podem acessar e modificar o campo `static` simultaneamente, causando comportamento inesperado, condições de corrida e problemas de sincronização.

**Atributo de Código Limpo:** COMPLETE (categoria INTENTIONAL)  
**Standards:** STIG V-222567 — Application must not be vulnerable to race conditions

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 2 | `UserManagementService/src/UserManagement.Application/Services/AttendanceTicketService.cs` | 25 | Remove this set, which updates a 'static' field from an instance method. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Campo static atualizado por método de instância
class AttendanceTicketService
{
    private static int count = 0;

    public void CreateTicket(TicketRequest request)
    {
        // ...
        count++; // Noncompliant: condição de corrida em ambiente multi-thread
    }
}

// ✅ OPÇÃO 1 — Tornar o método e campo static (se fizer sentido semântico)
class AttendanceTicketService
{
    private static int count = 0;

    public static void IncrementCount()
    {
        Interlocked.Increment(ref count); // Thread-safe
    }
}

// ✅ OPÇÃO 2 — Tornar o campo de instância (preferível na maioria dos casos)
class AttendanceTicketService
{
    private int count = 0; // Campo de instância — cada instância tem seu próprio

    public void CreateTicket(TicketRequest request)
    {
        count++;
    }
}
```

---

### 🟡 MAJOR

---

#### Regra: `csharpsquid:S6966` — Método aguardável deve ser usado (`await RunAsync`)

**Descrição:**  
Em um método `async`, operações bloqueantes devem ser evitadas. Usar o método síncrono `Run()` em vez de `RunAsync()` bloqueia a thread, reduz a escalabilidade e degrada a performance. O .NET fornece equivalentes assíncronos para todas as operações bloqueantes.

**Atributo de Código Limpo:** COMPLETE (categoria INTENTIONAL)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementApp/Program.cs` | 29 | Await RunAsync instead. |
| 2 | `NotificationService/src/NotificationService.Api/Program.cs` | 31 | Await RunAsync instead. |
| 3 | `UserManagementService/src/UserManagement.Api/Program.cs` | 54 | Await RunAsync instead. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Método síncrono em contexto assíncrono
app.Run(); // Noncompliant

// ✅ CONFORME — Usar equivalente assíncrono
await app.RunAsync();
```

---

#### Regra: `csharpsquid:S2629` — Não usar interpolação de string em templates de log

**Descrição:**  
Usar interpolação de string (`$"..."`) em mensagens de log força a concatenação da string mesmo quando o nível de log está desabilitado, gerando overhead desnecessário. Os frameworks de log modernos usam templates com parâmetros para avaliação lazy.

**Atributo de Código Limpo:** EFFICIENT (categoria INTENTIONAL)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 4 | `NotificationService/src/NotificationService.Application/NotificationService.cs` | 36 | Don't use string interpolation in logging message templates. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Interpolação de string (sempre avalia a expressão)
_logger.LogInformation($"Processing notification for {userId}");

// ❌ NÃO CONFORME — Concatenação de string
_logger.LogInformation("Processing notification for " + userId);

// ✅ CONFORME — Template com parâmetros (avaliação lazy, permite structured logging)
_logger.LogInformation("Processing notification for {UserId}", userId);
```

---

#### Regra: `csharpsquid:S6960` — Controller com múltiplas responsabilidades

**Descrição:**  
Um controller que gerencia mais de um recurso ou lida com responsabilidades distintas viola o Princípio da Responsabilidade Única (SRP). Isso dificulta o teste unitário, aumenta o acoplamento e torna o código mais difícil de manter.

**Atributo de Código Limpo:** MODULAR (categoria ADAPTABLE)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 5 | `UserManagementService/src/UserManagement.Api/Controllers/AttendanceController.cs` | 9 | This controller has multiple responsibilities and could be split into 2 smaller controllers. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Controller com múltiplas responsabilidades
[ApiController]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    // Endpoints de Tickets
    [HttpGet("tickets")] public IActionResult GetTickets() { ... }
    [HttpPost("tickets")] public IActionResult CreateTicket() { ... }

    // Endpoints de Users (responsabilidade diferente)
    [HttpGet("users")] public IActionResult GetUsers() { ... }
}

// ✅ CONFORME — Controllers separados por responsabilidade
[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    [HttpGet] public IActionResult GetTickets() { ... }
    [HttpPost] public IActionResult CreateTicket() { ... }
}

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    [HttpGet] public IActionResult GetUsers() { ... }
}
```

---

#### Regra: `csharpsquid:S125` — Código comentado deve ser removido

**Descrição:**  
Código comentado distrai do código real executado, cria ruído que aumenta o custo de manutenção e, por nunca ser executado, rapidamente se torna obsoleto e inválido. O histórico de controle de versão já preserva o código removido.

**Atributo de Código Limpo:** CLEAR (categoria INTENTIONAL)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 6 | `CustomerManagementService/src/CustomerManagementApp/Service/CustomerService.cs` | 19 | Remove this commented out code. |
| 7 | `CustomerManagementService/src/CustomerManagementAppHost/Program.cs` | 4 | Remove this commented out code. |
| 8 | `CustomerManagementService/src/CustomerManagementAppHost/Program.cs` | 6 | Remove this commented out code. |
| 9 | `CustomerManagementService/src/ServiceDefaults/Extensions.cs` | 37 | Remove this commented out code. |
| 10 | `CustomerManagementService/src/ServiceDefaults/Extensions.cs` | 84 | Remove this commented out code. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Código comentado
void Method(string s)
{
    // if (s.StartsWith('A'))
    // {
    //     s = s.Substring(1);
    // }
    // Do something...
}

// ✅ CONFORME — Apenas código ativo
void Method(string s)
{
    // Do something...
}
```

---

#### Regra: `css:S7924` — Texto e cores de fundo devem ter contraste suficiente

**Descrição:**  
A razão de contraste de cor é essencial para acessibilidade web. Quando o texto não tem contraste suficiente com o fundo, torna-se difícil ou impossível para usuários com deficiência visual lê-lo. O padrão WCAG AA exige contraste mínimo de 4.5:1 para texto normal.

**Atributo de Código Limpo:** CONVENTIONAL (categoria CONSISTENT)  
**Standards:** WCAG 2.2 — 1.4.3 Contrast (Minimum)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 11 | `UserManagementService/src/UserManagement.UI/Layout/NavMenu.razor.css` | 60 | Text does not meet the minimal contrast requirement with its background. |
| 12 | `UserManagementService/src/UserManagement.UI/Layout/NavMenu.razor.css` | 65 | Text does not meet the minimal contrast requirement with its background. |

**Como corrigir:**

```css
/* ❌ NÃO CONFORME — Contraste insuficiente (4.48:1) */
.nav-item {
  color: #777777;
  background-color: #ffffff;
}

/* ✅ CONFORME — Contraste adequado (7.0:1) */
.nav-item {
  color: #595959;
  background-color: #ffffff;
}
```

> 💡 Use a ferramenta [WebAIM Color Contrast Checker](https://webaim.org/resources/contrastchecker/) para verificar o contraste antes de definir as cores.

---

### 🔵 MINOR

---

#### Regra: `csharpsquid:S1075` — URIs não devem ser hard-coded

**Descrição:**  
URIs hard-coded dificultam testes, não são portáveis entre ambientes e podem conter informações sensíveis como endereços IP. Devem ser substituídos por parâmetros configuráveis.

**Atributo de Código Limpo:** CONVENTIONAL (categoria CONSISTENT)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `UserManagementService/src/UserManagement.UI/Program.cs` | 9 | Refactor your code not to use hardcoded absolute paths or URIs. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME
builder.Services.AddHttpClient("MyApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000"); // Noncompliant
});

// ✅ CONFORME — Usar configuração
builder.Services.AddHttpClient("MyApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});
```

---

## Conclusão desta Camada

**Prioridade de correções:**

1. **🟠 CRITICAL — `AttendanceTicketService.cs` (linha 25):** Campo `static` atualizado por método de instância — risco de condição de corrida em produção. Corrigir antes da próxima release.
2. **🟠 CRITICAL — `RabbitMqMessageBroker.cs` (linha 23):** Reduzir complexidade cognitiva de 16 para ≤15 — impacta diretamente a manutenibilidade do message broker.
3. **🟡 MAJOR — `RunAsync` nos 3 `Program.cs`:** Alteração simples e de baixo risco, mas melhora a escalabilidade do servidor.
4. **🟡 MAJOR — `AttendanceController.cs` (linha 9):** Dividir o controller em responsabilidades menores.
5. **🟡 MAJOR — Código comentado (5 ocorrências):** Limpar o código — remover blocos comentados.
6. **🟡 MAJOR — Log com interpolação de string:** Impacto de performance em ambiente com alto volume de logs.
7. **🟡 MAJOR — Contraste CSS (2 ocorrências):** Corrigir para compliance com acessibilidade WCAG.
8. **🔵 MINOR — URI hard-coded no `Program.cs` da UI:** Mover para configuração.
