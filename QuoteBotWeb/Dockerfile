#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update && apt-get install --no-install-recommends -y ffmpeg opus-tools

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["QuoteBotWeb.csproj", "QuoteBotWeb/"]
COPY ["../Domain.csproj", "Domain/"]
COPY ["../Discord.csproj", "Discord/"]
COPY ["../Aws.csproj", "Aws/"]
RUN dotnet restore "QuoteBotWeb.csproj"
COPY . .
WORKDIR "/src/QuoteBotWeb"
RUN dotnet build "QuoteBotWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuoteBotWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuoteBotWeb.dll"]