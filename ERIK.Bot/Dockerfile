#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["ERIK.Bot/ERIK.Bot.csproj", "ERIK.Bot/"]
RUN dotnet restore "ERIK.Bot/ERIK.Bot.csproj"
COPY . .
WORKDIR "/src/ERIK.Bot"
RUN dotnet build "ERIK.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ERIK.Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y libsodium23 libsodium-dev libopus0 libopus-dev

ENTRYPOINT ["dotnet", "ERIK.Bot.dll"]