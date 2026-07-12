# Multi-stage build: compile with the full SDK, run with the smaller ASP.NET runtime image.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Task_2_TranscriptAnalysis.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render (and most PaaS hosts) assign the port to listen on via $PORT at
# container start — it's not known at build time, so it has to be read in
# the shell that launches dotnet, not baked into ENV/EXPOSE.
ENTRYPOINT ["/bin/sh", "-c", "dotnet Task_2_TranscriptAnalysis.dll --urls http://+:${PORT:-8080}"]
