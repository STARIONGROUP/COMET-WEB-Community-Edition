# syntax=docker/dockerfile:1.4

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

COPY COMET.Web.Common COMET.Web.Common
COPY COMETwebapp COMETwebapp

RUN --mount=type=secret,id=DEVEXPRESS_NUGET_KEY export DEVEXPRESS_NUGET_KEY=$(cat /run/secrets/DEVEXPRESS_NUGET_KEY) \ 
 && dotnet nuget add source https://nuget.devexpress.com/api -n DXFeed -u DevExpress -p ${DEVEXPRESS_NUGET_KEY} --store-password-in-clear-text \
 && dotnet restore COMETwebapp

RUN dotnet build COMETwebapp -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish COMETwebapp -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY COMETwebapp/nginx.conf /etc/nginx/nginx.conf