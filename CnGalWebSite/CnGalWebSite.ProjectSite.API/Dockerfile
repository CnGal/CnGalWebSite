#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM littlefishtentears/dotnet-with-curl:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.ProjectSite.API/CnGalWebSite.ProjectSite.API.csproj", "CnGalWebSite/CnGalWebSite.ProjectSite.API/"]
COPY ["CnGalWebSite/CnGalWebSite.Core/CnGalWebSite.Core.csproj", "CnGalWebSite/CnGalWebSite.Core/"]
COPY ["CnGalWebSite/CnGalWebSite.DataModel/CnGalWebSite.DataModel.csproj", "CnGalWebSite/CnGalWebSite.DataModel/"]
COPY ["CnGalWebSite/CnGalWebSite.Extensions/CnGalWebSite.Extensions.csproj", "CnGalWebSite/CnGalWebSite.Extensions/"]
COPY ["CnGalWebSite/CnGalWebSite.HealthCheck/CnGalWebSite.HealthCheck.csproj", "CnGalWebSite/CnGalWebSite.HealthCheck/"]
COPY ["CnGalWebSite/CnGalWebSite.ProjectSite.Models/CnGalWebSite.ProjectSite.Models.csproj", "CnGalWebSite/CnGalWebSite.ProjectSite.Models/"]
RUN dotnet restore "CnGalWebSite/CnGalWebSite.ProjectSite.API/CnGalWebSite.ProjectSite.API.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.ProjectSite.API"
RUN dotnet build "CnGalWebSite.ProjectSite.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "CnGalWebSite.ProjectSite.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.ProjectSite.API.dll"]
