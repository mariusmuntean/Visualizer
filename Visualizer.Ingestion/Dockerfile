FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Visualizer.Ingestion/Visualizer.Ingestion.csproj", "Visualizer.Ingestion/"]
RUN dotnet restore "Visualizer.Ingestion/Visualizer.Ingestion.csproj"
COPY . .
WORKDIR "/src/Visualizer.Ingestion"
RUN dotnet build "Visualizer.Ingestion.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Visualizer.Ingestion.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Visualizer.Ingestion.dll"]
