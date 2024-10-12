FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 443
EXPOSE 443/udp

ENV ASPNETCORE_HTTPS_PORTS=443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LactoseEconomy/LactoseEconomy.csproj", "LactoseEconomy/"]
RUN dotnet restore "LactoseEconomy/LactoseEconomy.csproj"
COPY . .
WORKDIR "/src/LactoseEconomy"
RUN dotnet build "LactoseEconomy.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LactoseEconomy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LactoseEconomy.dll"]
