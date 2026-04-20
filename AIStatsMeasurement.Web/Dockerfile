FROM node:20 AS build
WORKDIR /app
COPY AIStatsMeasurement.Web/package*.json ./
RUN npm install
COPY AIStatsMeasurement.Web/ .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80