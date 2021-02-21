FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Asuka/Asuka.csproj", "src/Asuka/"]
RUN dotnet restore "src/Asuka/Asuka.csproj"
COPY . .
WORKDIR "/src/src/Asuka"
RUN dotnet build "Asuka.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Asuka.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Asuka.dll"]
