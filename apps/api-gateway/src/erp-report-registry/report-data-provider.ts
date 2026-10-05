/**
 * Dataset-builder contract for registry reports (design D4/D5): a
 * provider owns the DATA for a family of report keys; the engine owns
 * layout. Providers register themselves into ReportProviderRegistry at
 * module init (erp-md-reports for md.*, domain modules in later waves).
 */

import { Injectable } from '@nestjs/common';
import type { ReportData, StiDatasetSpec } from '../erp-report-engine/engine-types-v2';

export interface ReportRenderRequest {
  code: string;
  reportKey: string;
  title: string;
  params: Record<string, unknown>;
  userName?: string;
  /** Datasets declared by the converted template (names + columns). */
  datasets: StiDatasetSpec[];
}

export interface ReportDataProvider {
  /** Prefix this provider handles (e.g. 'md.'), used for diagnostics. */
  readonly prefix: string;
  canHandle(reportKey: string): boolean;
  build(req: ReportRenderRequest): Promise<ReportData>;
}

@Injectable()
export class ReportProviderRegistry {
  private readonly providers: ReportDataProvider[] = [];

  register(provider: ReportDataProvider): void {
    if (!this.providers.includes(provider)) this.providers.push(provider);
  }

  find(reportKey: string): ReportDataProvider | null {
    return this.providers.find((p) => p.canHandle(reportKey)) ?? null;
  }

  prefixes(): string[] {
    return this.providers.map((p) => p.prefix);
  }
}

/** Normalize raw SQL rows (BigInt/Decimal/Date) into engine-friendly values. */
export function normalizeRow(row: Record<string, unknown>): Record<string, unknown> {
  const out: Record<string, unknown> = {};
  for (const [k, v] of Object.entries(row)) out[k] = normalizeValue(v);
  return out;
}

export function normalizeValue(v: unknown): unknown {
  if (typeof v === 'bigint') return Number(v);
  if (v instanceof Date) return v;
  if (v !== null && typeof v === 'object') {
    const anyV = v as { toNumber?: unknown; constructor?: { name?: string } };
    if (typeof anyV.toNumber === 'function' && anyV.constructor?.name === 'Decimal') {
      return (anyV.toNumber as () => number).call(v);
    }
    return v;
  }
  return v;
}
