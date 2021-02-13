#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app
expose 53/udp

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster AS build
WORKDIR /src
COPY ["EdwardHsu.Dns.Main/EdwardHsu.Dns.Main.csproj", "EdwardHsu.Dns.Main/"]
RUN dotnet restore "EdwardHsu.Dns.Main/EdwardHsu.Dns.Main.csproj"
COPY . .
WORKDIR "/src/EdwardHsu.Dns.Main"
RUN dotnet build "EdwardHsu.Dns.Main.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EdwardHsu.Dns.Main.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EdwardHsu.Dns.Main.dll"]