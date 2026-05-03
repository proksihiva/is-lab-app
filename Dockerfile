FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY IsLabApp.csproj ./
RUN dotnet restore ./IsLabApp.csproj

COPY . ./
RUN dotnet build ./IsLabApp.csproj -c Release --no-restore

RUN dotnet publish ./IsLabApp.csproj -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "IsLabApp.dll"]
