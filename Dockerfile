# Используем один и тот же образ .NET 10 SDK для всего процесса
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файлы проекта и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем всё остальное и компилируем релизную версию
COPY . ./
RUN dotnet publish -c Release -o /app

# Используем тот же гарантированный .NET 10 SDK для финального запуска сайта
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# Передаем настройки портов для Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "CampaignApp.dll"]