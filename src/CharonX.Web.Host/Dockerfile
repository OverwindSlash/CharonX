#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

#FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
FROM 10.10.1.221:8086/library/aspdotnet-base:beta AS build
#此处略显尴尬：公司网络问题导致不能restore,我在家做aspdotnet-base镜像时忘记删除其中的src,该image已传到公司harbor，不想再更换
RUN rm -rf /src
WORKDIR /src
#COPY ["src/CharonX.Web.Host/nuget.config", "src/CharonX.Web.Host/"]
COPY ["src/CharonX.Web.Host/CharonX.Web.Host.csproj", "src/CharonX.Web.Host/"]
COPY ["src/CharonX.Web.Core/CharonX.Web.Core.csproj", "src/CharonX.Web.Core/"]
COPY ["src/CharonX.EntityFrameworkCore/CharonX.EntityFrameworkCore.csproj", "src/CharonX.EntityFrameworkCore/"]
COPY ["src/CharonX.Core/CharonX.Core.csproj", "src/CharonX.Core/"]
COPY ["src/CharonX.Application/CharonX.Application.csproj", "src/CharonX.Application/"]
RUN dotnet restore "src/CharonX.Web.Host/CharonX.Web.Host.csproj"

COPY . .
WORKDIR "/src/src/CharonX.Web.Host"
RUN dotnet build "CharonX.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CharonX.Web.Host.csproj" -c Release -o /app/publish

#WORKDIR /test
#COPY ["test/CharonX.Tests/CharonX.Tests.csproj", "test/CharonX.Tests/"]
#RUN dotnet restore "test/CharonX.Tests/CharonX.Tests.csproj"
#COPY . .
#WORKDIR "/test/test/CharonX.Tests"
#RUN dotnet test "CharonX.Tests.csproj"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ComPlus_ThreadPool_ForceMinWorkerThreads=200
ENV TZ=Asia/Shanghai
ENTRYPOINT ["dotnet", "CharonX.Web.Host.dll"]