# Stage 1: base - pulled for the TARGET platform (not the build machine)
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: build - runs natively on the BUILD machine (ARM); $TARGETARCH
# is passed through so dotnet can cross-compile for the target platform.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0 AS build
ARG TARGETARCH
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

# Stage 3: publish - cross-compile output for the target architecture
FROM build AS publish
ARG TARGETARCH
RUN dotnet publish "Gve.Demo/Gve.Demo/Gve.Demo.csproj" -c Release -o /app/publish -a $TARGETARCH

# Stage 4: final - copies target-arch publish output into the target-arch runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gve.Demo.dll"]