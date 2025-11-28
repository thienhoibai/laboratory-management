FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore BlogService.Presentation/BlogService.Presentation.csproj
RUN dotnet publish BlogService.Presentation/BlogService.Presentation.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
RUN mkdir -p /app/Images
ENV ASPNETCORE_URLS=http://+:5004
EXPOSE 5004
ENTRYPOINT ["dotnet","BlogService.Presentation.dll"]
