#!/bin/bash
set -e

until  ~/.dotnet/tools/dotnet-ef database update --project ../src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj; do
    >&2 echo "inicializando o banco de dados de notificação EF Core"
    sleep 1
done

>&2 echo "migração executada no postgresql"
exec $@
