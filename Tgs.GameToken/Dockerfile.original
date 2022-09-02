#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["GameTokenService/GameTokenService.csproj", "GameTokenService/"]
COPY ["GameToken.Repositories/GameToken.Repositories.csproj", "GameToken.Repositories/"]
COPY ["GameToken.Core/GameToken.Core.csproj", "GameToken.Core/"]
RUN dotnet restore "GameTokenService/GameTokenService.csproj"
COPY . .
WORKDIR "/src/GameTokenService"
RUN dotnet build "GameTokenService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "GameTokenService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameTokenService.dll"]
