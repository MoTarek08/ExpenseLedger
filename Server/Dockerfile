#    syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ExpenseLedgerSolution.slnx .

COPY src/Domain/Domain.csproj Domain/
COPY src/Application/Application.csproj Application/
COPY src/Host/Host.csproj Host/
COPY src/Infrastructure/Infrastructure.csproj Infrastructure/
COPY Tests/UnitTests/UnitTests.csproj /Tests/UnitTests/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore ExpenseLedgerSolution.slnx

# Any other src file that is not .csproj (not related to dependencies)
COPY . .


RUN dotnet build \
    src/Host/Host.csproj \
    -c Release \
    --no-restore


# The child image already inherits all the parent's filesystem
FROM build AS test

RUN dotnet test \
    Tests/UnitTests/UnitTests.csproj  \
    -c Release \
    --no-restore 

FROM build AS publish

RUN dotnet publish \
    src/Host/Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-build


FROM runtime AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Host.dll"]