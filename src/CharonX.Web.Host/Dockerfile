#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
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

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CharonX.Web.Host.dll"]