# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore separately so dependency downloads remain cached when only source changes.
COPY ["SlideMaker.Web/SlideMaker.Web.csproj", "SlideMaker.Web/"]
RUN dotnet restore "SlideMaker.Web/SlideMaker.Web.csproj"

COPY . .
RUN dotnet publish "SlideMaker.Web/SlideMaker.Web.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final
WORKDIR /app

# Tesseract's native runtime needs Leptonica; libgdiplus supports the image
# orientation normalization used before OCR.
RUN apt-get update \
    && apt-get install --no-install-recommends --yes libleptonica-dev libgdiplus \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Uploaded files and the optional SQLite database are runtime data. Mount a
# persistent volume at /app/data and set the paths there in production.
RUN mkdir -p /app/wwwroot/uploads /app/App_Data \
    && chown -R app:app /app/wwwroot/uploads /app/App_Data

USER app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "SlideMaker.Web.dll"]
