# Relatório de Segurança — ArquitetoMovel_customer-services

**Data:** 8 de março de 2026
**Session ID:** 20260308-433a
**Camada:** Segurança (BLOCKER + Security Hotspots)

---

## Resumo

| Categoria              | Quantidade |
|------------------------|-----------|
| Issues BLOCKER         | 6          |
| Vulnerabilidades       | 6          |
| Security Hotspots      | 6          |
| **Total desta camada** | **12**     |

> ⚠️ **AÇÃO IMEDIATA NECESSÁRIA:** Credenciais expostas no código-fonte devem ser rotacionadas **imediatamente**, independente de qualquer correção de código.

---

## Issues por Severidade

### 🔴 BLOCKER

---

#### Regra: `csharpsquid:S2068` — Credenciais não devem ser hard-coded

**Descrição:**  
Credenciais hard-coded no código-fonte facilitam a extração de informações sensíveis por atacantes, especialmente em aplicações distribuídas ou open-source. A regra detecta credenciais em connection strings e nomes de variáveis que correspondem a padrões conhecidos (como `password`, `pwd`, etc.).

**Referências OWASP:** A07:2021 (Identification and Authentication Failures), A02:2017 (Broken Authentication)  
**CWE:** CWE-798 (Use of Hard-coded Credentials), CWE-259 (Use of Hard-coded Password)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `CustomerManagementService/src/CustomerManagementApp/appsettings.json` | 9 | `"password"` detected here, make sure this is not a hard-coded credential. |
| 2 | `CustomerManagementService/src/CustomerManagementApp/appsettings.json` | 10 | Review this hard-coded URI, which may contain a credential. |
| 3 | `CustomerManagementService/src/CustomerManagementInfra/Database/UserTicketDbContextFactory.cs` | 11 | `"password"` detected here, make sure this is not a hard-coded credential. |
| 4 | `NotificationService/src/NotificationService.Api/appsettings.json` | 9 | `"password"` detected here, make sure this is not a hard-coded credential. |
| 5 | `NotificationService/src/NotificationService.Infrastructure/Persistence/NotificationDbContextFactory.cs` | 11 | `"password"` detected here, make sure this is not a hard-coded credential. |

**Como corrigir:**

```csharp
// ❌ NÃO CONFORME
string password = "Admin123"; // Noncompliant
string url = "scheme://user:Admin123@domain.com"; // Noncompliant

// ✅ CONFORME
string password = GetEncryptedPassword(); // Buscar de variável de ambiente ou secrets
string url = $"scheme://{username}:{password}@domain.com";
```

**Abordagem recomendada para .NET:**
- Usar `IConfiguration` com variáveis de ambiente ou `appsettings.{Environment}.json` (não versionado)
- Usar gerenciadores de segredos: **Azure Key Vault**, **AWS Secrets Manager**, ou **`dotnet user-secrets`** para desenvolvimento local
- Nunca commitar arquivos com senhas reais; adicionar `appsettings.Development.json` ao `.gitignore`
- Rotacionar **imediatamente** todas as senhas expostas nos commits anteriores

---

#### Regra: `secrets:S6703` — Senha de banco de dados detectada em arquivo IDE

**Descrição:**  
Um segredo real (senha de banco de dados) foi detectado em um arquivo de configuração da IDE (`.idea/dataSources.xml`). Arquivos de projeto/IDE nunca devem conter credenciais reais, pois são frequentemente enviados ao controle de versão inadvertidamente.

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 6 | `NotificationService/.idea/.idea.NotificationService/.idea/dataSources.xml` | 9 | Make sure this database password gets changed and removed from the code. |

**Como corrigir:**
- Remover o arquivo `dataSources.xml` do controle de versão (se já foi commitado)
- Adicionar `.idea/` ao `.gitignore`
- Rotacionar a senha do banco de dados exposta imediatamente
- Verificar o histórico de commits e utilizar ferramentas como `git filter-branch` ou `BFG Repo Cleaner` para remover o segredo do histórico

---

## Security Hotspots

Security Hotspots são pontos do código que requerem revisão manual para determinar se representam um risco real. Todos estão com status **TO_REVIEW**.

### Docker — Permissões e Configuração de Contêineres

---

#### Hotspot: `docker:S6470` — Cópia recursiva pode incluir dados sensíveis

**Categoria:** permission | **Probabilidade:** MEDIUM

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 1 | `NotificationService/Dockerfile` | 16 | Copying recursively might inadvertently add sensitive data to the container. Make sure it is safe here. |
| 2 | `UserManagementService/Dockerfile` | 15 | Copying recursively might inadvertently add sensitive data to the container. Make sure it is safe here. |

**Pergunta a se fazer:**
- Os arquivos copiados podem conter dados sensíveis?
- O diretório de contexto contém arquivos sem propósito funcional para a imagem final?

**Prática recomendada:**

```dockerfile
# ❌ SENSÍVEL
COPY . .

# ✅ SEGURO — Especificar apenas o que é necessário
COPY ./src/MyApp/ /app/src/
COPY ./MyApp.csproj /app/
```

---

#### Hotspot: `docker:S6471` — Imagem pode executar como usuário root

**Categoria:** permission | **Probabilidade:** MEDIUM

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 3 | `NotificationService/Dockerfile` | 24 | This image might run with "root" as the default user. Make sure it is safe here. |
| 4 | `UserManagementService/Dockerfile` | 23 | This image might run with "root" as the default user. Make sure it is safe here. |

**Prática recomendada:**

```dockerfile
# ✅ Criar e usar um usuário não-root
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser
```

---

#### Hotspot: `docker:S2612` — Permissão de escrita concedida para "outros"

**Categoria:** permission | **Probabilidade:** MEDIUM  
**CWE:** CWE-732 (Incorrect Permission Assignment for Critical Resource)

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 5 | `NotificationService/Dockerfile` | 26 | Make sure granting write access to others is safe here. |

**Prática recomendada:**

```dockerfile
# ❌ SENSÍVEL
RUN chmod +x resource         # Permissão para todos
COPY --chmod=777 src dst      # Permissão total

# ✅ SEGURO — Permissões mínimas necessárias
RUN chmod u+x resource        # Apenas para o proprietário
COPY --chown=user:user --chmod=744 src dst
```

---

### C# — Configuração do Servidor Web

#### Hotspot: `csharpsquid:S5122` — Política CORS permissiva

**Categoria:** insecure-conf | **Probabilidade:** LOW

| # | Arquivo | Linha | Mensagem |
|---|---------|-------|----------|
| 6 | `UserManagementService/src/UserManagement.Api/Program.cs` | 18 | Make sure this permissive CORS policy is safe here. |

**Pergunta a se fazer:**
- A política CORS permite origens, métodos ou cabeçalhos sem restrição?
- Dados sensíveis são retornados por esta API?

**Prática recomendada:**

```csharp
// ❌ PERMISSIVO — Permite qualquer origem
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ✅ RESTRITIVO — Permite apenas origens confiáveis
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", policy =>
        policy.WithOrigins("https://meu-frontend.com")
              .WithMethods("GET", "POST")
              .WithHeaders("Content-Type", "Authorization"));
});
```

---

## Conclusão desta Camada

**Ações imediatas (antes de qualquer merge):**
1. **Rotacionar todas as senhas** expostas nos arquivos `appsettings.json`, `UserTicketDbContextFactory.cs`, `NotificationDbContextFactory.cs` e `dataSources.xml`
2. **Remover o arquivo `.idea/dataSources.xml`** do repositório e do histórico de commits
3. **Mover credenciais** para variáveis de ambiente ou Azure Key Vault / AWS Secrets Manager
4. **Revisar os Security Hotspots de Docker** para confirmar se são seguros no contexto da aplicação
5. **Revisar a política CORS** no `UserManagement.Api/Program.cs`
