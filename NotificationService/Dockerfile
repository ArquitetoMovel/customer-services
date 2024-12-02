FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
RUN apt-get update && apt-get install -y iputils-ping telnet
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/NotificationService.Api/NotificationService.Api.csproj", "src/NotificationService.Api/"]
COPY ["src/NotificationService.Domain/NotificationService.Domain.csproj", "src/NotificationService.Domain/"]
COPY ["src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/NotificationService.Infrastructure/"]
COPY ["src/NotificationService.Application/NotificationService.Application.csproj", "src/NotificationService.Application/"]
RUN dotnet restore "src/NotificationService.Api/NotificationService.Api.csproj"
COPY . .
WORKDIR "src/NotificationService.Api"
RUN dotnet build "NotificationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NotificationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /src
COPY --chmod=777 --from=build /src/src .
WORKDIR /app
COPY --from=publish /app/publish .
# COPY --chmod=755 init-notificationDB.sh .

# Instalar a ferramenta dotnet-ef globalmente
RUN dotnet tool install --global dotnet-ef

#  ENTRYPOINT ["./init-notificationDB.sh"]
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
