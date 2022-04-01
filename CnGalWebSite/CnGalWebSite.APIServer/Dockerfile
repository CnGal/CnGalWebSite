#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
FROM littlefishtentears/dotnet-with-curl:v6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.APIServer/CnGalWebSite.APIServer.csproj", "CnGalWebSite/CnGalWebSite.APIServer/"]
COPY ["CnGalWebSite/CnGalWebSite.DataModel/CnGalWebSite.DataModel.csproj", "CnGalWebSite/CnGalWebSite.DataModel/"]
RUN dotnet restore "CnGalWebSite/CnGalWebSite.APIServer/CnGalWebSite.APIServer.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.APIServer"
RUN dotnet build "CnGalWebSite.APIServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CnGalWebSite.APIServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.APIServer.dll"]
