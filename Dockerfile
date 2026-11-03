# Restore and publish with the .NET SDK, then run the published application on the Wodby
# service image, which has the ASP.NET Core runtime but no SDK.
ARG WODBY_BASE_IMAGE

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG COPY_FROM
WORKDIR /src

# Restore first, so dependency layers stay cached until a project or lock file changes.
COPY ${COPY_FROM}/Directory.Build.props ${COPY_FROM}/App.slnx ./
COPY ${COPY_FROM}/src/App/App.csproj ${COPY_FROM}/src/App/packages.lock.json src/App/
RUN dotnet restore src/App/App.csproj --locked-mode

COPY ${COPY_FROM} .
RUN dotnet publish src/App/App.csproj -c Release -o /out --no-restore

FROM ${WODBY_BASE_IMAGE}
COPY --from=build --chown=wodby:wodby /out /usr/src/app

CMD ["dotnet-app"]
