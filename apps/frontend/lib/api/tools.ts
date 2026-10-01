// API client for ERP admin tools — recalc-cogs, repost-journal, data-validity
import { apiGet, apiPost } from './client';

// ─── Recalc COGS ──────────────────────────────────────────────────────────────

export interface RecalcCogsParams {
  fiscalPeriodId?: string;
  fromDate?: string;
  toDate?: string;
}

export interface RecalcCogsResult {
  processed: number;
  updated: number;
  errors: number;
  message: string;
}

// ─── Repost Journal ───────────────────────────────────────────────────────────

export interface RepostJournalParams {
  fiscalPeriodId?: string;
  fromDate?: string;
  toDate?: string;
  module?: string;
}

export interface RepostJournalResult {
  processed: number;
  posted: number;
  errors: number;
  message: string;
}

// ─── Data Validity ────────────────────────────────────────────────────────────

export interface ValidityCheck {
  name: string;
  status: 'OK' | 'WARN' | 'ERROR';
  count: number;
}

export interface DataValidityResult {
  checks: ValidityCheck[];
  summary: { ok: number; warn: number; error: number };
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function runRecalcCogs(
  params: RecalcCogsParams,
): Promise<{ success: boolean; data: RecalcCogsResult }> {
  return apiPost('/tools/recalc-cogs', params);
}

export async function runRepostJournal(
  params: RepostJournalParams,
): Promise<{ success: boolean; data: RepostJournalResult }> {
  return apiPost('/tools/repost-journal', params);
}

export async function runDataValidity(): Promise<{
  success: boolean;
  data: DataValidityResult;
}> {
  return apiGet('/tools/data-validity');
}
