FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 443
EXPOSE 443/udp

ENV ASPNETCORE_HTTPS_PORTS=443
USER app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["LactoseSimulation/LactoseSimulation.csproj", "LactoseSimulation/"]
RUN dotnet restore "LactoseSimulation/LactoseSimulation.csproj"
COPY . .

WORKDIR "/src/LactoseSimulation"
RUN dotnet build "LactoseSimulation.csproj" -c $BUILD_CONFIGURATION -o /app/build 

FROM build AS publish
RUN dotnet publish "LactoseSimulation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:Deterministic=true /p:ContinuousIntegrationBuild=true

RUN ls -l /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LactoseSimulation.dll"]
