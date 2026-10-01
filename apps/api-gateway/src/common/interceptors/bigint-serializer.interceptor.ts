import { CallHandler, ExecutionContext, Injectable, NestInterceptor } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { map, Observable } from 'rxjs';

/**
 * Serialize non-JSON-native values yang Prisma return:
 * - BigInt → string (avoid JSON.stringify error)
 * - Prisma.Decimal → number (untuk basePrice, taxAmount, dll)
 * - Date / null / undefined → unchanged
 *
 * Recurse ke array & object children.
 */
@Injectable()
export class BigIntSerializerInterceptor implements NestInterceptor {
  intercept(_context: ExecutionContext, next: CallHandler): Observable<unknown> {
    return next.handle().pipe(map((data) => serializePrismaTypes(data)));
  }
}

function serializePrismaTypes(value: unknown): unknown {
  if (typeof value === 'bigint') {
    return value.toString();
  }

  // Prisma Decimal — convert to number untuk JSON-friendly output.
  // Use toString() then Number() agar precision aman untuk uang (max IDR 999 trillion masih < 2^53).
  if (value instanceof Prisma.Decimal) {
    return Number(value.toString());
  }

  if (Array.isArray(value)) {
    return value.map((item) => serializePrismaTypes(item));
  }

  if (value instanceof Date || value === null || value === undefined) {
    return value;
  }

  if (typeof value === 'object') {
    // Detect Decimal-like serialized object (in case kena 2x serialize) — has s/e/d shape
    const obj = value as Record<string, unknown>;
    if (
      typeof obj.s === 'number' &&
      typeof obj.e === 'number' &&
      Array.isArray(obj.d) &&
      Object.keys(obj).length === 3
    ) {
      // Reconstruct Decimal then convert
      try {
        return Number(new Prisma.Decimal(obj as unknown as Prisma.Decimal.Value).toString());
      } catch {
        return value;
      }
    }

    const entries = Object.entries(obj);
    return Object.fromEntries(entries.map(([key, item]) => [key, serializePrismaTypes(item)]));
  }

  return value;
}
