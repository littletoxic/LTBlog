# 请参阅 https://aka.ms/customizecontainer 以了解如何自定义调试容器，以及 Visual Studio 如何使用此 Dockerfile 生成映像以更快地进行调试。

# 上下文文件夹为 ..
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled-extra AS base
WORKDIR /app

# 此阶段用于生成服务项目
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY --link ["LTBlog/LTBlog.csproj", "LTBlog/"]
COPY --link ["LTBlog.Client/LTBlog.Client.csproj", "LTBlog.Client/"]
RUN dotnet restore "./LTBlog/LTBlog.csproj"
COPY --link . .
WORKDIR "/src/LTBlog"
RUN dotnet build "./LTBlog.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此阶段用于发布要复制到最终阶段的服务项目
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
RUN dotnet publish "./LTBlog.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此阶段在生产中使用，或在常规模式下从 VS 运行时使用(在不使用调试配置时为默认值)
FROM base AS final
EXPOSE 8080
WORKDIR /app
COPY --link --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "LTBlog.dll"]