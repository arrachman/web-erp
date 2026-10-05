import { CanActivate, ExecutionContext, HttpException, HttpStatus, Injectable } from '@nestjs/common';

/**
 * W8 hardening — rate limit sederhana (in-memory, per instance gateway)
 * untuk endpoint PUBLIK portal: cegah brute-force login & spam register/
 * leads. Kategori: auth/tulis publik 15/menit/IP, baca publik 240/menit/IP.
 */
@Injectable()
export class PortalRateLimitGuard implements CanActivate {
  private readonly hits = new Map<string, number[]>();

  canActivate(context: ExecutionContext): boolean {
    const req = context.switchToHttp().getRequest();
    const ip = String(req.ip ?? req.headers?.['x-forwarded-for'] ?? 'unknown');
    const path = String(req.path ?? req.url ?? '');
    const isWrite = req.method !== 'GET';
    const limit = isWrite || path.includes('/auth/') ? 15 : 240;
    const key = `${ip}:${isWrite ? 'w' : 'r'}`;
    const now = Date.now();
    const windowStart = now - 60_000;
    const arr = (this.hits.get(key) ?? []).filter((t) => t > windowStart);
    if (arr.length >= limit) {
      throw new HttpException('Terlalu banyak permintaan. Coba lagi sebentar lagi.', HttpStatus.TOO_MANY_REQUESTS);
    }
    arr.push(now);
    this.hits.set(key, arr);
    if (this.hits.size > 5000) {
      for (const [k, v] of this.hits) if (v.every((t) => t <= windowStart)) this.hits.delete(k);
    }
    return true;
  }
}
