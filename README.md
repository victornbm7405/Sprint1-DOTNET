# MottuProjeto API – Sprint .NET (FIAP)

API RESTful em **.NET 8 Web API** para gestão de **Usuários**, **Motos** e **Áreas**, com:

- ✅ **JWT** (login → Bearer token via Swagger “Authorize”)  
- ✅ **Versionamento** (`/api/v1/...`)  
- ✅ **Swagger** por versão com documentação XML  
- ✅ **Health Checks** (`/healthz`, `/healthz/ready`)  
- ✅ **ML.NET** (predição de risco de manutenção por telemetria)  
- ✅ **Testes**: unitários e de integração com `WebApplicationFactory<Program>`

> Este README foi ajustado para o repositório visível na captura: contém **`coverage-report/`** e a **solution `MottuProjeto.sln`**.  
> Removido: instruções de variável de sistema e seção com comandos `curl` como solicitado.

---

## Sumário

- [Estrutura do Repositório](#estrutura-do-repositório)  
- [Requisitos](#requisitos)  
- [Configuração](#configuração)  
- [Como Executar (via Solution)](#como-executar-via-solution)  
- [Usando o Swagger](#usando-o-swagger)  
- [Endpoints Principais](#endpoints-principais)  
- [ML.NET – Risco de Manutenção](#mlnet--risco-de-manutenção)  
- [Testes (xUnit) e Cobertura](#testes-xunit-e-cobertura)  
- [Checklist da Avaliação (como comprovar)](#checklist-da-avaliação-como-comprovar)  
- [Troubleshooting](#troubleshooting)  
- [Notas de Segurança & Boas Práticas](#notas-de-segurança--boas-práticas)

---

## Estrutura do Repositório

```
Controllers/                     # Endpoints REST (Auth, Health, Usuarios, Areas, Motos, ML)
coverage-report/                 # Saída de relatórios de cobertura (HTML), quando gerados
Data/                            # AppDbContext, fábrica, migrações EF
DTOs/                            # Contratos de entrada/saída (Login, Usuario, Moto)
Infrastructure/                  # Helpers (PaginationHelper, HateoasExtensions)
Migrations/                      # EF Core Migrations
ML/                              # Modelos e serviços ML.NET (telemetria → risco)
Models/                          # Entidades (Usuario, Moto, Area)

MottuProjeto.IntegrationTests/   # Testes de integração (WebApplicationFactory<Program>)
MottuProjeto.Tests/              # Smoke/auxiliares
MottuProjeto.UnitTests/          # Testes unitários (ex.: PaginationHelper)

appsettings.json                 # Config de ambiente (ex.: ConnectionStrings, Jwt)
MottuProjeto.csproj              # Projeto Web API (.NET 8)
MottuProjeto.sln                 # Solution consolidando todos os projetos
packages-ef.txt / packages-tree.txt  # Listagem de pacotes (inventário)
Program.cs                       # Bootstrap (DI, JWT, Versionamento, Swagger, HealthChecks)
README.md                        # Este arquivo
*.bak                            # Backups do .csproj (podem ser removidos se não forem mais necessários)
```

---

## Requisitos

- **.NET 8 SDK**
- Banco Oracle (opcional para rodar CRUD real; health liveness funciona sem DB, mas *readiness* requer conexão)

---

## Configuração

### `appsettings.json`
Defina *apenas aqui* a conexão (sem variáveis de sistema):

```json
{
  "ConnectionStrings": {
    "Default": "User Id=USUARIO;Password=SENHA;Data Source=HOST:1521/SERVICO"
  },
  "Jwt": {
    "Key": "CHAVE_SECRETA_DEV"
  }
}
```

### Swagger – XML docs (já preparado)
O projeto já está configurado para gerar o XML de documentação no build e carregá-lo no Swagger.

- `MottuProjeto.csproj`:
  ```xml
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
  ```
- `Program.cs` (trecho):
  ```csharp
  var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xml), includeControllerXmlComments: true);
  ```

---

## Como Executar (via Solution)

> **Recomendado** usar a solution para restaurar/compilar **todos os projetos** (API + testes).

```bash
# Restaurar tudo
dotnet restore MottuProjeto.sln

# Build completo
dotnet build MottuProjeto.sln -c Debug

# Executar a API
dotnet run --project MottuProjeto.csproj
```
- A porta e a URL base aparecem no console (ex.: `http://localhost:5000`).

---

## Usando o Swagger

1. Abra o navegador em: `http://localhost:5000/swagger/index.html`.  
2. Clique em **Authorize**, cole o **JWT** obtido pelo **login**:  
   - `POST /api/v1/auth/login` (via Swagger), body: `{ "username":"admin", "password":"admin123" }`  
   - O Swagger retornará `{ token: "<JWT>" }`; use `Bearer <JWT>` no botão **Authorize**.
3. Execute as rotas protegidas (ex.: **ML → `/api/v1/ml/risco-manutencao`**).

---

## Endpoints Principais

### Health
- `GET /healthz` → 200 `Healthy` (liveness)
- `GET /healthz/ready` → 200 se DB ok; 503 caso contrário (readiness)

### Auth
- `POST /api/v1/auth/login` → retorna **JWT** (necessário para rotas protegidas).

### Usuários
- `GET /api/v1/usuarios`
- `GET /api/v1/usuarios/{id}`
- `POST /api/v1/usuarios`
- `PUT /api/v1/usuarios/{id}`
- `DELETE /api/v1/usuarios/{id}`

### Áreas
- CRUD em `/api/v1/areas`.

### Motos
- CRUD em `/api/v1/motos` (paginação opcional via `page`/`size` conforme `Infrastructure/PaginationHelper`).

### ML – Risco de Manutenção (JWT)
- `POST /api/v1/ml/risco-manutencao`  
  Body:
  ```json
  { "tempC": 61.5, "vib": 0.38, "battPct": 52 }
  ```
  Resposta exemplo:
  ```json
  {
    "tempC": 61.5,
    "vib": 0.38,
    "battPct": 52.0,
    "predicted": true,
    "probability": 0.77,
    "nivel": "Alerta"
  }
  ```

---

## ML.NET – Risco de Manutenção

- **Features**: `TempC`, `Vib`, `BattPct`  
- **Pipeline**: `Concatenate("Features")` → `NormalizeMinMax` → `BinaryClassification.Trainers.SdcaLogisticRegression`  
- **Mapeamento do nível**:  
  - `probability >= 0.80` → **Risco**  
  - `probability >= 0.60` → **Alerta**  
  - senão → **Normal**

> **Dica**: garanta que `TelemetryInput/Prediction/Request/Response` existam **apenas uma vez** no projeto para evitar **CS0101**.

---

## Testes (xUnit) e Cobertura

### Rodar testes (todos os projetos)
```bash
dotnet test MottuProjeto.sln --collect:"XPlat Code Coverage"
```

### Projetos de teste incluídos
- **`MottuProjeto.UnitTests/`** – testes unitários (ex.: `PaginationHelperTests.cs`).  
- **`MottuProjeto.IntegrationTests/`** – testes de integração usando `WebApplicationFactory<Program>` (ex.: `HealthzTests.cs`).  
- **`MottuProjeto.Tests/`** – testes auxiliares/smoke.

### Abrir relatório de cobertura (HTML)
A pasta **`coverage-report/`** é o diretório destino para HTML.  
Se ainda não gerou o HTML, você pode usar o ReportGenerator:

```bash
# Instalar (uma vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Gerar HTML para coverage-report/
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```
Depois, abra `coverage-report/index.html` no navegador.

> Se o repositório já contém `coverage-report/`, basta abrir `index.html` diretamente.

---

## Checklist da Avaliação (como comprovar)

| Requisito |
|---|---|---|
| API RESTful (.NET 8, boas práticas) | ✅ | Controllers/DTOs/DI/EF; Swagger ligado |
| **Health Checks (10 pts)** | ✅ | Swagger → `GET /healthz` e `GET /healthz/ready` |
| **Versionamento (10 pts)** | ✅ | Rotas `/api/v1/...` e docs por `v1` no Swagger |
| **Segurança (JWT) (25 pts)** | ✅ | `POST /api/v1/auth/login` → botão Authorize; rotas ML protegidas |
| **Endpoint ML.NET (25 pts)** | ✅ | `POST /api/v1/ml/risco-manutencao` (Swagger) |
| **Unit tests (xUnit) (30 pts)** | ✅ | `dotnet test` (projetos `UnitTests/` presentes) |
| **Integração (WebApplicationFactory)** | ✅ | `MottuProjeto.IntegrationTests/HealthzTests.cs` |
| **README com instruções de testes** | ✅ | Seção “Testes (xUnit) e Cobertura” |
| **Swagger atualizado (–20 se faltar)** | ✅ | XML docs habilitado e visível no Swagger |
| **Projeto compila (–100 se falhar)** | ✅ | `dotnet build MottuProjeto.sln` sem erros |

---

## Troubleshooting

- **404 em `/api/v1/ml/risco-manutencao`**  
  Verifique a **porta** exibida no console e a **versão** da rota. Confirme no Swagger.

- **401 Unauthorized**  
  Faça login no Swagger (`/api/v1/auth/login`) e clique em **Authorize** com `Bearer <token>`.

- **CS0101 (namespace já contém uma definição)**  
  Algum tipo ML (`TelemetryInput/Prediction/...`) está duplicado em múltiplos arquivos. Remova as duplicatas.

- **Readiness 503**  
  O banco não está acessível. Ajuste a `ConnectionStrings:Default` no `appsettings.json` ou suba o Oracle.

- **Swagger sem documentação**  
  Certifique-se de compilar em **Debug/Release** (o XML é gerado no build).

---

## Desenvolvido por:

RM 556293 Alice Teixeira Caldeira 
RM 555708 Gustavo Goulart 
RM 554557 Victor Medeiros

