FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY ["Visualizer/Visualizer.csproj", "Visualizer/"]
COPY ["Visualizer.Model/Visualizer.Model.csproj", "Visualizer.Model/"]
COPY ["Visualizer.Services/Visualizer.Services.csproj", "Visualizer.Services/"]
RUN dotnet restore "Visualizer/Visualizer.csproj"
COPY . .
WORKDIR "/src/Visualizer"
RUN dotnet publish "Visualizer.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5253
EXPOSE 7083
ENTRYPOINT ["dotnet", "Visualizer.dll"]
