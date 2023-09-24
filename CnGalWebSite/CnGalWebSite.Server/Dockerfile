#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM littlefishtentears/dotnet-with-curl:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.Server/CnGalWebSite.Server.csproj", "CnGalWebSite/CnGalWebSite.Server/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared/CnGalWebSite.Shared.csproj", "CnGalWebSite/CnGalWebSite.Shared/"]
COPY ["CnGalWebSite/CnGalWebSite.DataModel/CnGalWebSite.DataModel.csproj", "CnGalWebSite/CnGalWebSite.DataModel/"]
COPY ["CnGalWebSite/CnGalWebSite.Helper/CnGalWebSite.Helper.csproj", "CnGalWebSite/CnGalWebSite.Helper/"]
COPY ["CnGalWebSite/CnGalWebSite.PublicToolbox/CnGalWebSite.PublicToolbox.csproj", "CnGalWebSite/CnGalWebSite.PublicToolbox/"]
RUN dotnet restore "CnGalWebSite/CnGalWebSite.Server/CnGalWebSite.Server.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.Server"
RUN dotnet build "CnGalWebSite.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CnGalWebSite.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.Server.dll"]
