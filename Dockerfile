FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o App

FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=build src/App/ .
ENTRYPOINT ["dotnet", "Asuka.dll"]
