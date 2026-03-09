# Relatório da Camada de Infraestrutura — ArquitetoMovel_customer-services

**Data:** 8 de março de 2026
**Session ID:** 20260308-433a
**Camada:** Infraestrutura (Database, Migrations, Broker, Docker, Domain)

---

## Resumo

| Categoria              | Quantidade |
|------------------------|-----------|
| Issues MAJOR           | 7          |
| Issues MINOR           | 5          |
| **Total desta camada** | **12**     |

---

## Issues por Severidade

### 🟡 MAJOR

---

#### Regra: `csharpsquid:S3881` — Implementação de `IDisposable` deve seguir o padrão dispose

**Descrição:**  
A interface `IDisposable` tem um padrão de implementação bem definido no .NET. Não seguir este padrão pode causar vazamentos de recursos (memory leaks), comportamento imprevisível ao chamar `Dispose()` múltiplas vezes, e problemas com o `finalizer` (destructor). O padrão completo inclui o método `Dispose(bool disposing)` para permitir que classes derivadas também liberem seus recursos.

**Atributo de Código Limpo:** COMPLETE (categoria INTENTIONAL)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementInfra/Database/UnitOfWork.cs` | 7 | Fix this implementation of 'IDisposable' to conform to the dispose pattern. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Implementação incompleta do IDisposable
public class UnitOfWork : IDisposable
{
    private DbContext _context;

    public void Dispose()
    {
        _context?.Dispose(); // Noncompliant: não segue o padrão completo
    }
}

// ✅ CONFORME — Padrão dispose completo
public class UnitOfWork : IDisposable
{
    private DbContext _context;
    private bool _disposed = false;

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
                // Liberar recursos gerenciados
                _context?.Dispose();
            }
            // Liberar recursos não-gerenciados (se houver)
            _disposed = true;
        }
    }

    // Finalizer — apenas se houver recursos não-gerenciados
    // ~UnitOfWork() { Dispose(disposing: false); }
}
```

---

#### Regra: `csharpsquid:S112` — Exceções gerais ou reservadas não devem ser lançadas

**Descrição:**  
Lançar `System.NullReferenceException` manualmente é problemático pois: (1) é uma exceção _reservada_ pelo runtime do .NET, lançada automaticamente quando há desreferência de null; (2) ao ser lançada manualmente, confunde o consumidor sobre se foi um erro do runtime ou intencional; (3) exceções genéricas forçam consumidores a capturar exceções que não pretendem tratar.

**Atributo de Código Limpo:** COMPLETE (categoria INTENTIONAL)  
**CWE:** CWE-397 (Declaration of Throws for Generic Exception)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 2 | `CustomerManagementService/src/CustomerManagementInfra/Database/UnitOfWork.cs` | 22 | `'System.NullReferenceException'` should not be thrown by user code. |
| 3 | `CustomerManagementService/src/CustomerManagementInfra/Database/UnitOfWork.cs` | 37 | `'System.NullReferenceException'` should not be thrown by user code. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Lançando exceção reservada
public void SaveChanges()
{
    if (_context == null)
        throw new NullReferenceException("context"); // Noncompliant
}

// ✅ CONFORME — Usar exceção específica e não-reservada
public void SaveChanges()
{
    if (_context == null)
        throw new InvalidOperationException("UnitOfWork was disposed or context was not initialized.");

    // Ou para parâmetros de construtor:
    // throw new ArgumentNullException(nameof(_context), "DbContext cannot be null.");
}
```

---

#### Regra: `csharpsquid:S3928` — Nomes de parâmetros em construtores de `ArgumentException` devem corresponder a parâmetros existentes

**Descrição:**  
Os construtores de `ArgumentException`, `ArgumentNullException`, `ArgumentOutOfRangeException` e `DuplicateWaitObjectException` aceitam um nome de parâmetro como argumento. Quando este nome não corresponde a um parâmetro real do método, dificulta a depuração pois a mensagem de erro aponta para um parâmetro inexistente.

**Atributo de Código Limpo:** LOGICAL (categoria INTENTIONAL)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 4 | `CustomerManagementService/src/CustomerManagementDomain/Entity/UserTicket.cs` | 30 | Use a constructor overloads that allows a more meaningful exception message to be provided. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Construtor sem mensagem descritiva
public UserTicket(string customerId, string description)
{
    if (string.IsNullOrEmpty(customerId))
        throw new ArgumentException(); // Noncompliant: sem mensagem e sem nome do parâmetro

    if (string.IsNullOrEmpty(description))
        throw new ArgumentNullException("nonExistentParam"); // Noncompliant: parâmetro não existe
}

// ✅ CONFORME — Construtor com mensagem e nome de parâmetro correto
public UserTicket(string customerId, string description)
{
    if (string.IsNullOrEmpty(customerId))
        throw new ArgumentException("CustomerId cannot be null or empty.", nameof(customerId));

    if (string.IsNullOrEmpty(description))
        throw new ArgumentNullException(nameof(description), "Description cannot be null.");
}
```

---

#### Regra: `docker:S7021` — Instrução `WORKDIR` deve usar caminho absoluto

**Descrição:**  
A instrução `WORKDIR` define o diretório de trabalho para as instruções `RUN`, `CMD`, `ENTRYPOINT`, `COPY` e `ADD` subsequentes. Usar caminhos relativos pode causar confusão e comportamento inesperado, especialmente se o diretório de trabalho foi alterado por uma instrução anterior.

**Atributo de Código Limpo:** CONVENTIONAL (categoria CONSISTENT)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 5 | `NotificationService/Dockerfile` | 17 | Use an absolute path instead of this relative path when defining the WORKDIR. |
| 6 | `UserManagementService/Dockerfile` | 16 | Use an absolute path instead of this relative path when defining the WORKDIR. |

**Como corrigir:**

```dockerfile
# ❌ NÃO CONFORME — Caminho relativo
WORKDIR app
WORKDIR ./app

# ✅ CONFORME — Caminho absoluto
WORKDIR /app
WORKDIR /usr/src/app
```

---

#### Regra: `docker:S6570` — Variáveis devem ser envolvidas em aspas duplas no Dockerfile

**Descrição:**  
Em comandos shell dentro de Dockerfiles, referências a variáveis sem aspas duplas passam por expansão de glob (pathname expansion) e word splitting. Isso pode causar comportamento inesperado se a variável contiver espaços ou caracteres especiais como `*`.

**Atributo de Código Limpo:** COMPLETE (categoria INTENTIONAL)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 7 | `NotificationService/Dockerfile` | 22 | Surround this variable with double quotes; otherwise, it can lead to unexpected behavior. |
| 8 | `UserManagementService/Dockerfile` | 21 | Surround this variable with double quotes; otherwise, it can lead to unexpected behavior. |

**Como corrigir:**

```dockerfile
# ❌ NÃO CONFORME — Variável sem aspas (sujeita a glob e word splitting)
RUN test="command t*.sh" && echo $test
ENTRYPOINT dotnet $APP_DLL

# ✅ CONFORME — Variável com aspas duplas
RUN test="command t*.sh" && echo "$test"
ENTRYPOINT ["dotnet", "$APP_DLL"]
# Ou:
ENTRYPOINT dotnet "$APP_DLL"
```

---

### 🔵 MINOR

---

#### Regra: `csharpsquid:S3260` — Classes de record privadas não derivadas devem ser marcadas como `sealed`

**Descrição:**  
Classes de record que são privadas e não são estendidas em nenhum lugar da assembly devem ser declaradas como `sealed`. Isso é uma otimização de performance (o compilador pode gerar código mais eficiente) e comunica clareza de intenção ao leitor do código.

**Atributo de Código Limpo:** EFFICIENT (categoria INTENTIONAL)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementInfra/Broker/CustomerIntegrationBus.cs` | 15 | Private record classes which are not derived in the current assembly should be marked as 'sealed'. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME
private record CustomerEvent(string CustomerId, string EventType);

// ✅ CONFORME
private sealed record CustomerEvent(string CustomerId, string EventType);
```

---

#### Regra: `csharpsquid:S2325` — Métodos sem acesso a dados de instância devem ser `static`

**Descrição:**  
Um método que não acessa campos ou propriedades de instância pode ser declarado como `static`, comunicando claramente ao leitor que ele não depende de estado do objeto. Isso também facilita seu uso sem necessidade de instanciar a classe.

**Atributo de Código Limpo:** CLEAR (categoria INTENTIONAL)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 2 | `CustomerManagementService/src/CustomerManagementInfra/Database/UserTicketDbContextFactory.cs` | 8 | Make 'CreateDbContext' a static method. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Método de instância que não usa estado da instância
public class UserTicketDbContextFactory : IDesignTimeDbContextFactory<UserTicketDbContext>
{
    public UserTicketDbContext CreateDbContext(string[] args) // Noncompliant
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserTicketDbContext>();
        optionsBuilder.UseNpgsql("...");
        return new UserTicketDbContext(optionsBuilder.Options);
    }
}

// ✅ CONFORME — Método estático
public class UserTicketDbContextFactory : IDesignTimeDbContextFactory<UserTicketDbContext>
{
    public static UserTicketDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserTicketDbContext>();
        optionsBuilder.UseNpgsql("...");
        return new UserTicketDbContext(optionsBuilder.Options);
    }
}
```

> ⚠️ **Nota:** A interface `IDesignTimeDbContextFactory<T>` requer que `CreateDbContext` seja um método de instância (não estático). Esta regra pode não ser aplicável aqui — considere marcar como **falso positivo** no SonarQube se for o caso.

---

#### Regra: `csharpsquid:S1192` — Literais de string não devem ser duplicados

**Descrição:**  
Strings literais duplicadas tornam o processo de refatoração complexo e propenso a erros, pois qualquer mudança precisaria ser propagada para todas as ocorrências. Substituir por constantes garante que a mudança seja feita em um único lugar.

**Atributo de Código Limpo:** DISTINCT (categoria ADAPTABLE)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 3 | `CustomerManagementService/src/CustomerManagementInfra/Migrations/20241127155959_InitialCreate.cs` | 23 | Define a constant instead of using this literal `'timestamp with time zone'` 4 times. |
| 4 | `CustomerManagementService/src/CustomerManagementInfra/Migrations/20241202000212_migratonV2.cs` | 16 | Define a constant instead of using this literal `'UserTickets'` 6 times. |
| 5 | `CustomerManagementService/src/CustomerManagementInfra/Migrations/20241202000212_migratonV2.cs` | 17 | Define a constant instead of using this literal `'timestamp with time zone'` 12 times. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME — Literal repetida
migrationBuilder.AddColumn<DateTime>(type: "timestamp with time zone", ...);
migrationBuilder.AddColumn<DateTime>(type: "timestamp with time zone", ...);
migrationBuilder.AddColumn<DateTime>(type: "timestamp with time zone", ...);

// ✅ CONFORME — Usar constante
private const string TimestampWithTimeZone = "timestamp with time zone";
private const string UserTicketsTable = "UserTickets";

migrationBuilder.AddColumn<DateTime>(type: TimestampWithTimeZone, ...);
migrationBuilder.AddColumn<DateTime>(type: TimestampWithTimeZone, ...);
```

> 📝 **Nota sobre Migrations:** Arquivos de migration são gerados automaticamente pelo EF Core. Embora a correção seja válida, pode ser mais prático suprimir estas ocorrências via `#pragma warning` ou anotação `[SuppressMessage]` nos arquivos de migration, já que são arquivos gerados.

---

## Conclusão desta Camada

**Prioridade de correções:**

1. **🟡 MAJOR — `UnitOfWork.cs` (linhas 7, 22, 37):** Corrigir o padrão `IDisposable` e substituir `NullReferenceException` por exceções específicas — risco de vazamento de recursos no DbContext.
2. **🟡 MAJOR — `UserTicket.cs` (linha 30):** Melhorar as mensagens de exceção para facilitar depuração em produção.
3. **🟡 MAJOR — Dockerfiles (WORKDIR + variáveis):** Correções simples que melhoram a confiabilidade dos builds Docker.
4. **🔵 MINOR — `CustomerIntegrationBus.cs` (linha 15):** Adicionar `sealed` ao record — correção trivial.
5. **🔵 MINOR — Literais duplicados em Migrations:** Avaliar se vale adicionar constantes ou suprimir (arquivos gerados).
