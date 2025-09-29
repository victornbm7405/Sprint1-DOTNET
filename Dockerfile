# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo do repo
COPY . .

# Se o seu projeto web NÃO estiver na raiz, ajuste o caminho abaixo:
# ex.: RUN dotnet restore ./src/MottuProjeto/MottuProjeto.csproj
RUN dotnet restore ./MottuProjeto.csproj

# Publica em Release
# ex.: RUN dotnet publish ./src/MottuProjeto/MottuProjeto.csproj -c Release -o /app/out
RUN dotnet publish ./MottuProjeto.csproj -c Release -o /app/out

# ===== runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia artefatos publicados
COPY --from=build /app/out ./

# Porta interna do container
EXPOSE 8080

# Render injeta $PORT; localmente usa 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}

# Se o .csproj tiver outro nome, troque o DLL aqui
ENTRYPOINT ["dotnet","MottuProjeto.dll"]
