FROM node:20-alpine AS build
WORKDIR /src
COPY . .
WORKDIR /src/frontend
RUN npm ci
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /src/frontend/dist /usr/share/nginx/html
EXPOSE 80
