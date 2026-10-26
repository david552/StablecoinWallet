FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["StablecoinWallet/StablecoinWallet.Api.csproj", "StablecoinWallet/"]
COPY ["StablecoinWallet.Infrastructure/StablecoinWallet.Infrastructure.csproj", "StablecoinWallet.Infrastructure/"]
COPY ["StablecoinWallet.Domain/StablecoinWallet.Domain.csproj", "StablecoinWallet.Domain/"]
RUN dotnet restore "StablecoinWallet/StablecoinWallet.Api.csproj"

COPY . .
RUN dotnet publish "StablecoinWallet/StablecoinWallet.Api.csproj" \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "StablecoinWallet.Api.dll"]
