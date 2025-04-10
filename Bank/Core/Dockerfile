# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Core/Core.csproj", "Core/"]
COPY ["Core.Lib/Core.BL/Core.BL.csproj", "Core.Lib/Core.BL/"]
COPY ["Bank.BL/Bank.BL.csproj", "Bank.BL/"]
COPY ["Core.Lib/Core.DAL/Core.DAL.csproj", "Core.Lib/Core.DAL/"]
COPY ["Bank.DAL/Bank.DAL.csproj", "Bank.DAL/"]
COPY ["Core.Lib/Core.DTO/Core.DTO.csproj", "Core.Lib/Core.DTO/"]
COPY ["Bank.DTO/Bank.DTO.csproj", "Bank.DTO/"]
RUN dotnet restore "./Core/Core.csproj"
COPY . .
WORKDIR "/src/Core"
RUN dotnet build "./Core.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Core.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Core.dll"]