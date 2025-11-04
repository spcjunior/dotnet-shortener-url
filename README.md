# URL Shortener

Encurtador de URLs desenvolvido como POC (Proof of Concept) utilizando .NET 8 Minimal APIs, PostgreSQL e Docker.

## 🚀 Tecnologias

- **.NET 8** - Minimal APIs
- **PostgreSQL 16** - Banco de dados
- **Entity Framework Core** - ORM
- **Hashids.net** - Ofuscação de IDs
- **Swagger/OpenAPI** - Documentação interativa
- **Docker & Docker Compose** - Containerização
- **xUnit** - Testes unitários

## 🎯 Funcionalidades

- ✅ Criar URLs encurtadas com códigos únicos de 6+ caracteres
- ✅ Redirecionamento permanente (HTTP 301)
- ✅ IDs sequenciais → códigos não-sequenciais (Hashids)
- ✅ Busca otimizada por PRIMARY KEY

## 📋 Pré-requisitos

- Docker & Docker Compose
- .NET SDK 8.0 (apenas para desenvolvimento local)

## 🔧 Como Executar

### Com Docker (Recomendado)

```bash
# Subir aplicação e banco de dados
docker compose up -d

# Acessar Swagger UI
http://localhost:5000/swagger
```

### Desenvolvimento Local

```bash
# Restaurar dependências e buildar
cd src
dotnet build

# Executar testes
dotnet test

# Executar API (requer PostgreSQL rodando)
dotnet run --project UrlShortener.Api
```

## 📡 Endpoints

### POST /shorten

Cria uma URL encurtada.

**Request:**

```json
{
  "originalUrl": "https://github.com/spcjunior"
}
```

**Response (201):**

```json
{
  "shortUrl": "http://localhost:5000/wyXPayx",
  "shortCode": "wyXPayx",
  "originalUrl": "https://github.com/spcjunior"
}
```

### GET /{shortCode}

Redireciona para a URL original (HTTP 301).

**Exemplo:**

```bash
curl -L http://localhost:5000/wyXPayx
```

## 🧪 Testes

```bash
cd src
dotnet test
```

**Cobertura:** 10 testes unitários validando geração de códigos, reversibilidade e unicidade.

## 🏗️ Estrutura do Projeto

```
dotnet-shortener-url/
├── src/
│   ├── UrlShortener.sln
│   ├── UrlShortener.Api/          # API principal
│   └── UrlShortener.Tests/        # Testes unitários
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## 🔐 Algoritmo de Geração de Códigos

1. PostgreSQL gera ID sequencial (a partir de `916132832`)
2. Hashids converte ID → código Base62 ofuscado
3. Salt: `url-shortener-poc-secret-key`
4. Comprimento mínimo: 6 caracteres
5. Alfabeto: `0-9a-zA-Z` (62 caracteres)

**Resultado:** IDs sequenciais produzem códigos **não-sequenciais** e **reversíveis**.

## 📊 Otimizações Implementadas

- ✅ Busca por PRIMARY KEY ao invés de índice secundário
- ✅ Decode do shortCode → ID antes de consultar banco
- ✅ Dockerfile otimizado (sem código de testes)
- ✅ Extension Methods para organização de endpoints

## 🚀 Roadmap

Para informações sobre escalabilidade, alta disponibilidade e próximos passos para produção, consulte o [ROADMAP.md](./ROADMAP.md).

## 📝 Licença
