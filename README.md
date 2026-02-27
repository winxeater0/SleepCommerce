# SleepCommerce - API de Produtos

API REST para gerenciamento de produtos, desenvolvida como desafio técnico da Sleep Commerce.

## Tecnologias

- .NET 9
- Entity Framework Core 9
- PostgreSQL 16
- FluentValidation
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

### 1. Subir o banco de dados

```bash
docker-compose up -d
```

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
