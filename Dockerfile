# Build stage: сборка и публикация приложения
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app

# Runtime stage: используем лёгкий образ aspnet вместо sdk.
# Образ sdk (~800MB) содержит компилятор и инструменты, не нужные в production.
# Образ aspnet (~200MB) содержит только .NET runtime — этого достаточно для запуска.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# Порт 10000 — стандартный порт для Web Service на платформе Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "CampaignApp.dll"]
