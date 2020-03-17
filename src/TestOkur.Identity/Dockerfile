ARG COMMIT=""

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as base
LABEL "Maintainer"="Nazmi Altun <nazmialtun@windowslive.com>"
LABEL Commit=${COMMIT}

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /src
COPY . .
RUN dotnet build "./src/TestOkur.Identity/TestOkur.Identity.csproj" -c Release -o /app
RUN dotnet tool install --global dotnet-trace
RUN dotnet tool install --global dotnet-dump
RUN dotnet tool install --global dotnet-counters
RUN dotnet tool install --global dotnet-sos
RUN dotnet tool install --global dotnet-gcdump
ENV PATH="/root/.dotnet/tools:${PATH}"

FROM build as integrationtest
WORKDIR /src/tests/Integration.Tests

FROM node:current-alpine AS ui-build
ENV NODE_ENV production
ENV NODE_PATH ./dist
WORKDIR /opt/app
COPY src/TestOkur.Identity/package.json  ./
COPY src/TestOkur.Identity/yarn.lock  ./
COPY src/TestOkur.Identity/webpack.common.js  ./
COPY src/TestOkur.Identity/webpack.prod.js  ./
COPY src/TestOkur.Identity/wwwroot  ./wwwroot
RUN yarn install --network-timeout 1000000 
RUN yarn build-prod

FROM build AS publish
RUN dotnet publish "./src/TestOkur.Identity/TestOkur.Identity.csproj" --no-restore -c Release -o /app
RUN rm -rf /app/wwwroot
COPY --from=ui-build /opt/app/wwwroot/dist /app/wwwroot/dist

FROM base as final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app .
COPY --from=build /root/.dotnet/tools/ /opt/bin
ENV PATH="/opt/bin:${PATH}"
ARG COMMIT
ENV COMMIT_SHA=${COMMIT}
ENTRYPOINT ["dotnet", "TestOkur.Identity.dll"]