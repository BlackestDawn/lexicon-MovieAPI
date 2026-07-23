# ---- Dependencies ----
FROM oven/bun:1 AS deps
WORKDIR /app

COPY frontend/package.json frontend/bun.lock ./
RUN bun install --frozen-lockfile

# ---- Build ----
FROM oven/bun:1 AS build
WORKDIR /app

# `next build` calls next.config.ts's rewrites() once to bake the resolved
# destinations into the routes manifest, so BACKEND_URL has to be known at
# build time - it is not re-read by the standalone server at request time.
ARG BACKEND_URL
ENV BACKEND_URL=${BACKEND_URL}

COPY --from=deps /app/node_modules ./node_modules
COPY frontend/ .
RUN bun run build

# ---- Runtime ----
FROM oven/bun:1 AS runtime
WORKDIR /app

ENV NODE_ENV=production \
    HOSTNAME=0.0.0.0 \
    PORT=3000

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
