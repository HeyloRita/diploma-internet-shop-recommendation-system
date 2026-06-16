FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/InternetShop.API/InternetShop.API.csproj", "src/InternetShop.API/"]
RUN dotnet restore "src/InternetShop.API/InternetShop.API.csproj"
COPY . .
WORKDIR "/src/src/InternetShop.API"
RUN dotnet build "InternetShop.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InternetShop.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InternetShop.API.dll"]