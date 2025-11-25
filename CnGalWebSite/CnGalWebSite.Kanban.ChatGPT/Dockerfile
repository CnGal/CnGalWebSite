#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
FROM littlefishtentears/dotnet-with-curl:10.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.Kanban.ChatGPT/CnGalWebSite.Kanban.ChatGPT.csproj", "CnGalWebSite/CnGalWebSite.Kanban.ChatGPT/"]
COPY ["CnGalWebSite/CnGalWebSite.Core/CnGalWebSite.Core.csproj", "CnGalWebSite/CnGalWebSite.Core/"]
COPY ["CnGalWebSite/CnGalWebSite.EventBus/CnGalWebSite.EventBus.csproj", "CnGalWebSite/CnGalWebSite.EventBus/"]
COPY ["CnGalWebSite/CnGalWebSite.Extensions/CnGalWebSite.Extensions.csproj", "CnGalWebSite/CnGalWebSite.Extensions/"]
RUN dotnet restore "./CnGalWebSite/CnGalWebSite.Kanban.ChatGPT/CnGalWebSite.Kanban.ChatGPT.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.Kanban.ChatGPT"
RUN dotnet build "./CnGalWebSite.Kanban.ChatGPT.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CnGalWebSite.Kanban.ChatGPT.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.Kanban.ChatGPT.dll"]
