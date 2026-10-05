# =====================================================================
# wa-gateway — self-hosted WhatsApp gateway (Baileys, Fonnte-compatible)
# Context build: apps/wa-gateway/  (self-contained, deps di package.json)
# =====================================================================

# ---- build ----
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json ./
RUN npm install --os=linux --cpu=x64 --libc=musl
COPY tsconfig.json ./
COPY src ./src
RUN npm run build \
 && npm prune --omit=dev \
 && rm -rf \
      node_modules/.bin \
      node_modules/@emnapi \
      node_modules/@img/sharp-linux-x64 \
      node_modules/@img/sharp-libvips-linux-x64 \
      node_modules/@img/sharp-wasm32 \
      node_modules/@types \
      node_modules/*/.github \
      node_modules/*/.husky \
      node_modules/*/benchmarks \
      node_modules/*/benchmark \
      node_modules/*/coverage \
      node_modules/*/docs \
      node_modules/*/doc \
      node_modules/*/examples \
      node_modules/*/example \
      node_modules/*/test \
      node_modules/*/tests \
      node_modules/@*/*/.github \
      node_modules/@*/*/.husky \
      node_modules/@*/*/benchmarks \
      node_modules/@*/*/benchmark \
      node_modules/@*/*/coverage \
      node_modules/@*/*/docs \
      node_modules/@*/*/doc \
      node_modules/@*/*/examples \
      node_modules/@*/*/example \
      node_modules/@*/*/test \
      node_modules/@*/*/tests \
 && npm cache clean --force

# ---- runner ----
FROM node:22-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production

RUN addgroup --system --gid 1001 nodejs \
 && adduser --system --uid 1001 nodejs \
 && mkdir -p /app/data \
 && chown -R nodejs:nodejs /app

COPY --from=build --chown=nodejs:nodejs /app/package.json ./
COPY --from=build --chown=nodejs:nodejs /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist

# Data persisten (registry.json + sessions/<token> creds Baileys).
# WAJIB di-mount sebagai volume saat run, kalau tidak hilang saat container recreate.
ENV WA_GATEWAY_DATA_DIR=/app/data
VOLUME ["/app/data"]

USER nodejs
EXPOSE 3204
ENV WA_GATEWAY_PORT=3204

CMD ["node", "dist/index.js"]
