import { fileURLToPath } from 'node:url';
import { dirname } from 'node:path';

const __dirname = dirname(fileURLToPath(import.meta.url));

// Internal api-gateway URL (server-side only — not exposed to browser).
// In production set ERP_INTERNAL_API_URL to wherever api-gateway is reachable
// from this Next.js process (e.g. http://localhost:3203 on the same host).
const ERP_INTERNAL_API_URL =
  process.env.ERP_INTERNAL_API_URL ?? 'http://localhost:3203';

/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  // Shared workspace package consumed as TS source (not a built dist).
  transpilePackages: ['@sentient-factory/ui-kit'],
  // Scope Turbopack to the app root so it resolves local node_modules.
  turbopack: { root: __dirname },
  devIndicators: false,

  // Proxy /api/erp/* → api-gateway so same-origin calls from the browser
  // (NEXT_PUBLIC_ERP_API_URL = https://erp.fr-labs.my.id/api/erp) are
  // forwarded to the NestJS backend instead of hitting Next.js 404.
  async rewrites() {
    return [
      {
        source: '/api/erp/:path*',
        destination: `${ERP_INTERNAL_API_URL}/api/erp/:path*`,
      },
    ];
  },
};

const PORT = process.env.WEB_ERP_PORT || '3219';

const defaultDevOrigins = [
  '192.168.1.150',
  `192.168.1.150:${PORT}`,
  'localhost',
  `localhost:${PORT}`,
  '127.0.0.1',
  `127.0.0.1:${PORT}`,
  'erp.fr-labs.my.id',
];

const envDevOrigins = (process.env.NEXT_ALLOWED_DEV_ORIGINS || '')
  .split(',')
  .map((origin) => origin.trim())
  .filter(Boolean);

const allowedDevOrigins = Array.from(
  new Set([...defaultDevOrigins, ...envDevOrigins])
);

if (allowedDevOrigins.length > 0) {
  nextConfig.allowedDevOrigins = allowedDevOrigins;
}

const basePath = process.env.NEXT_PUBLIC_BASE_PATH?.trim();
if (basePath && basePath !== '/') {
  nextConfig.basePath = basePath;
  nextConfig.assetPrefix = basePath;
}

export default nextConfig;
