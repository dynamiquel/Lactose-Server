FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 443
EXPOSE 443/udp

ENV ASPNETCORE_HTTPS_PORTS=443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LactoseIdentity/LactoseIdentity.csproj", "LactoseIdentity/"]
RUN dotnet restore "LactoseIdentity/LactoseIdentity.csproj"
COPY . .
WORKDIR "/src/LactoseIdentity"
RUN dotnet build "LactoseIdentity.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LactoseIdentity.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LactoseIdentity.dll"]
