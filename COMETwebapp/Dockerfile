# syntax=docker/dockerfile:1.4

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG DEVEXPRESS_NUGET_KEY
ARG PACKAGE_TOKEN

# Workaround for NU3012: the ReactiveUI/Splat NuGet signing certificate has been revoked.
# Disable enforced package signature verification until the packages are updated. See issue for the long-term fix.
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

COPY COMET.Web.Common COMET.Web.Common
COPY COMETwebapp COMETwebapp

RUN --mount=type=secret,id=DEVEXPRESS_NUGET_KEY --mount=type=secret,id=PACKAGE_TOKEN \
 DEVEXPRESS_NUGET_KEY="${DEVEXPRESS_NUGET_KEY:-$(cat /run/secrets/DEVEXPRESS_NUGET_KEY 2>/dev/null)}" \
 PACKAGE_TOKEN="${PACKAGE_TOKEN:-$(cat /run/secrets/PACKAGE_TOKEN 2>/dev/null)}" \
 && dotnet nuget add source https://nuget.devexpress.com/api -n DXFeed -u DevExpress -p ${DEVEXPRESS_NUGET_KEY} --store-password-in-clear-text \
 && dotnet nuget add source https://nuget.pkg.github.com/STARIONGROUP/index.json -n StarionGithubFeed -u STARIONGROUP -p ${PACKAGE_TOKEN} --store-password-in-clear-text \
 && dotnet restore COMETwebapp

RUN dotnet build COMETwebapp -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish COMETwebapp -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0.5-alpine3.23 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user and give this user access to the working directory
RUN chown -R "$APP_UID" /app
USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
 CMD wget --no-verbose --tries=1 --spider http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "COMETwebapp.dll"]
