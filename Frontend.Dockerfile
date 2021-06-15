ARG BUILDIMAGE

# Stage 1
FROM $BUILDIMAGE AS build
WORKDIR /app
RUN dotnet publish \
    Frontend/Frontend.csproj \
    -c Release \
    -o Frontend/out \
    --no-restore \
    -r alpine-x64 \
    --self-contained true \
    -p:PublishTrimmed=true \
    -p:PublishReadyToRunShowWarnings=true \
    -p:PublishSingleFile=true

# Stage 2
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS final
WORKDIR /app
COPY --from=build /app/Frontend/out/ ./
ENTRYPOINT ["./Frontend"]
