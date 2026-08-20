#    syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /build

COPY global.json ./
COPY ExpenseLedgerSolution.slnx src/
COPY NuGet.config src/


COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Host/Host.csproj src/Host/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/

# .NET follows the project references 
RUN --mount=type=cache,target=/build/src/.nuget/packages \
    dotnet restore src/Host/Host.csproj
# Any other src file
COPY src src/

RUN --mount=type=cache,target=/build/src/.nuget/packages \
    dotnet build \
    src/Host/Host.csproj \
    -c Release \
    --no-restore


FROM build AS publish

RUN --mount=type=cache,target=/build/src/.nuget/packages \
    dotnet publish \
    src/Host/Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-build


FROM runtime AS final

WORKDIR /app

COPY --from=publish /app/publish .

RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "Host.dll"]
