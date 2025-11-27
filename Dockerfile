FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["KopiAku.csproj", "./"]
RUN dotnet restore "KopiAku.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "KopiAku.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KopiAku.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "KopiAku.dll"]