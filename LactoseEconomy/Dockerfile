FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 443
EXPOSE 443/udp

ENV ASPNETCORE_HTTPS_PORTS=443
USER app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["LactoseEconomy/LactoseEconomy.csproj", "LactoseEconomy/"]
RUN dotnet restore "LactoseEconomy/LactoseEconomy.csproj"
COPY . .

WORKDIR "/src/LactoseEconomy"
RUN dotnet build "LactoseEconomy.csproj" -c $BUILD_CONFIGURATION -o /app/build 

FROM build AS publish
RUN dotnet publish "LactoseEconomy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:Deterministic=true /p:ContinuousIntegrationBuild=true

RUN ls -l /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LactoseEconomy.dll"]
