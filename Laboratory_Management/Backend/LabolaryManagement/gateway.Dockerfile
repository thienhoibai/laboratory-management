# ---- Stage 1: build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore APIGateway.Presentation/APIGateway.Presentation.csproj
RUN dotnet publish APIGateway.Presentation/APIGateway.Presentation.csproj -c Release -o /out

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","APIGateway.Presentation.dll"]
