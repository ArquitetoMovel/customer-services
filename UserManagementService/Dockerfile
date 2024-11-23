FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UserManagement.Api/UserManagement.Api.csproj", "src/UserManagement.Api/"]
COPY ["src/UserManagement.Application/UserManagement.Application.csproj", "src/UserManagement.Application/"]
COPY ["src/UserManagement.Domain/UserManagement.Domain.csproj", "src/UserManagement.Domain/"]
COPY ["src/UserManagement.Infrastructure/UserManagement.Infrastructure.csproj", "src/UserManagement.Infrastructure/"]
RUN dotnet restore "src/UserManagement.Api/UserManagement.Api.csproj"
COPY . .
WORKDIR "src/UserManagement.Api"
RUN dotnet build "UserManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UserManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserManagement.Api.dll"]
