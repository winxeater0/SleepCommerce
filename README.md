# SleepCommerce - API de Produtos

API REST para gerenciamento de produtos, desenvolvida como desafio técnico da Sleep Commerce.

## Tecnologias

- .NET 9
- Entity Framework Core 9
- PostgreSQL 16
- FluentValidation
- OpenTelemetry (traces, métricas e logs)
- Aspire Dashboard
- Swagger (Swashbuckle)
- xUnit + Moq + FluentAssertions + Testcontainers

## Arquitetura

Clean Architecture com 4 camadas:

- **Domain** - Entidades e interfaces de repositório
- **Application** - DTOs, services, validadores
- **Infrastructure** - EF Core, repositórios, migrations, seed
- **API** - Controllers, middleware, configuração

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started)

## Como executar

### 1. Subir a infraestrutura (PostgreSQL + Aspire Dashboard)

```bash
docker-compose up -d
```

Isso inicia o PostgreSQL e o Aspire Dashboard (coletor de telemetria).

### 2. Executar a API

```bash
dotnet run --project src/SleepCommerce.API
```

### 3. Acessar o Swagger

Abra no navegador: `https://localhost:5001/swagger` ou `http://localhost:5000/swagger`

## Endpoints

| Método | Rota                | Descrição                         |
|--------|---------------------|-----------------------------------|
| GET    | /api/products       | Listar (paginado, filtro, sort)   |
| GET    | /api/products/{id}  | Buscar por ID                     |
| POST   | /api/products       | Criar produto                     |
| PUT    | /api/products/{id}  | Atualizar produto                 |
| DELETE | /api/products/{id}  | Deletar produto                   |

### Query Parameters (GET /api/products)

| Parâmetro      | Tipo   | Default  | Descrição                              |
|----------------|--------|----------|----------------------------------------|
| nome           | string | -        | Filtro por nome (contains, case-insensitive) |
| orderBy        | string | nome     | Campo: nome, valor, estoque, dataCriacao     |
| orderDirection | string | asc      | Direção: asc, desc                           |
| pageNumber     | int    | 1        | Número da página                             |
| pageSize       | int    | 10       | Itens por página                             |

## Testes

### Testes unitários

```bash
dotnet test tests/SleepCommerce.UnitTests
```

### Testes de integração

Requer Docker em execução (Testcontainers cria o PostgreSQL automaticamente):

```bash
dotnet test tests/SleepCommerce.IntegrationTests
```

### Todos os testes

```bash
dotnet test
```

## Observabilidade (OpenTelemetry + Aspire Dashboard)

A API exporta traces, métricas e logs via OpenTelemetry (protocolo OTLP) para o Aspire Dashboard, que já está configurado no `docker-compose.yml`.

### Passo a passo

#### 1. Iniciar o Aspire Dashboard

Se ainda não subiu a infraestrutura:

```bash
docker-compose up -d
```

Verifique se o container está rodando:

```bash
docker ps --filter name=sleepcommerce-dashboard
```

#### 2. Acessar o Dashboard

Abra no navegador: [http://localhost:18888](http://localhost:18888)

O dashboard não exige autenticação (configurado com `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS`).

#### 3. Executar a API e gerar telemetria

```bash
dotnet run --project src/SleepCommerce.API
```

A API envia telemetria automaticamente para `localhost:4317` (OTLP gRPC). Faça algumas requisições via Swagger ou curl para gerar dados:

```bash
# Listar produtos
curl http://localhost:5000/api/products

# Criar produto
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"nome":"Teclado RGB","descricao":"Mecânico","estoque":50,"valor":299.90}'

# Buscar por ID
curl http://localhost:5000/api/products/{id}
```

#### 4. Visualizar no Dashboard

No Aspire Dashboard (`http://localhost:18888`) você encontra três seções:

| Seção          | O que mostra                                                                 |
|----------------|------------------------------------------------------------------------------|
| **Traces**     | Cada requisição HTTP de ponta a ponta, incluindo spans do EF Core (queries SQL) e spans customizados de negócio (`ProductService.Create`, `.Update`, `.Delete`) |
| **Metrics**    | Métricas do ASP.NET Core (req/s, duração, status codes) e do HttpClient      |
| **Structured Logs** | Logs da aplicação com scopes e mensagens formatadas                     |

#### O que é instrumentado

- **ASP.NET Core** — trace automático de cada request/response (rota, status code, duração)
- **Entity Framework Core** — trace de cada query SQL executada
- **HttpClient** — trace de chamadas HTTP de saída (se houver)
- **Spans customizados** — operações de criação, atualização e exclusão no `ProductService`, com tag `product.id`

## Estrutura do Projeto

```
SleepCommerce/
├── docker-compose.yml
├── SleepCommerce.sln
├── src/
│   ├── SleepCommerce.Domain/
│   │   ├── Entities/Produto.cs
│   │   └── Interfaces/IProductRepository.cs
│   ├── SleepCommerce.Application/
│   │   ├── Diagnostics/ActivitySources.cs
│   │   ├── DTOs/
│   │   ├── Interfaces/IProductService.cs
│   │   ├── Services/ProductService.cs
│   │   └── Validators/ProductRequestValidator.cs
│   ├── SleepCommerce.Infrastructure/
│   │   ├── Data/AppDbContext.cs
│   │   ├── Data/Configurations/ProductConfiguration.cs
│   │   ├── Data/Seed/ProductSeed.cs
│   │   └── Repositories/ProductRepository.cs
│   └── SleepCommerce.API/
│       ├── Controllers/ProductsController.cs
│       ├── Middleware/ExceptionHandlingMiddleware.cs
│       └── Program.cs
└── tests/
    ├── SleepCommerce.UnitTests/
    └── SleepCommerce.IntegrationTests/
```
