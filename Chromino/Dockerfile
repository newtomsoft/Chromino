#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Chromino/ChrominoWeb.csproj", "Chromino/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["SignalR/SignalR.csproj", "SignalR/"]
COPY ["Core/ChrominoBI.csproj", "Core/"]
COPY ["Tool/Tool.csproj", "Tool/"]
RUN dotnet restore "Chromino/ChrominoWeb.csproj"
COPY . .
WORKDIR "/src/Chromino"
RUN dotnet build "ChrominoWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChrominoWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChrominoWeb.dll"]