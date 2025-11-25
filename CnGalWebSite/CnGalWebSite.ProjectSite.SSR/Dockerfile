#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM littlefishtentears/dotnet-with-curl:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.ProjectSite.SSR/CnGalWebSite.ProjectSite.SSR.csproj", "CnGalWebSite/CnGalWebSite.ProjectSite.SSR/"]
COPY ["CnGalWebSite/CnGalWebSite.ProjectSite.Shared/CnGalWebSite.ProjectSite.Shared.csproj", "CnGalWebSite/CnGalWebSite.ProjectSite.Shared/"]
COPY ["CnGalWebSite/CnGalWebSite.Components/CnGalWebSite.Components.csproj", "CnGalWebSite/CnGalWebSite.Components/"]
COPY ["CnGalWebSite/CnGalWebSite.Core/CnGalWebSite.Core.csproj", "CnGalWebSite/CnGalWebSite.Core/"]
COPY ["CnGalWebSite/CnGalWebSite.DrawingBed.Helper/CnGalWebSite.DrawingBed.Helper.csproj", "CnGalWebSite/CnGalWebSite.DrawingBed.Helper/"]
COPY ["CnGalWebSite/CnGalWebSite.DrawingBed.Models/CnGalWebSite.DrawingBed.Models.csproj", "CnGalWebSite/CnGalWebSite.DrawingBed.Models/"]
COPY ["CnGalWebSite/CnGalWebSite.Extensions/CnGalWebSite.Extensions.csproj", "CnGalWebSite/CnGalWebSite.Extensions/"]
COPY ["CnGalWebSite/CnGalWebSite.ProjectSite.Models/CnGalWebSite.ProjectSite.Models.csproj", "CnGalWebSite/CnGalWebSite.ProjectSite.Models/"]
RUN dotnet restore "CnGalWebSite/CnGalWebSite.ProjectSite.SSR/CnGalWebSite.ProjectSite.SSR.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.ProjectSite.SSR"
RUN dotnet build "CnGalWebSite.ProjectSite.SSR.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "CnGalWebSite.ProjectSite.SSR.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.ProjectSite.SSR.dll"]
