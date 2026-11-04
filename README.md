# Projeto - Cidades ESG Inteligentes | HydroGuard

Sistema de monitoramento de consumo de água, cadastro de medidores, alertas de possíveis vazamentos e indicadores ESG. API ASP.NET Core 8, frontend HTML/CSS/JavaScript e SQL Server, com pipeline CI/CD completo (GitHub Actions), containerização com Docker e deploy automatizado em **staging** e **produção**.

> Deploy na VM Azure: siga [o roteiro completo de configuração SSH, secrets e demonstração](docs/DEPLOY-AZURE.md). Não é necessário cadastrar self-hosted runners.

## Como executar localmente com Docker

Pré-requisitos: Docker Engine/Docker Desktop com containers Linux e Docker Compose v2.24 ou superior; SDK .NET 8 para testes/smoke; Python 3.10+ apenas para a geração opcional do `.env`; host x86-64 com cerca de 4 GB de RAM livres para uma stack.

1. Clone o repositório e abra um terminal na pasta do projeto.
2. Gere a configuração local com senhas aleatórias (o script recusa sobrescrever um `.env` existente):

```bash
python3 scripts/setup_env.py
```

No Windows, use `python` se `python3` não estiver disponível. Alternativa manual: copie `.env.example` para `.env` e substitua TODOS os valores com `SUBSTITUA`. Use senhas de pelo menos 12 caracteres e chave JWT aleatória de pelo menos 32 bytes. Evite espaços, aspas, `$` e `;` nas senhas.

3. Suba a stack:

```bash
docker compose -p hydroguard-local config --quiet
docker compose -p hydroguard-local up -d --build --wait --wait-timeout 300
docker compose -p hydroguard-local ps
```

4. Abra a [interface local](http://localhost:5000), o [Swagger](http://localhost:5000/swagger) e o [readiness](http://localhost:5000/healthz).
5. Entre com usuário `admin` e a senha `ADMIN_PASSWORD` do seu `.env`. O ambiente local cria dados demonstrativos. Staging e produção criam somente o admin.

O banco só é acessível pela rede Docker. As migrations são aplicadas no início da API, depois que o SQL Server fica saudável. O health check `/healthz` inclui a conexão com o banco; `/livez` verifica apenas o processo.

```bash
# Parar sem apagar dados
docker compose -p hydroguard-local down
# Diagnóstico local (revise antes de compartilhar)
docker compose -p hydroguard-local logs --tail=100 api sqlserver
```

Não use `down --volumes` em staging/produção: isso apaga os dados. O pipeline usa essa opção somente na stack temporária de CI.

### Executar testes

```bash
dotnet restore HydroGuard.sln
dotnet build HydroGuard.sln -c Release --no-restore
dotnet test tests/HydroGuard.Tests/HydroGuard.Tests.csproj -c Release --no-build --logger "console;verbosity=normal"
```

Com o SDK .NET 8 instalado, também é possível executar a suíte com um comando no Git Bash:

```bash
bash scripts/test.sh
```

Para executar a mesma suíte em um container, na raiz do repositório:

```bash
docker build -f Dockerfile.test -t hydroguard-tests .
docker run --rm hydroguard-tests
```

A suíte `HydroGuard.Tests` usa xUnit, Reqnroll, WebApplicationFactory e EF Core InMemory. Ela executa os testes de integração existentes, quatro cenários BDD e valida as respostas das dez operações HTTP com JSON Schema. Os dados e credenciais são exclusivos do ambiente de teste. O job `container` executa separadamente os smoke tests com SQL Server real.

| Método e rota | Teste ou cenário | Schema de sucesso |
| --- | --- | --- |
| POST /api/auth/login | `Login_ContratoRetornaTokenAceitoPelaApi` | `login` |
| GET /api/medidores | `ListarMedidores_ContratoRetornaPaginaComMedidorEsperado` | `medidores` |
| POST /api/medidores | `CriarMedidor_ContratoRetornaDadosDoCadastro` | `medidor` |
| GET /api/consumos | Registrar uma leitura válida | `consumos` |
| POST /api/consumos | Registrar uma leitura válida | `consumo` |
| GET /api/alertas/vazamentos | Alertar quando a leitura ultrapassa a média histórica em mais de 40% | `vazamentos` |
| GET /api/esg/indicadores | `Indicadores_ContratoRetornaConsumoDoMes` | `esg` |
| GET /api/relatorios/consumo | `Relatorio_ContratoRetornaMedidorNoRanking` | `relatorio` |
| GET /healthz | `Readiness_ReturnsTestingEnvironmentWithoutAuthentication` | `healthz` |
| GET /livez | `Liveness_RespondsWithoutAuthentication` | `livez` |

Os arquivos `.feature` ficam em `tests/HydroGuard.Tests/Features` e os contratos em `tests/HydroGuard.Tests/Schemas`. As respostas JSON de leitura negativa (HTTP 400) e credenciais inválidas (HTTP 401) têm contratos próprios. O desafio JWT sem autenticação é verificado pelo status HTTP, pois a resposta não tem corpo JSON.

Para registrar as evidências da atividade, abra um pull request para `main` e acesse **Checks → build-test → Testes de API, BDD e contratos JSON**. Capture os testes BDD, os testes de contrato e o resumo de aprovados e falhas. Inclua no PDF o link do run e o SHA exibido para identificar a revisão. O run da seção **Prints** é a referência histórica da suíte anterior.

## Pipeline CI/CD

Ferramenta: **GitHub Actions**. Registry: **GitHub Container Registry (GHCR)**. Workflow: `.github/workflows/ci-cd.yml`.

| Etapa | Onde executa | O que valida / realiza |
| --- | --- | --- |
| build-test | Runner GitHub Ubuntu 24.04 | Restore, build Release e testes de API, BDD e contratos JSON, com resultados no log do Actions |
| container | Runner GitHub Ubuntu 24.04 | Build Docker, Compose com SQL Server real, readiness, frontend, Swagger, 401 anônimo, login JWT, medidores e ESG |
| publicação | Mesmo job `container`; somente em `main` | Publica a imagem com a tag do commit e obtém a referência imutável por digest SHA-256 |
| deploy-staging | Runner GitHub Ubuntu; comandos via SSH na VM Azure | Pull da imagem por digest, Compose, smoke test. Não recompila a imagem |
| deploy-production | Runner GitHub Ubuntu; comandos via SSH na VM Azure | Executa após o staging aprovado, com exatamente o mesmo digest e o mesmo smoke test |

```mermaid
flowchart TD
    A[Push ou Pull Request] --> B[Build e testes]
    B --> C[Imagem Docker e smoke com SQL]
    C --> D{Branch main e evento autorizado?}
    D -->|Sim| E[Publicar no GHCR]
    D -->|Não| F[Encerrar validação CI]
    E --> G[Deploy staging e smoke]
    G --> H[Deploy produção e smoke]
```

Pull requests executam apenas o CI e não publicam nem fazem deploy. Push em `main` e execução manual em `main` executam o fluxo completo. Uma falha bloqueia as etapas seguintes. A produção é promovida automaticamente após o staging (deployment contínuo, sem aprovação manual).

### Infraestrutura de staging e produção

Os dois ambientes são hospedados em uma VM Azure. GitHub Actions conecta por SSH e envia os arquivos de deploy e smoke do commit, sem precisar clonar o repositório na VM. API e SQL Server permanecem ativos após o job; volumes são preservados entre releases.

| Item | Configuração de referência |
| --- | --- |
| VM | `vm-hydroguard`, Ubuntu 24.04 x64, Standard B2as v2 (2 vCPUs, 8 GiB) |
| Região / grupo | South Africa North / `rg-hydroguard` |
| Deploy | Runner GitHub com SSH; sem agente self-hosted na VM |
| Stacks | `hydroguard-staging` e `hydroguard-production`, com bancos e volumes independentes |
| Portas | `127.0.0.1:5001` e `127.0.0.1:5000`, acessadas por túnel SSH |

Configure os environments `staging` e `production`, restritos a `main`. Cada um precisa dos secrets `SSH_HOST`, `SSH_USER`, `SSH_PRIVATE_KEY`, `SSH_KNOWN_HOSTS`, `MSSQL_SA_PASSWORD`, `JWT_KEY` e `ADMIN_PASSWORD`. As informações SSH podem ser iguais; senhas e JWT devem ser diferentes. O GHCR usa `GITHUB_TOKEN` automático. `API_PORT` é opcional; `APP_URL` e `BIND_ADDRESS` não são utilizadas pelo workflow de deploy atual.

O acesso SSH verifica a identidade da VM com uma chave de servidor confiável. A porta 22 deve aceitar o runner do GitHub; permitir somente o IP do seu computador não basta. A autenticação temporária do GHCR é removida ao final; nenhum pacote com credenciais é publicado. Os testes permanecem em C#; Python não é necessário para o pipeline ou deploy.

Consulte [docs/DEPLOY-AZURE.md](docs/DEPLOY-AZURE.md) para preparação da VM, chaves, rede, configuração dos secrets, execução, evidências e diagnóstico.

### Falhas e rollback

Se o smoke de staging falhar, a produção não é promovida. Não há rollback automático: para voltar, execute `scripts/deploy.sh` no host com `API_IMAGE` apontando para o digest anterior e `DEPLOY_ENV` correspondente. Reverter a imagem não reverte migrations, então use apenas versões compatíveis com o schema atual. O modelo usa uma réplica da API por ambiente e pode ter breve indisponibilidade ao recriar o container.

## Containerização

Dockerfile multi-stage: o SDK restaura e publica a aplicação; a imagem final contém só o runtime ASP.NET e o `curl` para o health check, sem SDK nem código-fonte. A restauração precede a cópia do código para aproveitar o cache de camadas. A API roda como usuário não-root `app`, escuta na porta 8080 e recebe os segredos por variáveis de ambiente em runtime. A revisão do commit é incluída na imagem e aparece em `/healthz`.

```dockerfile
# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY src/HydroGuard.API/HydroGuard.API.csproj src/HydroGuard.API/
RUN dotnet restore src/HydroGuard.API/HydroGuard.API.csproj
COPY src/HydroGuard.API/ src/HydroGuard.API/
RUN dotnet publish src/HydroGuard.API/HydroGuard.API.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build --chown=app:app /app/publish .
ARG APP_REVISION=local
ARG APP_SOURCE=
LABEL org.opencontainers.image.revision=$APP_REVISION org.opencontainers.image.source=$APP_SOURCE
ENV ASPNETCORE_URLS=http://+:8080 HydroGuard__Revision=$APP_REVISION
USER app
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=60s --retries=10 CMD curl -fsS http://localhost:8080/healthz || exit 1
ENTRYPOINT ["dotnet", "HydroGuard.API.dll"]
```

O `docker-compose.yml` orquestra a API e o SQL Server 2022 Express, com volume persistente, uma rede `frontend` e uma rede `database` interna (`internal: true`). A porta 1433 não é publicada. `depends_on: service_healthy` evita iniciar a API antes de o SQL Server estar pronto, e `up --wait` aguarda também o readiness da API.

Tag da imagem: `ghcr.io/felipemm-santos/hydroguard:<commit>`. No deploy, a imagem é referenciada por digest imutável (`ghcr.io/felipemm-santos/hydroguard@sha256:...`), e a mesma imagem é promovida de staging para produção.

## Prints

Evidências da execução **#4** do workflow `HydroGuard CI-CD` (commit `b67d754`, branch `main`).

### Pipeline

**Execução completa com os 4 jobs aprovados** (build-test 28s, container 1m 34s, deploy-staging 1m 5s, deploy-production 9m 30s; total 18m 15s):

![Resumo do pipeline](docs/prints/01-pipeline-resumo.png)

**Testes automatizados: 20 aprovados, 0 falhas:**

![Testes](docs/prints/02-testes-20-passed.png)

### Staging (porta 5001)

**Dashboard ESG:**

![Staging - dashboard](docs/prints/03-staging-dashboard.png)

**Medidores cadastrados (seriais `HG-STG-*`):**

![Staging - medidores](docs/prints/04-staging-medidores.png)

**Histórico de consumos:**

![Staging - consumos](docs/prints/05-staging-consumos.png)

**Alerta de vazamento detectado (severidade Alta):**

![Staging - alertas](docs/prints/06-staging-alertas.png)

### Produção (porta 5000)

**Dashboard ESG:**

![Produção - dashboard](docs/prints/07-producao-dashboard.png)

**Medidores cadastrados (seriais `HG-PRD-*`):**

![Produção - medidores](docs/prints/08-producao-medidores.png)

**Histórico de consumos:**

![Produção - consumos](docs/prints/09-producao-consumos.png)

**Detecção de vazamentos sem ocorrências (cenário saudável):**

![Produção - alertas](docs/prints/10-producao-alertas.png)

### Containers, redes e volumes na VM

Containers `healthy` nos dois projetos Compose, dois volumes `sqlserver_data` separados, banco sem porta publicada no host, e **o mesmo digest `sha256:c0ba08b5...` da imagem da API nos dois ambientes**, o que comprova a promoção da mesma imagem de staging para produção:

![Containers e volumes](docs/prints/11-containers-volumes-vm.png)
![Log Staging](docs/prints/12-log-staging.png)
![Log Produção](docs/prints/13-log-producao.png)

## Tecnologias utilizadas

- C# e ASP.NET Core 8; Entity Framework Core 8 com migrations.
- SQL Server 2022 Express; BCrypt; autenticação JWT.
- HTML, CSS e JavaScript; Swagger/OpenAPI.
- xUnit, WebApplicationFactory, EF InMemory e Coverlet.
- Docker, Docker Compose v2, GitHub Actions, GitHub Container Registry (GHCR), runners hospedados e deploy remoto por SSH.
- Microsoft Azure (VM Ubuntu 24.04 x64) como host de staging e produção.
- C#/.NET 8 para todos os testes e para o smoke HTTP; Bash para o deploy.
- Python somente no gerador opcional de `.env`; não é usado no pipeline ou deploy.

## Estrutura do projeto

```text
.
├── Dockerfile
├── docker-compose.yml
├── .env.example
├── HydroGuard.sln
├── .github/workflows/ci-cd.yml
├── scripts/            (deploy.sh, deploy-ssh.sh, deploy-remote.sh, setup_env.py)
├── src/HydroGuard.API/ (API, frontend em wwwroot, migrations)
├── tests/              (HydroGuard.Tests e HydroGuard.SmokeTests)
└── docs/               (documentação técnica e prints)
```

## Desafios encontrados e soluções

| Desafio | Solução |
| --- | --- |
| Os 20 testes falharam na primeira execução do CI: a `Jwt:Key` configurada pela factory de testes chegava ao `Program.cs` vazia, porque `ConfigureAppConfiguration` é aplicado depois da leitura no hosting mínimo | Troca por `builder.UseSetting("Jwt:Key", ...)` na `HydroGuardWebApplicationFactory`, que é aplicada antes do `Program.cs` |
| Tamanho de VM indisponível e cota zero em uma região do Azure (`QuotaExceeded`) | Escolha de uma VM com 2 vCPU e 8 GiB e cota liberada na assinatura |
| Hospedar ambientes sem instalar agentes do GitHub | VM única Azure acessada por SSH e projetos Compose separados |
| Segredos fora do código em um repositório público | GitHub Environments com secrets e variables distintos por ambiente e deploy restrito à branch `main` |
| API iniciando antes do SQL Server | Health check do SQL, `depends_on: service_healthy` e readiness da API com `up --wait` |
| Staging e produção colidindo no mesmo host | Projetos Compose, portas, redes e volumes distintos |
| Ambientes sem URL pública | Portas publicadas só em `127.0.0.1` e acesso por túnel SSH nas demonstrações |

## Checklist de entrega da versão por SSH

- [x] Dockerfile, Compose, aplicação e testes C# organizados.
- [x] Pipeline adaptado para deploy via SSH em VM Azure.
- [x] Promoção do mesmo digest, resultados no log e persistência dos volumes.
- [x] Roteiro de configuração atualizado em `docs/DEPLOY-AZURE.md`.
- [x] Confirmar SDK .NET 8 na VM e configurar os secrets SSH nos dois environments.
- [x] Validar a execução real dos quatro jobs desta versão em `main`.
- [x] Capturar prints dos testes, digest e resultados dos deploys.
- [x] Capturar novos prints e comprovar isolamento/persistência após redeploy.

## Referências técnicas

- [GitHub Actions: controle de deployments](https://docs.github.com/en/actions/how-tos/deploy/configure-and-manage-deployments/control-deployments)
- [GitHub: secrets](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets)
- [GitHub Container Registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry)
- [Docker: ordem de inicialização](https://docs.docker.com/compose/how-tos/startup-order/)
- [Microsoft: SQL Server em containers](https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver16)
- [Microsoft: testes de integração ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0)
