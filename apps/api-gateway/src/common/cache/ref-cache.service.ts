import { Injectable, Logger, OnModuleDestroy } from '@nestjs/common';
import Redis from 'ioredis';

/**
 * Lightweight Redis cache for small reference lists (currencies, taxes, …).
 * Graceful no-op when REDIS_URL is missing or Redis is down — callers always
 * fall through to the DB loader.
 */
function safeJsonStringify(value: unknown): string {
  return JSON.stringify(value, (_key, item) => {
    if (typeof item === 'bigint') return item.toString();
    if (item && typeof item === 'object' && typeof item.toString === 'function') {
      const ctor = (item as { constructor?: { name?: string } }).constructor?.name;
      if (ctor === 'Decimal') return item.toString();
    }
    return item;
  });
}

@Injectable()
export class RefCacheService implements OnModuleDestroy {
  private readonly log = new Logger(RefCacheService.name);
  private client: Redis | null = null;
  private readonly prefix = 'erp:ref:';

  constructor() {
    const url = process.env.REDIS_URL?.trim();
    if (!url) {
      this.log.warn('REDIS_URL not set — RefCacheService disabled');
      return;
    }
    try {
      this.client = new Redis(url, {
        maxRetriesPerRequest: 1,
        enableOfflineQueue: false,
        retryStrategy: (times) => (times > 3 ? null : 200),
      });
      this.client.on('error', (err) => {
        this.log.warn(`Redis error: ${err.message}`);
      });
    } catch (err) {
      this.log.warn(`Redis init failed: ${(err as Error).message}`);
      this.client = null;
    }
  }

  async onModuleDestroy() {
    if (this.client) {
      try {
        await this.client.quit();
      } catch {
        // ignore
      }
      this.client = null;
    }
  }

  private key(ns: string, part = 'all'): string {
    return `${this.prefix}${ns}:${part}`;
  }

  private ready(): boolean {
    return !!this.client && this.client.status === 'ready';
  }

  async getJson<T>(ns: string, part = 'all'): Promise<T | null> {
    if (!this.ready()) return null;
    try {
      const raw = await this.client!.get(this.key(ns, part));
      if (!raw) return null;
      return JSON.parse(raw) as T;
    } catch {
      return null;
    }
  }

  async setJson(ns: string, value: unknown, ttlSec = 300, part = 'all'): Promise<void> {
    if (!this.ready()) return;
    try {
      await this.client!.set(this.key(ns, part), safeJsonStringify(value), 'EX', ttlSec);
    } catch {
      // ignore
    }
  }

  async del(ns: string, part = 'all'): Promise<void> {
    if (!this.ready()) return;
    try {
      await this.client!.del(this.key(ns, part));
    } catch {
      // ignore
    }
  }

  /** Invalidate every key under a namespace (SCAN + DEL). */
  async invalidateNs(ns: string): Promise<void> {
    if (!this.ready()) return;
    const match = `${this.prefix}${ns}:*`;
    try {
      let cursor = '0';
      do {
        const [next, keys] = await this.client!.scan(cursor, 'MATCH', match, 'COUNT', 50);
        cursor = next;
        if (keys.length) await this.client!.del(...keys);
      } while (cursor !== '0');
    } catch {
      // ignore
    }
  }

  /**
   * Cache-aside helper: return cached JSON or load, store, return.
   */
  async getOrLoad<T>(
    ns: string,
    loader: () => Promise<T>,
    ttlSec = 300,
    part = 'all',
  ): Promise<T> {
    const hit = await this.getJson<T>(ns, part);
    if (hit !== null) return hit;
    const value = await loader();
    await this.setJson(ns, value, ttlSec, part);
    return value;
  }
}
