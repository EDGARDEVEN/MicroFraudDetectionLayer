FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY MFDL.Tools/*.csproj MFDL.Tools/
COPY MFDL.Core/*.csproj MFDL.Core/
COPY MFDL.Api/*.csproj MFDL.Api/

RUN dotnet restore

COPY MFDL.Tools/ MFDL.Tools/
COPY MFDL.Core/ MFDL.Core/
COPY MFDL.Api/ MFDL.Api/

RUN dotnet publish MFDL.Tools/MFDL.Tools.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MFDL.Tools.dll"]