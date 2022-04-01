#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
FROM littlefishtentears/dotnet-with-curl:v6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.Server/CnGalWebSite.Server.csproj", "CnGalWebSite/CnGalWebSite.Server/"]
COPY ["CnGalWebSite/CnGalWebSite.Shared/CnGalWebSite.Shared.csproj", "CnGalWebSite/CnGalWebSite.Shared/"]
RUN dotnet restore "CnGalWebSite/CnGalWebSite.Server/CnGalWebSite.Server.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.Server"
RUN dotnet build "CnGalWebSite.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CnGalWebSite.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.Server.dll"]
