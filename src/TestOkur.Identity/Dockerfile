ARG COMMIT=""

FROM mcr.microsoft.com/dotnet/aspnet:5.0 as base
LABEL "Maintainer"="Nazmi Altun <nazmialtun@windowslive.com>"
LABEL Commit=${COMMIT}

FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /src
COPY . .
RUN dotnet build "./src/TestOkur.Identity/TestOkur.Identity.csproj" -c Release -o /app

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
WORKDIR /src
RUN dotnet publish "./src/TestOkur.Identity/TestOkur.Identity.csproj" --no-restore -c Release -o /app
RUN rm -rf /app/wwwroot
COPY --from=ui-build /opt/app/wwwroot/dist /app/wwwroot/dist

FROM base as final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app .
ARG COMMIT
ENV COMMIT_SHA=${COMMIT}
ENTRYPOINT ["dotnet", "TestOkur.Identity.dll"]