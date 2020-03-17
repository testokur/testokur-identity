FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as base
EXPOSE 80 
LABEL "Maintainer"="Nazmi Altun <nazmialtun@windowslive.com>"

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /src
COPY ./*.sln ./
COPY src/*/*.csproj ./  
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
COPY tests/*/*.csproj ./  
RUN for file in $(ls *.csproj); do mkdir -p tests/${file%.*}/ && mv $file tests/${file%.*}/; done
RUN dotnet restore
COPY . .
RUN dotnet build "./src/TestOkur.Identity/TestOkur.Identity.csproj" -c Release --no-restore -o /app

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
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TestOkur.Identity.dll"]
