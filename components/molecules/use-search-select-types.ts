import * as React from 'react';
import { SearchSelectColumn, SearchSelectOption } from './search-select-types';

/** Display label builder — "{code} - {label}" when code is present. */
export const optLabel = (opt: SearchSelectOption): string =>
  opt.code ? `${opt.code} - ${opt.label}` : opt.label;

/**
 * Prioritaskan exact code match: kalau dalam results ada tepat 1 row dgn
 * code == query (case-insensitive), auto-pilih row itu — meskipun results
 * total > 1 (mis. nama lain ikut match LIKE). §2.30
 */
export function pickExactCodeMatch(
  results: SearchSelectOption[],
  q: string,
): SearchSelectOption | null {
  const qLower = q.trim().toLowerCase();
  if (!qLower) return null;
  const exact = results.filter((r) => String(r.code ?? '').toLowerCase() === qLower);
  return exact.length === 1 ? exact[0] : null;
}

export interface SearchSelectState {
  // Modal state
  open: boolean;
  query: string;
  options: SearchSelectOption[];
  loading: boolean;
  focusedIdx: number;
  tableActive: boolean;
  page: number;
  total: number;
  localSelected: Set<string>;
  localSingle: string;
  localSingleLabel: string;
  localSingleMeta: string;
  displayLabel: string;
  // Single inline-search state
  inputText: string;
  inputFocused: boolean;
  dropdownOpen: boolean;
  dropdownOptions: SearchSelectOption[];
  dropdownLoading: boolean;
  // Computed
  columns: SearchSelectColumn[];
  totalPages: number;
  displayOptions: SearchSelectOption[];
  colSpan: number;
  confirmCount: number;
  triggerDisplay: string;
  allChecked: boolean;
  someChecked: boolean;
  isMulti: boolean;
  resolvedTitle: string;
  // Refs
  searchRef: React.RefObject<HTMLInputElement | null>;
  triggerRef: React.RefObject<HTMLInputElement | null>;
  scrollRef: React.RefObject<HTMLDivElement | null>;
  dropdownRef: React.RefObject<HTMLDivElement | null>;
}

export interface SearchSelectHandlers {
  openModal: (initialQuery?: string) => void;
  /** Cancel/ESC: true/'trigger'; submit/pilih: 'next' (advance to next field). */
  closeModal: (refocus?: boolean | 'trigger' | 'next') => void;
  handleTriggerFocus: () => void;
  handleSingleFocus: () => void;
  handleSingleBlur: (e: React.FocusEvent) => void;
  handleSingleChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  handleIconMouseDown: (e: React.MouseEvent) => void;
  handleSingleKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => void;
  selectFromDropdown: (opt: SearchSelectOption) => void;
  selectSingle: (opt: SearchSelectOption, idx?: number) => void;
  confirmRow: (opt: SearchSelectOption) => void;
  toggleMulti: (val: string) => void;
  confirm: () => void;
  toggleAll: () => void;
  handleKeyDown: (e: React.KeyboardEvent) => void;
  goToPage: (page: number) => void;
  setQuery: React.Dispatch<React.SetStateAction<string>>;
  setPage: React.Dispatch<React.SetStateAction<number>>;
}
