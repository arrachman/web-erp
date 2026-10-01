'use client';

import * as React from 'react';
import { DEFAULT_COLS, SearchSelectColumn, SearchSelectOption, SearchSelectProps } from './search-select-types';
import {
  SearchSelectHandlers,
  SearchSelectState,
  optLabel,
} from './use-search-select-types';
import { useSearchSelectInlineHandlers } from './search-select/use-search-select-inline-handlers';
import { useSearchSelectModalHandlers } from './search-select/use-search-select-modal-handlers';

export type { SearchSelectHandlers, SearchSelectState } from './use-search-select-types';

export function useSearchSelect(props: SearchSelectProps): SearchSelectState & SearchSelectHandlers {
  const {
    placeholder = 'Pilih…', loadOptions,
    debounceMs = 300, columns: columnsProp, limit = 10, initialLabel,
  } = props;

  const resolvedTitle = (props.title ?? placeholder.replace(/…$/, '').trim());
  const isMulti = props.mode === 'multi';

  // ── Modal state ───────────────────────────────────────────────────────────
  const [open, setOpen] = React.useState(false);
  const [query, setQuery] = React.useState('');
  const [options, setOptions] = React.useState<SearchSelectOption[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [focusedIdx, setFocusedIdx] = React.useState(0);
  const [tableActive, setTableActive] = React.useState(false);
  const [page, setPage] = React.useState(1);
  const [total, setTotal] = React.useState(0);
  const [debouncedQuery, setDebouncedQuery] = React.useState('');
  const [localSelected, setLocalSelected] = React.useState<Set<string>>(new Set());
  const [localSingle, setLocalSingle] = React.useState('');
  const [localSingleLabel, setLocalSingleLabel] = React.useState('');
  const [localSingleMeta, setLocalSingleMeta] = React.useState('');
  const [displayLabel, setDisplayLabel] = React.useState('');

  // ── Single inline-search state ────────────────────────────────────────────
  const [inputText, setInputText] = React.useState('');
  const [inputFocused, setInputFocused] = React.useState(false);
  const [dropdownOpen, setDropdownOpen] = React.useState(false);
  const [dropdownOptions, setDropdownOptions] = React.useState<SearchSelectOption[]>([]);
  const [dropdownLoading, setDropdownLoading] = React.useState(false);

  const searchRef = React.useRef<HTMLInputElement>(null);
  const triggerRef = React.useRef<HTMLInputElement>(null);
  const scrollRef = React.useRef<HTMLDivElement>(null);
  const dropdownRef = React.useRef<HTMLDivElement>(null);
  const skipNextFocusRef = React.useRef(false);
  const dropdownTimer = React.useRef<ReturnType<typeof setTimeout> | null>(null);
  const ignoreNextBlurRef = React.useRef(false);

  // ── Sync inputText ← displayLabel when not focused ───────────────────────
  React.useEffect(() => {
    if (!inputFocused) setInputText(displayLabel);
  }, [displayLabel, inputFocused]);

  // ── Grid cell edit-mode: seed inline query + focus on mount ──────────────
  React.useEffect(() => {
    if (isMulti) return;
    const seed = props.initialQuery;
    if (seed) { setInputText(seed); setInputFocused(true); }
    if ((props.autoFocus || seed) && !props.autoOpenModal) {
      setTimeout(() => { triggerRef.current?.focus(); }, 0);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ── Resolve single display label on mount ────────────────────────────────
  React.useEffect(() => {
    if (isMulti) return;
    if (initialLabel && !isMulti && props.value) setDisplayLabel(initialLabel);
    loadOptions('', 1, limit).then(({ data }) => {
      setOptions(data);
      const found = data.find((o) => o.value === props.value);
      if (found) setDisplayLabel(optLabel(found));
    }).catch(() => {});
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    if (isMulti) return;
    if (!props.value) { setDisplayLabel(''); return; }
    const found = options.find((o) => o.value === props.value);
    if (found) setDisplayLabel(optLabel(found));
    // Value set async (e.g. form default) after mount, and not on the first
    // options page → fall back to the caller-provided label so it isn't blank.
    else if (initialLabel) setDisplayLabel(initialLabel);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.value, options, initialLabel]);

  // ── Modal debounced search — defers debouncedQuery update; resets page ───
  React.useEffect(() => {
    if (!open) return;
    const t = setTimeout(() => { setDebouncedQuery(query); setPage(1); }, debounceMs);
    return () => clearTimeout(t);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [query, open]);

  // ── Server-side fetch — fires on debouncedQuery OR page change ────────────
  // Tidak reset `tableActive` di sini — biarkan persist saat user nav ←/→ (§2.28).
  React.useEffect(() => {
    if (!open) return;
    let cancelled = false;
    setLoading(true);
    loadOptions(debouncedQuery, page, limit)
      .then(({ data, total: t }) => {
        if (!cancelled) { setOptions(data); setTotal(t); setFocusedIdx(0); setLoading(false); }
      })
      .catch(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedQuery, open, page]);

  // ── Scroll focused row into view ─────────────────────────────────────────
  React.useEffect(() => {
    scrollRef.current?.querySelector<HTMLElement>(`[data-idx="${focusedIdx}"]`)
      ?.scrollIntoView({ block: 'nearest' });
  }, [focusedIdx]);

  // ── Column resolution ─────────────────────────────────────────────────────
  const columns = React.useMemo<SearchSelectColumn[]>(() => columnsProp ?? DEFAULT_COLS, [columnsProp]);

  // ── Pagination (server-driven) ────────────────────────────────────────────
  const totalPages = Math.max(1, Math.ceil(total / limit));
  const displayOptions = options; // server already paginates — no client-side slice

  // SearchSelectProps is a discriminated union (single vs multi): only the
  // branch matching `isMulti` is ever invoked at runtime, so the other branch's
  // callbacks are filled with a no-op at the boundary. Preserves the original
  // behavior exactly (the guarded branches were never reachable before either).
  const noopValues = (_v: string[]) => {};
  const noopString = (_v: string) => {};

  // ── Modal handler group (built FIRST — owns openModal) ─────────────────────
  const modalHandlers = useSearchSelectModalHandlers({
    isMulti,
    value: props.value,
    values: props.values ?? [],
    onValueChange: props.onValueChange ?? noopString,
    onPick: props.onPick,
    onValuesChange: props.onValuesChange ?? noopValues,
    displayLabel,
    options,
    displayOptions,
    focusedIdx,
    tableActive,
    totalPages,
    localSelected,
    localSingle,
    localSingleLabel,
    localSingleMeta,
    setOpen,
    setQuery,
    setDebouncedQuery,
    setFocusedIdx,
    setTableActive,
    setPage,
    setLocalSelected,
    setLocalSingle,
    setLocalSingleLabel,
    setLocalSingleMeta,
    setDisplayLabel,
    setInputFocused,
    setDropdownOpen,
    searchRef,
    triggerRef,
    skipNextFocusRef,
  });

  const { openModal, closeModal, handleTriggerFocus } = modalHandlers;

  // ── Auto-open the modal on mount when requested (grid search-icon click) ──
  // Deferred a tick so the originating click finishes first — otherwise Radix's
  // dismissable layer treats that same interaction as an outside-click and the
  // dialog opens then immediately closes.
  React.useEffect(() => {
    if (!props.autoOpenModal) return;
    const t = setTimeout(() => openModal(''), 0);
    return () => clearTimeout(t);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ── Inline handler group (receives openModal from modal group) ──────────────
  const inlineHandlers = useSearchSelectInlineHandlers({
    isMulti,
    onValueChange: props.onValueChange ?? noopString,
    onPick: props.onPick,
    loadOptions,
    limit,
    openModal,
    inputText,
    displayLabel,
    setInputText,
    setInputFocused,
    setDisplayLabel,
    setDropdownOpen,
    setDropdownLoading,
    searchRef,
    triggerRef,
    dropdownRef,
    skipNextFocusRef,
    ignoreNextBlurRef,
    dropdownTimer,
  });

  const {
    handleSingleFocus, handleSingleBlur, handleSingleChange,
    handleIconMouseDown, handleSingleKeyDown, selectFromDropdown,
  } = inlineHandlers;

  // ── Computed helpers ──────────────────────────────────────────────────────
  const colSpan = columns.length + (isMulti ? 1 : 0);
  const confirmCount = isMulti ? localSelected.size : 0;
  const triggerDisplay = isMulti
    ? props.values.length > 0 ? `${props.values.length} dipilih` : ''
    : displayLabel;

  return {
    // State
    open, query, options, loading, focusedIdx, tableActive, page, total,
    localSelected, localSingle, localSingleLabel, localSingleMeta, displayLabel,
    inputText, inputFocused, dropdownOpen, dropdownOptions, dropdownLoading,
    // Computed
    columns, totalPages, displayOptions, colSpan, confirmCount, triggerDisplay,
    allChecked: modalHandlers.allChecked, someChecked: modalHandlers.someChecked,
    isMulti, resolvedTitle,
    // Refs
    searchRef, triggerRef, scrollRef, dropdownRef,
    // Handlers
    openModal, closeModal,
    handleTriggerFocus, handleSingleFocus, handleSingleBlur,
    handleSingleChange, handleIconMouseDown, handleSingleKeyDown,
    selectFromDropdown,
    selectSingle: modalHandlers.selectSingle,
    confirmRow: modalHandlers.confirmRow,
    toggleMulti: modalHandlers.toggleMulti,
    confirm: modalHandlers.confirm,
    toggleAll: modalHandlers.toggleAll,
    handleKeyDown: modalHandlers.handleKeyDown,
    goToPage: setPage,
    setQuery, setPage,
  };
}