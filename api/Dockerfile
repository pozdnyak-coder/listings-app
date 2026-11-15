# ---- сборка ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ListingsApi.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish ListingsApi.csproj -c Release -o /app/publish --no-restore

# ---- запуск ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# папка для загруженных фото должна существовать до старта сервера
RUN mkdir -p /app/wwwroot/uploads
ENV ASPNETCORE_ENVIRONMENT=Production
# порт задаёт Render через переменную PORT — Program.cs её читает
ENTRYPOINT ["dotnet", "ListingsApi.dll"]
