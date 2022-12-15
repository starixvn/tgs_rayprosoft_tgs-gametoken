#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameToken/Tgs.GameToken.csproj", "Tgs.GameToken/"]
COPY ["Tgs.GameToken.Core/Tgs.GameToken.Core.csproj", "Tgs.GameToken.Core/"]
COPY ["Tgs.GameToken.Repositories/Tgs.GameToken.Repositories.csproj", "Tgs.GameToken.Repositories/"]
RUN dotnet restore "Tgs.GameToken/Tgs.GameToken.csproj"
COPY . .
WORKDIR "/src/Tgs.GameToken"
RUN dotnet build "Tgs.GameToken.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.GameToken.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameToken.dll"]