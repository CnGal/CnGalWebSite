#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM littlefishtentears/dotnet-with-curl:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.BlazorWeb/CnGalWebSite.BlazorWeb.csproj", "CnGalWebSite/CnGalWebSite.BlazorWeb/"]
COPY ["CnGalWebSite/CnGalWebSite.HealthCheck/CnGalWebSite.HealthCheck.csproj", "CnGalWebSite/CnGalWebSite.HealthCheck/"]
COPY ["CnGalWebSite/CnGalWebSite.Core/CnGalWebSite.Core.csproj", "CnGalWebSite/CnGalWebSite.Core/"]
COPY ["CnGalWebSite/CnGalWebSite.Extensions/CnGalWebSite.Extensions.csproj", "CnGalWebSite/CnGalWebSite.Extensions/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared/CnGalWebSite.Shared.csproj", "CnGalWebSite/CnGalWebSite.Shared/"]
COPY ["CnGalWebSite/CnGalWebSite.Kanban/CnGalWebSite.Kanban.csproj", "CnGalWebSite/CnGalWebSite.Kanban/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared.MasaComponent/CnGalWebSite.Shared.MasaComponent.csproj", "CnGalWebSite/CnGalWebSite.Shared.MasaComponent/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared.Common/CnGalWebSite.Shared.Common.csproj", "CnGalWebSite/CnGalWebSite.Shared.Common/"]
COPY ["CnGalWebSite/CnGalWebSite.Components/CnGalWebSite.Components.csproj", "CnGalWebSite/CnGalWebSite.Components/"]
COPY ["CnGalWebSite/CnGalWebSite.DrawingBed.Helper/CnGalWebSite.DrawingBed.Helper.csproj", "CnGalWebSite/CnGalWebSite.DrawingBed.Helper/"]
COPY ["CnGalWebSite/CnGalWebSite.DrawingBed.Models/CnGalWebSite.DrawingBed.Models.csproj", "CnGalWebSite/CnGalWebSite.DrawingBed.Models/"]
COPY ["CnGalWebSite/CnGalWebSite.DataModel/CnGalWebSite.DataModel.csproj", "CnGalWebSite/CnGalWebSite.DataModel/"]
COPY ["CnGalWebSite/CnGalWebSite.Helper/CnGalWebSite.Helper.csproj", "CnGalWebSite/CnGalWebSite.Helper/"]
COPY ["CnGalWebSite/CnGalWebSite.PublicToolbox/CnGalWebSite.PublicToolbox.csproj", "CnGalWebSite/CnGalWebSite.PublicToolbox/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared.Service/CnGalWebSite.Shared.Service.csproj", "CnGalWebSite/CnGalWebSite.Shared.Service/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared.Extentions/CnGalWebSite.Shared.Extentions.csproj", "CnGalWebSite/CnGalWebSite.Shared.Extentions/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared.Models/CnGalWebSite.Shared.Models.csproj", "CnGalWebSite/CnGalWebSite.Shared.Models/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared.Layout/CnGalWebSite.Shared.Layout.csproj", "CnGalWebSite/CnGalWebSite.Shared.Layout/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared.Component/CnGalWebSite.Shared.Component.csproj", "CnGalWebSite/CnGalWebSite.Shared.Component/"]
RUN dotnet restore "./CnGalWebSite/CnGalWebSite.BlazorWeb/./CnGalWebSite.BlazorWeb.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.BlazorWeb"
RUN dotnet build "./CnGalWebSite.BlazorWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CnGalWebSite.BlazorWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.BlazorWeb.dll"]
