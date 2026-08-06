#    syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ExpenseLedgerSolution.slnx .
COPY NuGet.config .


COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Host/Host.csproj src/Host/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY Tests/UnitTests/UnitTests.csproj /Tests/UnitTests/


# .NET follows the project references and end up restoring every project except IntegrationTests.csproj
RUN dotnet restore src/Host/Host.csproj && \
    dotnet restore /Tests/UnitTests/UnitTests.csproj

# Any other src file
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