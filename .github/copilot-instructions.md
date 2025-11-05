# URL Shortener - Proof of Concept

## ⚠️ REGRA CRÍTICA - LEIA PRIMEIRO

**NÃO COMECE A IMPLEMENTAÇÃO SEM AUTORIZAÇÃO EXPLÍCITA DO USUÁRIO.**

Quando solicitado para ajudar neste projeto:

1. Primeiro, explique o que você pretende fazer
2. Aguarde confirmação do usuário antes de criar/modificar arquivos
3. Execute apenas o que foi explicitamente solicitado

## Contexto do Projeto

**IMPORTANTE**: Esta é uma **Prova de Conceito (POC)**. Mantenha a simplicidade. Evite overengineering.

Você está trabalhando em um serviço de encurtamento de URLs em .NET 8 usando Minimal APIs. O objetivo é validar a viabilidade técnica da solução de forma rápida e pragmática.

## Stack Tecnológico

- **Runtime**: .NET 8
- **API**: Minimal APIs (simplicidade e performance)
- **Banco de Dados**: PostgreSQL
- **ORM**: Entity Framework Core com Npgsql
- **Containerização**: Docker Compose (toda a stack)

## Ambiente de Desenvolvimento

Use SEMPRE `docker-compose` para subir o ambiente completo:

```bash
# Subir todos os serviços (API + PostgreSQL)
docker-compose up -d

# Ver logs
docker-compose logs -f

# Parar ambiente
docker-compose down

# Rebuild após mudanças
docker-compose up -d --build
```

O `docker-compose.yml` DEVE incluir:

- Serviço da API (.NET)
- PostgreSQL com configurações de desenvolvimento
- Network bridge entre os serviços
- Volumes para persistência do banco

## Endpoints da API

Implemente APENAS estes dois endpoints essenciais:

1. **POST /shorten** - Criar URL encurtada
2. **GET /{shortCode}** - Redirecionar para URL original (HTTP 301)

**NÃO** implemente estatísticas, analytics ou endpoints administrativos nesta POC.

## Algoritmo de Geração de Short Codes

A unicidade dos short codes é garantida através de um identificador numérico incremental do PostgreSQL convertido para base62 usando **Hashids.net**.

### Passos do Algoritmo

1. **Gerar ID**: Usar sequence do PostgreSQL iniciando em `916132832` (garante 6 caracteres em base62)
2. **Converter e Ofuscar com Hashids.net**: Biblioteca que converte IDs em códigos ofuscados usando:
   - Salt: `url-shortener-poc-secret-key`
   - Alfabeto Base62: `0-9a-zA-Z`
   - Comprimento mínimo: 6 caracteres
3. **Resultado**: Short code de 6+ caracteres, não sequencial, único e reversível

### Exemplo de Implementação

```csharp
// Usando Hashids.net (NuGet: HashidsNet)
private static readonly Hashids _hashids = new Hashids(
    salt: "url-shortener-poc-secret-key",
    minHashLength: 6,
    alphabet: "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
);

public static string Generate(long id) => _hashids.EncodeLong(id);
public static long Decode(string shortCode) => _hashids.DecodeLong(shortCode)[0];
```

### Alfabeto Base62

Use exatamente esta ordem: `0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ`

## Modelo de Dados

Tabela `urls` no PostgreSQL:

```sql
CREATE TABLE urls (
    id BIGSERIAL PRIMARY KEY DEFAULT nextval('url_id_seq'),
    short_code VARCHAR(10) NOT NULL,
    original_url TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE SEQUENCE url_id_seq START WITH 916132832;
CREATE UNIQUE INDEX IX_urls_short_code ON urls(short_code);
```

**IMPORTANTE**:

- ✅ UNIQUE constraint em `short_code` garante integridade no banco de dados
- ✅ PostgreSQL cria automaticamente um índice para a constraint UNIQUE
- ✅ Busca otimizada: decode do shortCode → ID → busca por PRIMARY KEY
- ✅ Índice único serve apenas como proteção de integridade, não para consultas

Campos essenciais apenas. NÃO adicione campos de analytics, contadores ou metadados desnecessários.

## Regras de Implementação

### FAÇA

- Use Minimal APIs (sem controllers)
- DTOs simples para request/response usando o tipo record em dotnet
- Validação básica de URL (formato válido, não vazio)
- Async/await em operações de I/O
- Logs com `ILogger` (built-in do .NET)
- Tratamento de erros com try/catch nos endpoints
- Testes unitários básicos com xUnit
- **Hashids.net** para ofuscação de IDs (biblioteca NuGet)
- **Extension Methods** para organização de endpoints
- **Swagger/OpenAPI** para documentação interativa da API

### NÃO FAÇA (para esta POC)

- ❌ Rate limiting
- ❌ Autenticação/autorização
- ❌ Cache Redis
- ❌ Custom short codes
- ❌ Estatísticas de acesso
- ❌ Validação avançada de URLs (malware, phishing)
- ❌ Repository pattern (use DbContext diretamente)
- ❌ CQRS, MediatR ou arquiteturas complexas
- ❌ Health checks elaborados
- ❌ Índice secundário em `short_code` (busca por PRIMARY KEY após decode)
- ❌ **Documentação (README, comentários extensos) antes de tudo funcionar e ser testado**

## Ordem de Execução (OBRIGATÓRIA)

**IMPORTANTE**: Siga esta ordem rigorosamente. NÃO pule etapas.

1. **Implementar** toda a funcionalidade
2. **Testar** localmente com `docker-compose up`
3. **Validar** que os endpoints funcionam corretamente
4. **Executar** testes unitários e garantir que passam
5. **SOMENTE ENTÃO** criar documentação (README.md)

**Regra de Ouro**: Código funcionando e testado > Documentação. Não documente nada antes de validar que funciona.

## Comandos de Desenvolvimento

```bash
# Build local (sem Docker)
dotnet build

# Executar testes
dotnet test

# Executar API localmente (requer PostgreSQL rodando)
dotnet run --project src/UrlShortener.Api

# Ver cobertura de testes
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=cobertura
```

## Estrutura de Arquivos Implementada

```
src/
  UrlShortener.sln            # Solution file
  UrlShortener.Api/
    Program.cs                # Configuração da API
    appsettings.json          # Connection string do PostgreSQL
    Endpoints/
      UrlEndpoints.cs         # Extension Methods com endpoints
    Models/
      Url.cs                  # Entity do EF Core
      ShortenRequest.cs       # DTO de entrada
      ShortenResponse.cs      # DTO de saída
    Services/
      ShortCodeGenerator.cs   # Hashids.net para conversão
    Data/
      AppDbContext.cs         # DbContext do EF Core
    Migrations/
      *_InitialCreate.cs      # Migration inicial
  UrlShortener.Tests/
    ShortCodeGeneratorTests.cs
Dockerfile
docker-compose.yml
README.md
ROADMAP.md
```

## Exemplo de Uso da API

### Criar URL Encurtada

```bash
POST /shorten
Content-Type: application/json

{
  "originalUrl": "https://www.exemplo.com/pagina/muito/longa"
}
```

**Response 201 Created:**

```json
{
  "shortUrl": "http://localhost:5000/aB3xY9",
  "shortCode": "aB3xY9",
  "originalUrl": "https://www.exemplo.com/pagina/muito/longa"
}
```

### Acessar URL Encurtada

```bash
GET /aB3xY9
```

**Response 301 Moved Permanently:**

```
Location: https://www.exemplo.com/pagina/muito/longa
```

## Princípios de Código para esta POC

1. **Simplicidade > Perfeição**: Código funcional e legível é melhor que arquitetura complexa
2. **Pragmatismo**: Se funciona e é simples, está bom para a POC
3. **Foco no Core**: Geração de short codes e redirecionamento são as únicas features críticas
4. **Docker First**: Todo desenvolvimento e testes devem usar Docker Compose
5. **Documentação Mínima**: README com instruções de setup e exemplos de uso

## Validação de Sucesso da POC

A POC está completa quando:

- ✅ `docker-compose up` sobe API + PostgreSQL sem erros
- ✅ POST /shorten retorna short code de 6+ caracteres
- ✅ GET /{shortCode} redireciona corretamente (301)
- ✅ Short codes são únicos e não sequenciais (Hashids)
- ✅ Busca otimizada por PRIMARY KEY (decode → ID → query)
- ✅ Testes básicos passam (10 testes unitários)
- ✅ Swagger UI acessível em `/swagger`
- ✅ README documenta como usar
- ✅ ROADMAP documenta próximos passos para produção
