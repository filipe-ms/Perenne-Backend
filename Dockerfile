FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

COPY Perenne.csproj ./
RUN dotnet restore Perenne.csproj

COPY . .

RUN dotnet publish Perenne.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build-env /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Perenne.dll"]
