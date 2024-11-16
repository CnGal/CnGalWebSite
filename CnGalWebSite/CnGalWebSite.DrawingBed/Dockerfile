#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM littlefishtentears/dotnet-with-curl:9.0 AS base
RUN ["apt-get", "--assume-yes", "update"]
RUN ["apt-get", "--assume-yes", "install", "ffmpeg"]

USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.DrawingBed/CnGalWebSite.DrawingBed.csproj", "CnGalWebSite/CnGalWebSite.DrawingBed/"]
COPY ["CnGalWebSite/CnGalWebSite.Core/CnGalWebSite.Core.csproj", "CnGalWebSite/CnGalWebSite.Core/"]
COPY ["CnGalWebSite/CnGalWebSite.DrawingBed.Models/CnGalWebSite.DrawingBed.Models.csproj", "CnGalWebSite/CnGalWebSite.DrawingBed.Models/"]
COPY ["CnGalWebSite/CnGalWebSite.Extensions/CnGalWebSite.Extensions.csproj", "CnGalWebSite/CnGalWebSite.Extensions/"]
RUN dotnet restore "./CnGalWebSite/CnGalWebSite.DrawingBed/CnGalWebSite.DrawingBed.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.DrawingBed"
RUN dotnet build "./CnGalWebSite.DrawingBed.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CnGalWebSite.DrawingBed.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.DrawingBed.dll"]
