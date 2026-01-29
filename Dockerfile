# Stage 1: base
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: build
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Gve.Demo/Gve.Demo/Gve.Demo.csproj", "Gve.Demo/Gve.Demo/"]
COPY ["Gve.Demo/Gve.Demo.Client/Gve.Demo.Client.csproj", "Gve.Demo/Gve.Demo.Client/"]
# restore the dependencies
RUN dotnet restore "Gve.Demo/Gve.Demo/Gve.Demo.csproj" -s https://api.nuget.org/v3/index.json --verbosity n /p:Configuration=Release
# copy the content of the project
COPY . .
# build it
RUN dotnet build "Gve.Demo/Gve.Demo/Gve.Demo.csproj" -c Release -o /app/build

# Stage 3: publish
FROM build AS publish
RUN dotnet publish "Gve.Demo/Gve.Demo/Gve.Demo.csproj" -c Release -o /app/publish

# Stage 4: final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gve.Demo.dll"]