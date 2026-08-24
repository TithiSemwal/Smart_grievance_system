FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SmartGrievanceSystem.sln .
COPY SmartGrievanceSystem.Core/*.csproj SmartGrievanceSystem.Core/
COPY SmartGrievanceSystem.Infrastructure/*.csproj SmartGrievanceSystem.Infrastructure/
COPY SmartGrievanceSystem.Web/*.csproj SmartGrievanceSystem.Web/

RUN dotnet restore SmartGrievanceSystem.sln

COPY SmartGrievanceSystem.Core/ SmartGrievanceSystem.Core/
COPY SmartGrievanceSystem.Infrastructure/ SmartGrievanceSystem.Infrastructure/
COPY SmartGrievanceSystem.Web/ SmartGrievanceSystem.Web/

WORKDIR /src/SmartGrievanceSystem.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartGrievanceSystem.Web.dll"]
