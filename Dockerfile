FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

WORKDIR /src/StellarBlueAssignment

COPY StellarBlueAssignment.csproj .

RUN dotnet restore

COPY . .

RUN dotnet publish StellarBlueAssignment.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "StellarBlueAssignment.dll"]