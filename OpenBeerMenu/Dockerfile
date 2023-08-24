FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
ARG TARGETPLATFORM
ARG RID
WORKDIR /app

ENV PATH="$PATH:/root/.dotnet/tools"

# Install tailwind and the ef tools we need
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then SUFFIX="x64"; else SUFFIX="arm64"; fi; curl -sLO "https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-linux-$SUFFIX"; chmod +x tailwindcss-linux*; mv tailwindcss-linux* /usr/bin/tailwindcss

RUN dotnet tool install --global dotnet-ef

# Copy everything
COPY . ./

# Restore as distinct layers
RUN dotnet restore

# Build and publish a release
RUN dotnet publish -c Release -o out
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then RID="linux-x64"; else RID="linux-arm64"; fi; dotnet ef migrations bundle -r $RID -o out/efbundle

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
RUN chmod +x /app/start
ENTRYPOINT ["bash", "/app/start"]