# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/AutoWork.API/AutoWork.API.csproj", "src/AutoWork.API/"]
COPY ["src/AutoWork.Application/AutoWork.Application.csproj", "src/AutoWork.Application/"]
COPY ["src/AutoWork.Domain/AutoWork.Domain.csproj", "src/AutoWork.Domain/"]
COPY ["src/AutoWork.Infrastructure/AutoWork.Infrastructure.csproj", "src/AutoWork.Infrastructure/"]
COPY ["src/AutoWork.Persistence/AutoWork.Persistence.csproj", "src/AutoWork.Persistence/"]
COPY ["src/AutoWork.Shared/AutoWork.Shared.csproj", "src/AutoWork.Shared/"]

RUN dotnet restore "src/AutoWork.API/AutoWork.API.csproj"

COPY . .
WORKDIR "/src/src/AutoWork.API"
RUN dotnet publish "AutoWork.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

RUN mkdir -p /app/logs /app/wwwroot/uploads

COPY --from=build /app/publish .

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/swagger/index.html || exit 1

ENTRYPOINT ["dotnet", "AutoWork.API.dll"]
