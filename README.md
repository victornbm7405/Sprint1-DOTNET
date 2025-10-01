# Mottu API — Gestão de Áreas, Motos e Usuários (ASP.NET Core .NET 8)

API RESTful construída com **ASP.NET Core (.NET 8)** e **Entity Framework Core** (Oracle) seguindo boas práticas REST:
**CRUD completo** para 3 entidades, **paginação**, **HATEOAS**, **status codes adequados** e **Swagger/OpenAPI com exemplos**.

---

## 🧭 Domínio e Justificativa
O domínio modela uma **gestão simples de frotas**:
- **Usuário**: representa quem acessa o sistema (operações administrativas, cadastro e consulta).
- **Área**: regiões/zonas operacionais (ex.: Zona Leste, Filial Campinas) para organizar as motos.
- **Moto**: ativo principal (placa, modelo, área associada).

**Por que este domínio?** É coeso, **realista** e **extensível** para cenários de negócios (telemetria, ordens de serviço, manutenção)

---

## 🏗️ Arquitetura (resumo)
- **Web API em .NET 8** com Controllers.
- **EF Core** + **Oracle** para persistência.
- **Boas práticas REST**: DTOs, paginação (`page`/`pageSize`), HATEOAS (`_links`), status codes (`201`, `204`, `404`, `409`).
- **Autenticação**: **Cookie** (pronta).
- **Swagger/OpenAPI** com **descrições** e **exemplos de payload** (via `Swashbuckle.AspNetCore.Filters`).

Estrutura típica de pastas:
```
MottuProjeto/
 ├─ Controllers/        # AreaController, MotosController, UsuariosController, AuthController
 ├─ Models/             # Entidades: Area, Moto, Usuario (DataAnnotations de mapeamento)
 ├─ DTOs/               # MotoDTO, UsuarioDTO, LoginRequestDTO etc.
 ├─ Data/               # AppDbContext (DbSets, índices, fluent config)
 ├─ Infrastructure/     # HATEOAS helpers
 ├─ Migrations/         # Migrações EF Core
 ├─ Program.cs          # Pipeline, Swagger, Auth, DI
 └─ appsettings.json    # ConnectionString (usar env em produção)
```

---

## 🔧 Tecnologias
- **.NET 8** (ASP.NET Core Web API)
- **Entity Framework Core** (Oracle)
- **Swashbuckle** (Swagger/OpenAPI) + `Swashbuckle.AspNetCore.Filters` (exemplos)
- **xUnit** / **WebApplicationFactory** (para testes de integração – opcional)

---

## 📦 Requisitos
- [.NET SDK 8.x](https://dotnet.microsoft.com/)
- Banco Oracle acessível
- Permissões de rede/firewall para acesso ao Oracle

### 1) Restaurar e compilar
```powershell
dotnet restore
dotnet build
```

### 2) Banco e migrações
Instale a CLI do EF (se não tiver):
```powershell
dotnet tool install --global dotnet-ef
```
Aplique as migrações:
```powershell
dotnet ef database update
```

### 3) Rodar a API
```powershell
dotnet run
```
- Base URL (padrão): **http://localhost:5000** (confirme no console)
- Swagger: **http://localhost:5000/swagger**

---

## 🔐 Autenticação
- **Cookie Auth** pronta: `POST /api/Auth/login` com `{{ admin, admin123 }}` cria a sessão; `GET /api/Auth/me` retorna o usuário atual; `POST /api/Auth/logout` encerra a sessão.
- Para **aplicativos mobile** (React Native/Expo), **considere JWT** (evita problemas de Cookie/CORS/SameSite).

---

## 🔗 Endpoints Principais (REST)
> Todos os Controllers seguem `api/[controller]` e **[Authorize]** (exceto login).

### Auth (`/api/Auth`)
- `POST /login` — body: `{{{{ "username":"admin", "password":"admin123" }}}}`  
- `GET /me` — retorna dados do usuário autenticado  
- `POST /logout` — encerra sessão

### Áreas (`/api/Area`)
- `GET /?page=1&pageSize=10` — lista paginada  
- `GET /{id}` — obtém por id  
- `POST /` — cria área (201 + Location)  
- `PUT /{id}` — atualiza área  
- `DELETE /{id}` — remove (204)

**Exemplo de create:**
```json
{{{{ "nome": "Zona Leste" }}}}
```

### Motos (`/api/Motos`)
- `GET /?page=1&pageSize=10`
- `GET /{id}`
- `POST /` — cria moto (201 + Location)
- `PUT /{id}`
- `DELETE /{id}`

**Exemplo de create:**
```json
{{{{ "placa": "ABC1D23", "modelo": "CG 160", "idArea": 1 }}}}
```

### Usuários (`/api/Usuarios`)
- `GET /?page=1&pageSize=10`
- `GET /{id}`
- `POST /`
- `PUT /{id}`
- `DELETE /{id}`

**Exemplo de create (não retornar PasswordHash em produção):**
```json
{{{{ "nome": "Fulano da Silva", "email": "fulano@example.com", "username": "fulano", "password": "123456", "role": "User" }}}}
```

---

## 📄 Swagger / OpenAPI
- Documentação em **/swagger** com **descrições** e **exemplos** de payload (configurar `Swashbuckle.AspNetCore.Filters`).
- Habilite **XML comments** no projeto para descrever ações e parâmetros.

---

## 📑 Paginação & HATEOAS
- **Paginação:** `page` (>=1) e `pageSize` (1–100). Respostas incluem contagem e itens.  
- **HATEOAS:** recursos singulares retornam `_links` (ex.: `self`, `update`, `delete`), facilitando navegação por links.

---

## 🧪 Testes
Com o projeto de testes configurado (ex.: `MottuProjeto.Tests`):
```powershell
dotnet test
```

---

Desenvolvido por:
RM 556293 Alice Teixeira Caldeira
RM 555708 Gustavo Goulart RM 554557 Victor Medeiros
