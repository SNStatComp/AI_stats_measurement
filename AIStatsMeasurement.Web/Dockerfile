FROM node:20 AS build
WORKDIR /app

COPY AIStatsMeasurement.Web/package*.json ./
RUN npm ci

COPY AIStatsMeasurement.Web/ .
RUN npm run build

FROM nginxinc/nginx-unprivileged:alpine

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080