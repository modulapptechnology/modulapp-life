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

FROM base AS desktop-gui
# Tauri on Linux: WebKitGTK and the libraries of the project, plus a virtual screen served through noVNC.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        build-essential curl ca-certificates pkg-config file patchelf \
        libwebkit2gtk-4.1-dev libssl-dev libxdo-dev libayatana-appindicator3-dev librsvg2-dev \
        libdbus-1-dev libsecret-1-dev dbus \
        xvfb x11vnc novnc websockify fluxbox \
    && rm -rf /var/lib/apt/lists/*
RUN curl --proto "=https" --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y --profile minimal
ENV PATH="/root/.cargo/bin:$PATH"
# No GPU and no user namespaces in the container: WebKit renders in software and runs without its sandbox.
ENV WEBKIT_DISABLE_COMPOSITING_MODE=1 \
    WEBKIT_DISABLE_DMABUF_RENDERER=1 \
    WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1
# The app chooses Spanish or English from the system locale, and the locale must exist for WebKit to use it.
RUN apt-get update \
    && apt-get install -y --no-install-recommends locales \
    && sed -i "s/^# *es_ES.UTF-8/es_ES.UTF-8/" /etc/locale.gen \
    && locale-gen \
    && rm -rf /var/lib/apt/lists/*
COPY docker/frontend/desktop-gui.sh /usr/local/bin/desktop-gui.sh
RUN chmod +x /usr/local/bin/desktop-gui.sh
EXPOSE 6080
CMD ["desktop-gui.sh"]

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
