#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 80
EXPOSE 443

RUN apt -y update
RUN apt install -y ffmpeg
RUN apt install -y libsodium-dev
RUN apt install -y libopus-dev

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Erik/Erik.csproj", "Erik/"]
RUN dotnet restore "Erik/Erik.csproj"
COPY . .
WORKDIR "/src/Erik"
RUN dotnet build "Erik.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Erik.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Erik.dll"]