FROM mcr.microsoft.com/dotnet/core/runtime:3.1 as prerun
RUN apt-get update && apt-get install --no-install-recommends -y ffmpeg libopus0 libsodium23 libopus-dev libsodium-dev

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app
COPY DiscordBot/*.csproj ./DiscordBot/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY Domain/*.csproj ./Domain/
WORKDIR /app/DiscordBot
RUN dotnet restore

WORKDIR /app
COPY DiscordBot/. ./DiscordBot/
COPY Infrastructure/. ./Infrastructure/
COPY Domain/. ./Domain/
WORKDIR /app/DiscordBot
RUN dotnet publish -c Release -o out

FROM prerun as run
WORKDIR /app
COPY --from=build-env /app/DiscordBot/out .

ENTRYPOINT ["dotnet", "DiscordBot.dll"]
