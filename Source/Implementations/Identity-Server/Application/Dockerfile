#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
RUN apt update && apt install -y libldap-2.4-2
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Application/Application.csproj", "Application/"]
RUN dotnet restore "Application/Application.csproj"
COPY . .
WORKDIR "/src/Application"
RUN dotnet build "Application.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Application.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HansKindberg.IdentityServer.Application.dll"]