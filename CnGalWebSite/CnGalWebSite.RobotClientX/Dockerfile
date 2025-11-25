#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
FROM littlefishtentears/dotnet-with-curl:10.0 AS base

WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.RobotClientX/CnGalWebSite.RobotClientX.csproj", "CnGalWebSite/CnGalWebSite.RobotClientX/"]
COPY ["CnGalWebSite/CnGalWebSite.Components/CnGalWebSite.Components.csproj", "CnGalWebSite/CnGalWebSite.Components/"]
COPY ["CnGalWebSite/CnGalWebSite.Core/CnGalWebSite.Core.csproj", "CnGalWebSite/CnGalWebSite.Core/"]
COPY ["CnGalWebSite/CnGalWebSite.Extensions/CnGalWebSite.Extensions.csproj", "CnGalWebSite/CnGalWebSite.Extensions/"]
COPY ["CnGalWebSite/CnGalWebSite.RobotClientX.Models/CnGalWebSite.RobotClientX.Models.csproj", "CnGalWebSite/CnGalWebSite.RobotClientX.Models/"]
RUN dotnet restore "CnGalWebSite/CnGalWebSite.RobotClientX/CnGalWebSite.RobotClientX.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.RobotClientX"
RUN dotnet build "CnGalWebSite.RobotClientX.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "CnGalWebSite.RobotClientX.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.RobotClientX.dll"]
