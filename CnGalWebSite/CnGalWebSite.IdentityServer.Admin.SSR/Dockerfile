#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
FROM littlefishtentears/dotnet-with-curl:7.0.3 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.IdentityServer.Admin.SSR/CnGalWebSite.IdentityServer.Admin.SSR.csproj", "CnGalWebSite/CnGalWebSite.IdentityServer.Admin.SSR/"]
COPY ["CnGalWebSite/CnGalWebSite.IdentityServer.Admin.Shared/CnGalWebSite.IdentityServer.Admin.Shared.csproj", "CnGalWebSite/CnGalWebSite.IdentityServer.Admin.Shared/"]
COPY ["CnGalWebSite/CnGalWebSite.Extensions/CnGalWebSite.Extensions.csproj", "CnGalWebSite/CnGalWebSite.Extensions/"]
COPY ["CnGalWebSite/CnGalWebSite.IdentityServer.Models/CnGalWebSite.IdentityServer.Models.csproj", "CnGalWebSite/CnGalWebSite.IdentityServer.Models/"]
RUN dotnet restore "CnGalWebSite/CnGalWebSite.IdentityServer.Admin.SSR/CnGalWebSite.IdentityServer.Admin.SSR.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.IdentityServer.Admin.SSR"
RUN dotnet build "CnGalWebSite.IdentityServer.Admin.SSR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CnGalWebSite.IdentityServer.Admin.SSR.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.IdentityServer.Admin.SSR.dll"]
