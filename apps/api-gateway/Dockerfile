# =====================================================================
# api-gateway — Multi-stage Docker build (production)
# Base Debian (bukan Alpine): Prisma binaryTargets = debian-openssl-3.0.x
# (lihat prisma/schema/datasource.prisma). Output: node dist/main.
# =====================================================================

# ---- deps ----
FROM node:22-bookworm-slim AS deps
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci

# ---- builder ----
FROM node:22-bookworm-slim AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
# Generate Prisma client (multi-file schema via prismaSchemaFolder) lalu nest build.
RUN npx prisma generate && npm run build

# ---- runner ----
FROM node:22-bookworm-slim AS runner
WORKDIR /app
ENV NODE_ENV=production
# openssl dibutuhkan Prisma query engine saat runtime.
RUN apt-get update \
 && apt-get install -y --no-install-recommends openssl \
 && rm -rf /var/lib/apt/lists/*

# node_modules dari builder sudah berisi Prisma client ter-generate + CLI (utk migrate deploy).
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/prisma ./prisma
COPY --from=builder /app/package.json ./package.json

EXPOSE 3203

# Jalankan migrasi (non-interaktif) sebelum start. migrate deploy aman utk prod
# (tidak generate migrasi baru, hanya apply yang sudah ada).
CMD ["sh", "-c", "npx prisma migrate deploy && node dist/main"]
