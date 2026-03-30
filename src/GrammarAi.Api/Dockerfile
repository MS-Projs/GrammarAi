FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/GrammarAi.Api/GrammarAi.Api.csproj", "src/GrammarAi.Api/"]
COPY ["src/GrammarAi.Application/GrammarAi.Application.csproj", "src/GrammarAi.Application/"]
COPY ["src/GrammarAi.Infrastructure/GrammarAi.Infrastructure.csproj", "src/GrammarAi.Infrastructure/"]
COPY ["src/GrammarAi.Domain/GrammarAi.Domain.csproj", "src/GrammarAi.Domain/"]
RUN dotnet restore "src/GrammarAi.Api/GrammarAi.Api.csproj"
COPY . .
RUN dotnet build "src/GrammarAi.Api/GrammarAi.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/GrammarAi.Api/GrammarAi.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GrammarAi.Api.dll"]
