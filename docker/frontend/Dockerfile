# Build context: the repository root (see docker/docker-compose-*.yml).
FROM node:22-bookworm-slim AS base
WORKDIR /app
COPY life-frontend/ ./
RUN npm ci

FROM base AS web-dev
EXPOSE 5173
CMD ["npm", "run", "dev", "-w", "@life/web", "--", "--host", "0.0.0.0"]

FROM base AS mobile-dev
EXPOSE 8081
CMD ["npm", "run", "start", "-w", "@life/mobile", "--", "--lan"]

FROM base AS test
# The container shares the CPU with the other services: the slowest UI tests exceed the 20 s default.
CMD ["npm", "test", "--", "--testTimeout=90000"]

FROM base AS desktop-check
# The native window needs a desktop session: in a container only the TypeScript code is checked.
CMD ["sh", "-c", "npm run typecheck -w @life/desktop && npm run lint -w @life/desktop"]

FROM base AS web-build
ARG EXPO_PUBLIC_APP_ENV=production
ARG EXPO_PUBLIC_API_URL
ENV EXPO_PUBLIC_APP_ENV=$EXPO_PUBLIC_APP_ENV
ENV EXPO_PUBLIC_API_URL=$EXPO_PUBLIC_API_URL
RUN npm run build -w @life/web

FROM nginx:1.27-alpine AS web
COPY docker/web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=web-build /app/apps/web/dist /usr/share/nginx/html
EXPOSE 80
