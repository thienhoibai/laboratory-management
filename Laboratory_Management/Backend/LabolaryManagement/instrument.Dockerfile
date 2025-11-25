FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore Instrument.Presentation/Instrument.Presentation.csproj
RUN dotnet publish Instrument.Presentation/Instrument.Presentation.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:5008
EXPOSE 5008
ENTRYPOINT ["dotnet","Instrument.Presentation.dll"]
