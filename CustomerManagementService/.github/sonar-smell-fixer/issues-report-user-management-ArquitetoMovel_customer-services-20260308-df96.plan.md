# Relatório SonarQube — UserManagementService
## Projeto: `ArquitetoMovel_customer-services` · Sessão: `20260308-df96`

**Data:** 8 de março de 2026  
**Serviço:** `UserManagementService/`  
**Issues:** 9 abertas (1 BLOCKER · 1 CRITICAL · 6 MAJOR · 1 MINOR)  
**Hotspots:** 3 (todos com status `TO_REVIEW`)

---

## Resumo por Regra

| Regra | Descrição | Severidade | Ocorrências |
|-------|-----------|------------|-------------|
| `csharpsquid:S2699` | Test without assertion | 🔴 BLOCKER | 1 |
| `csharpsquid:S2696` | Static field written by instance method | 🟠 CRITICAL | 1 |
| `docker:S7021` | WORKDIR with relative path | 🟡 MAJOR | 1 |
| `docker:S6570` | Variable not double-quoted | 🟡 MAJOR | 1 |
| `csharpsquid:S6960` | Controller with multiple responsibilities | 🟡 MAJOR | 1 |
| `csharpsquid:S6966` | Use awaitable method | 🟡 MAJOR | 1 |
| `css:S7924` | Text/background insufficient contrast | 🟡 MAJOR | 2 |
| `csharpsquid:S1075` | Hard-coded URI | 🔵 MINOR | 1 |

---

## Issues por Severidade

### 🔴 BLOCKER

---

#### Regra: `csharpsquid:S2699` — Tests should include assertions

**Tipo:** Code Smell  
**Descrição:** Um teste de integração sem nenhuma asserção não verifica nada. Sempre passa, independente do comportamento do sistema, fornecendo falsa confiança na qualidade do software.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `UserManagementService/tests/UserManagement.IntegrationTests/UnitTest1.cs` | 6 | Add at least one assertion to this test case. |

**Como corrigir:**
```csharp
// ❌ Não-conforme — teste vazio
[Fact]
public async Task Test1()
{
    // sem Assert
}

// ✅ Conforme — cenário de integração real com asserção
[Fact]
public async Task GetAttendanceTickets_WhenUserExists_ShouldReturn200()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/attendance-tickets?userId=some-user");

    // Assert
    response.EnsureSuccessStatusCode();
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

---

### 🟠 CRITICAL

---

#### Regra: `csharpsquid:S2696` — Instance members should not write to `static` fields

**Tipo:** Code Smell  
**Descrição:** Atualizar um campo `static` a partir de um método de instância cria problemas de concorrência sérios: múltiplas instâncias da classe e threads podem modificar o campo simultaneamente, resultando em race conditions e comportamento imprevisível. STIG V-222567.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `UserManagementService/src/UserManagement.Application/Services/AttendanceTicketService.cs` | 25 | Remove this set, which updates a `static` field from an instance method. |

**Como corrigir:**
```csharp
// ❌ Não-conforme — campo static modificado por método de instância
public class AttendanceTicketService
{
    private static int _ticketCount = 0;

    public void CreateTicket(AttendanceTicket ticket)
    {
        // ...
        _ticketCount++; // ← race condition!
    }
}

// ✅ Opção 1 — remover o estado estático e usar instância ou injeção
public class AttendanceTicketService
{
    private int _ticketCount = 0; // estado de instância

    public void CreateTicket(AttendanceTicket ticket)
    {
        _ticketCount++;
    }
}

// ✅ Opção 2 — se o contador deve ser compartilhado, usar Interlocked para thread safety
private static int _ticketCount = 0;

public void CreateTicket(AttendanceTicket ticket)
{
    Interlocked.Increment(ref _ticketCount);
}

// ✅ Opção 3 — tornar o método estático se não acessa estado de instância
public static void CreateTicket(AttendanceTicket ticket)
{
    _ticketCount++;
}
```

---

### 🟡 MAJOR

---

#### Regra: `docker:S7021` — WORKDIR instruction should only be used with absolute path

**Tipo:** Code Smell  
**Descrição:** Caminhos relativos no `WORKDIR` podem causar comportamento inesperado se o diretório de trabalho for alterado por instruções anteriores. Caminhos absolutos garantem clareza e reprodutibilidade.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `UserManagementService/Dockerfile` | 16 | Use an absolute path instead of this relative path when defining the WORKDIR. |

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
**Descrição:** Variáveis não delimitadas por aspas duplas em instruções shell estão sujeitas a globbing e word splitting, podendo produzir comportamento errático em valores com espaços ou caracteres especiais.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `UserManagementService/Dockerfile` | 21 | Surround this variable with double quotes; otherwise, it can lead to unexpected behavior. |

**Como corrigir:**
```dockerfile
# ❌ Não-conforme
RUN echo $MY_VAR

# ✅ Conforme
RUN echo "$MY_VAR"
```

---

#### Regra: `csharpsquid:S6960` — Controllers should not have too many responsibilities

**Tipo:** Code Smell  
**Descrição:** Um controller com múltiplas responsabilidades viola o Princípio da Responsabilidade Única (SRP). Isso dificulta testes unitários, aumenta o tamanho da classe e torna o código mais difícil de manter. O SonarQube detectou que este controller poderia ser dividido em 2 controllers menores.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `UserManagementService/src/UserManagement.Api/Controllers/AttendanceController.cs` | 9 | This controller has multiple responsibilities and could be split into 2 smaller controllers. |

**Como corrigir:**
```csharp
// ❌ Não-conforme — controller gerenciando dois domínios distintos
[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    // Endpoints de attendance tickets
    [HttpGet("tickets")] public IActionResult GetTickets() { ... }
    [HttpPost("tickets")] public IActionResult CreateTicket() { ... }

    // Endpoints de outro domínio misturado
    [HttpGet("schedules")] public IActionResult GetSchedules() { ... }
    [HttpPost("schedules")] public IActionResult CreateSchedule() { ... }
}

// ✅ Conforme — controllers separados por responsabilidade
[ApiController]
[Route("api/attendance-tickets")]
public class AttendanceTicketsController : ControllerBase
{
    [HttpGet] public IActionResult GetTickets() { ... }
    [HttpPost] public IActionResult CreateTicket() { ... }
}

[ApiController]
[Route("api/schedules")]
public class SchedulesController : ControllerBase
{
    [HttpGet] public IActionResult GetSchedules() { ... }
    [HttpPost] public IActionResult CreateSchedule() { ... }
}
```

---

#### Regra: `csharpsquid:S6966` — Awaitable method should be used

**Tipo:** Code Smell  
**Descrição:** Usar método síncrono `Run()` em lugar de `RunAsync()` em contexto assíncrono bloqueia a thread, prejudicando escalabilidade.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `UserManagementService/src/UserManagement.Api/Program.cs` | 54 | Await `RunAsync` instead. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
app.Run();

// ✅ Conforme
await app.RunAsync();
```

---

#### Regra: `css:S7924` — Text and background colors should have sufficient contrast

**Tipo:** Code Smell (Acessibilidade)  
**Descrição:** Texto com contraste insuficiente contra o fundo viola as diretrizes WCAG 2.2 AA, tornando o conteúdo inacessível para usuários com deficiência visual, daltonismo ou visão baixa. Requisito mínimo: 4.5:1 para texto normal e 3:1 para texto grande (≥18pt ou ≥14pt bold).

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `UserManagementService/src/UserManagement.UI/Layout/NavMenu.razor.css` | 60 | Text does not meet the minimal contrast requirement with its background. |
| 2 | `UserManagementService/src/UserManagement.UI/Layout/NavMenu.razor.css` | 65 | Text does not meet the minimal contrast requirement with its background. |

**Como corrigir:**
```css
/* ❌ Não-conforme — contraste insuficiente (ex: cinza claro sobre branco) */
.nav-item a {
  color: #777777;           /* ratio ~4.48:1 — falha para texto normal */
  background-color: #ffffff;
}

/* ✅ Conforme — cinza mais escuro para atingir ratio ≥ 4.5:1 */
.nav-item a {
  color: #595959;           /* ratio ~7.0:1 — passa WCAG AA */
  background-color: #ffffff;
}
```
Use a ferramenta [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/) para verificar os valores corretos para as cores utilizadas no projeto.

---

### 🔵 MINOR

---

#### Regra: `csharpsquid:S1075` — URIs should not be hardcoded

**Tipo:** Code Smell  
**Descrição:** URIs hard-coded dificultam testes (o caminho pode não existir no ambiente de test), não são portáveis entre sistemas operacionais e podem conter informações sensíveis como IPs internos.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `UserManagementService/src/UserManagement.UI/Program.cs` | 9 | Refactor your code not to use hardcoded absolute paths or URIs. |

**Como corrigir:**
```csharp
// ❌ Não-conforme
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("http://localhost:5000") });

// ✅ Conforme — ler da configuração
var baseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl not configured.");
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(baseUrl) });
```
Definir `"ApiBaseUrl"` em `appsettings.json` por ambiente ou via variável de ambiente.

---

## Security Hotspots

### Hotspots de `UserManagementService/Dockerfile` e `Program.cs`

| # | Arquivo | Linha | Regra | Categoria | Probabilidade | Mensagem |
|---|---------|-------|-------|-----------|---------------|----------|
| 1 | `UserManagementService/Dockerfile` | 15 | `docker:S6470` | Permission | MEDIUM | Copying recursively might inadvertently add sensitive data to the container. |
| 2 | `UserManagementService/Dockerfile` | 23 | `docker:S6471` | Permission | MEDIUM | This image might run with "root" as the default user. |
| 3 | `UserManagementService/src/UserManagement.Api/Program.cs` | 18 | `csharpsquid:S5122` | Insecure Config | LOW | Make sure this permissive CORS policy is safe here. |

---

#### Hotspot: `docker:S6470` — Recursively copying context directories (Linha 15)

**Pergunte-se:** O contexto de build contém arquivos com segredos, credenciais ou dados sensíveis que não devem estar na imagem final?

**Como corrigir:**
```dockerfile
# ❌ Não-conforme
COPY . .

# ✅ Conforme — copiar apenas o necessário
COPY ./src/ ./src/
COPY ./UserManagementService.sln .
```
Adicionar `.dockerignore`:
```
.git
.idea
**/*.user
**/.vs
**/*.secrets.json
```

---

#### Hotspot: `docker:S6471` — Running containers as root user (Linha 23)

**Pergunte-se:** O container serve endpoints acessíveis da internet? O container não requer root para funcionar?

**Como corrigir:**
```dockerfile
# ❌ Não-conforme — sem USER (executa como root implicitamente)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserManagement.Api.dll"]

# ✅ Conforme — adicionar usuário dedicado antes do ENTRYPOINT
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
COPY --from=build --chown=appuser:appgroup /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "UserManagement.Api.dll"]
```

---

#### Hotspot: `csharpsquid:S5122` — Permissive CORS policy (Linha 18)

**Pergunte-se:**
- A política CORS permite origens não confiáveis (`AllowAnyOrigin()` ou `WithOrigins("*")`)?
- A política é definida dinamicamente por input controlado pelo usuário (cabeçalho `Origin`)?

**Como corrigir:**
```csharp
// ❌ Não-conforme — permite qualquer origem
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()      // ← Sensitivo
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ✅ Conforme — whitelist de origens confiáveis
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                  "https://minha-app.empresa.com",
                  "https://staging.empresa.com")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ✅ Conforme — ler origens da configuração para diferentes ambientes
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));
```

---

## Checklist de Correções

- [ ] **[BLOCKER]** Implementar asserção real em `UserManagement.IntegrationTests/UnitTest1.cs` linha 6
- [ ] **[CRITICAL]** Corrigir race condition em `AttendanceTicketService.cs` linha 25 → remover escrita a campo `static` de método de instância
- [ ] **[MAJOR]** Corrigir `WORKDIR` em `Dockerfile` linha 16 → usar caminho absoluto `/app`
- [ ] **[MAJOR]** Adicionar aspas duplas à variável em `Dockerfile` linha 21 → `"$VARIABLE"`
- [ ] **[MAJOR]** Dividir `AttendanceController.cs` em 2 controllers separados por responsabilidade
- [ ] **[MAJOR]** Usar `await app.RunAsync()` em `Program.cs` linha 54
- [ ] **[MAJOR]** Aumentar contraste em `NavMenu.razor.css` linha 60 → verificar com WebAIM (mínimo 4.5:1)
- [ ] **[MAJOR]** Aumentar contraste em `NavMenu.razor.css` linha 65 → verificar com WebAIM (mínimo 4.5:1)
- [ ] **[MINOR]** Extrair URI hard-coded de `UserManagement.UI/Program.cs` linha 9 → usar `IConfiguration`
- [ ] **[HOTSPOT]** Revisar `COPY . .` em `Dockerfile` linha 15 → especificar arquivos/diretórios explicitamente + criar `.dockerignore`
- [ ] **[HOTSPOT]** Adicionar instrução `USER nonroot` em `Dockerfile` linha 23 → criar usuário dedicado
- [ ] **[HOTSPOT]** Revisar política CORS em `Program.cs` linha 18 → substituir `AllowAnyOrigin()` por whitelist de origens confiáveis
