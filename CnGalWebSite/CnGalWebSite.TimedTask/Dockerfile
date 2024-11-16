#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM littlefishtentears/dotnet-with-curl:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.TimedTask/CnGalWebSite.TimedTask.csproj", "CnGalWebSite/CnGalWebSite.TimedTask/"]
COPY ["CnGalWebSite/CnGalWebSite.Core/CnGalWebSite.Core.csproj", "CnGalWebSite/CnGalWebSite.Core/"]
COPY ["CnGalWebSite/CnGalWebSite.EventBus/CnGalWebSite.EventBus.csproj", "CnGalWebSite/CnGalWebSite.EventBus/"]
COPY ["CnGalWebSite/CnGalWebSite.Extensions/CnGalWebSite.Extensions.csproj", "CnGalWebSite/CnGalWebSite.Extensions/"]
COPY ["CnGalWebSite/CnGalWebSite.TimedTask.Models/CnGalWebSite.TimedTask.Models.csproj", "CnGalWebSite/CnGalWebSite.TimedTask.Models/"]
RUN dotnet restore "./CnGalWebSite/CnGalWebSite.TimedTask/./CnGalWebSite.TimedTask.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.TimedTask"
RUN dotnet build "./CnGalWebSite.TimedTask.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CnGalWebSite.TimedTask.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.TimedTask.dll"]
