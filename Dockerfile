FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish src/L4D2PlayStats.Hub.Web --configuration Release --output /app

FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080 \
    Hub__DataDirectory=/data
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "L4D2PlayStats.Hub.Web.dll"]
