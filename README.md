# HydroGuard 💧

Sistema de monitoramento inteligente do consumo de água para residências e empresas, desenvolvido para o tema ESG **"Acesso à água e preservação de recursos naturais"**.

## Sobre o Projeto

O HydroGuard permite:

- Cadastro de medidores/hidrômetros
- Registro de consumo de água
- Detecção automática de vazamentos (consumo > média histórica + 40%)
- Dashboard com indicadores ESG (score 0-100)
- Relatórios gerenciais com agregações LINQ
- Autenticação JWT com roles (Admin, Operador)

## Stack Tecnológica

| Camada         | Tecnologia                         |
| -------------- | ---------------------------------- |
| Backend        | ASP.NET Core 8 Web API, C#         |
| Banco de Dados | SQL Server + Entity Framework Core |
| Autenticação   | JWT Bearer                         |
| Testes         | xUnit + WebApplicationFactory      |
| Frontend       | HTML, CSS, JavaScript puro         |
| Container      | Docker + Docker Compose            |

## Arquitetura (MVVM)

```
HydroGuard/
├── HydroGuard.API/
│   ├── Controllers/     → Endpoints da API REST
│   ├── Models/          → Entidades do banco (User, Medidor, Consumo, Alerta)
│   ├── ViewModels/      → DTOs de entrada/saída com validações
│   ├── Services/        → Lógica de negócio
│   ├── Repositories/    → Acesso a dados (Repository Pattern)
│   ├── Data/            → DbContext + Seed
│   ├── Middleware/      → Tratamento global de erros
│   ├── Config/          → Configuração JWT
│   ├── Migrations/      → InitialCreate + SeedData
│   └── wwwroot/         → Frontend HTML/CSS/JS
├── HydroGuard.Tests/    → Testes xUnit (18 testes)
├── docker-compose.yml   → API + SQL Server
└── README.md
```

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional, para container)
- SQL Server (local ou via Docker)

## Como Executar

### Opção 1: Docker Compose (recomendado)

```bash
# Na pasta HydroGuard/
docker-compose up --build

# Aguarde os containers iniciar (SQL Server + API)
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
# Frontend: http://localhost:5000
```

### Opção 2: Execução Local

```bash
# 1. Clone ou acesse a pasta do projeto
cd HydroGuard

# 2. Restaure dependências
dotnet restore

# 3. Configure a connection string em appsettings.json (se necessário)
# Server=localhost,1433;Database=HydroGuardDB;User Id=sa;Password=HydroGuard@2026;TrustServerCertificate=True;

# 4. Execute a API (migrations e seed rodam automaticamente)
dotnet run --project HydroGuard.API

# 5. Acesse:
# - Frontend: http://localhost:5000
# - Swagger:  http://localhost:5000/swagger
```

### Opção 3: SQL Server via Docker (sem compose completo)

```bash
# Apenas SQL Server
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=HydroGuard@2026" -p 1433:1433 --name hydroguard-sql -d mcr.microsoft.com/mssql/server:2022-latest

# Depois execute a API localmente
dotnet run --project HydroGuard.API
```

## Credenciais de Acesso

| Usuário  | Senha       | Role     |
| -------- | ----------- | -------- |
| admin    | admin123    | Admin    |
| operador | operador123 | Operador |

## Endpoints da API

| Método | Endpoint                            | Descrição             | Auth |
| ------ | ----------------------------------- | --------------------- | ---- |
| POST   | `/api/auth/login`                   | Login JWT             | Não  |
| GET    | `/api/medidores?page=1&pageSize=10` | Listar medidores      | Sim  |
| POST   | `/api/medidores`                    | Cadastrar medidor     | Sim  |
| GET    | `/api/consumos?page=1&pageSize=10`  | Histórico de consumos | Sim  |
| POST   | `/api/consumos`                     | Registrar consumo     | Sim  |
| GET    | `/api/alertas/vazamentos`           | Detectar vazamentos   | Sim  |
| GET    | `/api/esg/indicadores`              | Dashboard ESG         | Sim  |
| GET    | `/api/relatorios/consumo`           | Relatório gerencial   | Sim  |

### Exemplo de Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

### Exemplo com Token

```bash
# Use o token retornado no header Authorization
curl http://localhost:5000/api/medidores?page=1&pageSize=10 \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

## Testes Unitários

```bash
# Executar todos os testes
dotnet test HydroGuard.Tests

# Com detalhes
dotnet test HydroGuard.Tests --verbosity normal
```

**Cobertura:** 18 testes em 6 controllers (3 testes cada):

- AuthControllerTests
- MedidoresControllerTests
- ConsumosControllerTests
- AlertasControllerTests
- EsgControllerTests
- RelatoriosControllerTests

## Migrations

As migrations são aplicadas automaticamente ao iniciar a API:

1. `InitialCreate` — Cria tabelas Users, Medidores, Consumos, Alertas
2. `SeedData` — Marca ponto de seed (dados inseridos via DbSeeder)

Para criar migrations manualmente:

```bash
dotnet ef migrations add NomeMigration --project HydroGuard.API
dotnet ef database update --project HydroGuard.API
```

## Frontend

O frontend está em `HydroGuard.API/wwwroot/` e é servido automaticamente pela API.

**Telas disponíveis:**

1. Login (JWT salvo no localStorage)
2. Dashboard ESG com indicadores e relatório
3. Cadastro e listagem de medidores
4. Registro e histórico de consumos
5. Alertas de vazamento

## Detecção de Vazamentos

Regra implementada:

```
Se consumo_atual > média_histórica × 1.4 → Alerta de vazamento
```

Severidade:

- **Alta**: consumo > 80% acima da média
- **Média**: consumo > 50% acima da média
- **Baixa**: consumo > 40% acima da média

## Tratamento de Erros

Respostas padronizadas via middleware global:

```json
{
  "error": true,
  "message": "Medidor não encontrado"
}
```

| Status | Situação                                |
| ------ | --------------------------------------- |
| 400    | Dados inválidos / regra de negócio      |
| 401    | Não autenticado / credenciais inválidas |
| 404    | Recurso não encontrado                  |
| 500    | Erro interno do servidor                |

## ESG - Score de Sustentabilidade

O score ESG (0-100) é calculado com:

- Base: 70 pontos
- Bônus: +1 por cada 100L economizados (max +20)
- Penalidade: -1 por cada 50L desperdiçados (max -30)

## Autores

Aurélio | André | Danilo | Felipe | Victor
