FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY . .
# RUN dotnet restore "Visualizer.Ingestion/Visualizer.Ingestion.csproj"
RUN dotnet publish "Visualizer.Ingestion/Visualizer.Ingestion.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
ARG HTTP_PORT=5121
ARG HTTPS_PORT=7193

WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE $HTTP_PORT
EXPOSE $HTTPS_PORT
ENTRYPOINT ["dotnet", "Visualizer.Ingestion.dll"]
