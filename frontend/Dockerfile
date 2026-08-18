# ---- Dependencies ----
FROM node:22-alpine AS deps
WORKDIR /app

RUN corepack enable && corepack prepare pnpm@11 --activate

COPY frontend/package.json frontend/pnpm-lock.yaml frontend/pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile

# ---- Build ----
FROM node:22-alpine AS build
WORKDIR /app

RUN corepack enable && corepack prepare pnpm@11 --activate

COPY --from=deps /app/node_modules ./node_modules
COPY frontend/ .
RUN pnpm run build

# ---- Runtime ----
FROM oven/bun:1 AS runtime
WORKDIR /app

ENV NODE_ENV=production \
    HOSTNAME=0.0.0.0 \
    PORT=3000

# BACKEND_URL is read at runtime by src/proxy.ts, not baked in at build time -
# set it at `docker run`/compose time, e.g. BACKEND_URL=http://api:8080.
# Required: if unset, every /api/* request throws ("Invalid URL") since
# proxy.ts's `new URL(path, undefined)` has no base to resolve against.

# `next build` with output: "standalone" only traces the files actually
# needed at runtime; public/ and .next/static are excluded and must be
# copied in separately.
COPY --from=build /app/public ./public
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static

# The base image ships a non-root "bun" user for exactly this purpose.
USER bun
EXPOSE 3000

ENTRYPOINT ["bun", "server.js"]
