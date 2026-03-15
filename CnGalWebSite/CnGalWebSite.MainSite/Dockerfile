# 请参阅 https://aka.ms/customizecontainer 以了解如何自定义调试容器，以及 Visual Studio 如何使用此 Dockerfile 生成映像以更快地进行调试。

# 此阶段用于在快速模式(默认为调试配置)下从 VS 运行时
FROM littlefishtentears/dotnet-with-curl:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# 此阶段用于生成服务项目
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CnGalWebSite/CnGalWebSite.MainSite/CnGalWebSite.MainSite.csproj", "CnGalWebSite/CnGalWebSite.MainSite/"]
COPY ["CnGalWebSite/CnGalWebSite.MainSite.Shared/CnGalWebSite.MainSite.Shared.csproj", "CnGalWebSite/CnGalWebSite.MainSite.Shared/"]
COPY ["CnGalWebSite/CnGalWebSite.Extensions/CnGalWebSite.Extensions.csproj", "CnGalWebSite/CnGalWebSite.Extensions/"]
COPY ["CnGalWebSite/CnGalWebSite.SDK.MainSite/CnGalWebSite.SDK.MainSite.csproj", "CnGalWebSite/CnGalWebSite.SDK.MainSite/"]
COPY ["CnGalWebSite/CnGalWebSite.DataModel/CnGalWebSite.DataModel.csproj", "CnGalWebSite/CnGalWebSite.DataModel/"]
COPY ["CnGalWebSite/CnGalWebSite.Core/CnGalWebSite.Core.csproj", "CnGalWebSite/CnGalWebSite.Core/"]
RUN dotnet restore "./CnGalWebSite/CnGalWebSite.MainSite/CnGalWebSite.MainSite.csproj"
COPY . .
WORKDIR "/src/CnGalWebSite/CnGalWebSite.MainSite"
RUN dotnet build "./CnGalWebSite.MainSite.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此阶段用于发布要复制到最终阶段的服务项目
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CnGalWebSite.MainSite.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此阶段在生产中使用，或在常规模式下从 VS 运行时使用(在不使用调试配置时为默认值)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnGalWebSite.MainSite.dll"]
