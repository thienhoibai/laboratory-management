FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore TestOrder.Presentation/TestOrder.Presentation.csproj
RUN dotnet publish TestOrder.Presentation/TestOrder.Presentation.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Copy images for report
COPY TestOrder.Presentation/Img ./Img

ENV ASPNETCORE_URLS=http://+:5003
EXPOSE 5003
ENTRYPOINT ["dotnet","TestOrder.Presentation.dll"]
