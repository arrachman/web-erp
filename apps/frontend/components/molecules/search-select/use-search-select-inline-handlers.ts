'use client';

import * as React from 'react';
import { SearchSelectOption } from '../search-select-types';
import { optLabel, pickExactCodeMatch } from '../use-search-select-types';
import { focusNextFrom } from './search-select-focus';

/**
 * Context passed from the main `useSearchSelect` hook to the inline-search
 * handler group. All refs are the SAME ref objects owned by the main hook —
 * never mirrored/recreated — so ref identity is preserved across renders.
 *
 * The main hook must construct the modal handler group FIRST (because it
 * owns `openModal`) and pass `openModal` in here.
 */
export interface InlineHandlersCtx {
  // Mode / props callbacks. SearchSelectProps is a discriminated union (single
  // vs multi); the main hook passes a no-op for the branch not matching `isMulti`
  // so these stay required — the no-op is never invoked because every call site
  // is guarded by `!isMulti`.
  isMulti: boolean;
  onValueChange: (v: string) => void;
  onPick?: (o: SearchSelectOption) => void;
  loadOptions: (q: string, p: number, limit: number) => Promise<{ data: SearchSelectOption[]; total: number }>;
  limit: number;
  openModal: (initialQuery?: string) => void;

  // Inline state values
  inputText: string;
  displayLabel: string;

  // Inline state setters
  setInputText: React.Dispatch<React.SetStateAction<string>>;
  setInputFocused: React.Dispatch<React.SetStateAction<boolean>>;
  setDisplayLabel: React.Dispatch<React.SetStateAction<string>>;
  setDropdownOpen: React.Dispatch<React.SetStateAction<boolean>>;
  setDropdownLoading: React.Dispatch<React.SetStateAction<boolean>>;

  // Refs (shared identity with main hook)
  searchRef: React.RefObject<HTMLInputElement | null>;
  triggerRef: React.RefObject<HTMLInputElement | null>;
  dropdownRef: React.RefObject<HTMLDivElement | null>;
  skipNextFocusRef: React.MutableRefObject<boolean>;
  ignoreNextBlurRef: React.MutableRefObject<boolean>;
  dropdownTimer: React.MutableRefObject<ReturnType<typeof setTimeout> | null>;
}

/**
 * Inline-search handler group (single-select input behavior): focus, blur,
 * change, search-icon click, Enter commit, and dropdown selection.
 *
 * Extracted from use-search-select.ts (original L168-304) — content preserved
 * exactly. Only `openModal` is currently `useCallback`-memoized in the main
 * hook; the handlers returned here are NOT memoized wholesale to preserve
 * the identity observed by effects.
 */
export function useSearchSelectInlineHandlers(ctx: InlineHandlersCtx) {
  const {
    isMulti, onValueChange, onPick, loadOptions, limit, openModal,
    inputText, displayLabel,
    setInputText, setInputFocused, setDisplayLabel, setDropdownOpen, setDropdownLoading,
    triggerRef, dropdownRef, skipNextFocusRef, ignoreNextBlurRef, dropdownTimer,
  } = ctx;

  const handleSingleFocus = () => {
    if (skipNextFocusRef.current) { skipNextFocusRef.current = false; return; }
    setInputFocused(true);
  };

  const handleSingleBlur = async (e: React.FocusEvent) => {
    if (dropdownRef.current?.contains(e.relatedTarget as Node)) return;
    if (ignoreNextBlurRef.current) {
      ignoreNextBlurRef.current = false;
      setInputFocused(false);
      return;
    }
    // Simpan sebelum await — relatedTarget hilang setelah event loop tick
    const blurTarget = e.relatedTarget as HTMLElement | null;
    setDropdownOpen(false);
    if (!inputText) {
      if (!isMulti) { onValueChange(''); setDisplayLabel(''); }
      setInputFocused(false);
      return;
    }
    if (inputText !== displayLabel) {
      setDropdownLoading(true);
      try {
        const { data: results } = await loadOptions(inputText, 1, limit);
        if (results.length === 0) {
          // Tidak ditemukan → revert (jangan buka modal saat blur)
          if (!isMulti) onValueChange('');
          setDisplayLabel('');
          setInputText('');
          setInputFocused(false);
          return;
        }
        const exact = pickExactCodeMatch(results, inputText);
        if (exact) {
          selectFromDropdown(exact);
          blurTarget?.focus(); // kembalikan fokus ke field tujuan user
          return;
        }
        if (results.length === 1) {
          selectFromDropdown(results[0]);
          blurTarget?.focus(); // kembalikan fokus ke field tujuan user
          return;
        }
        // Banyak hasil, tidak ada exact match → revert (jangan buka modal saat blur)
        setInputFocused(false);
        setInputText(displayLabel);
        return;
      } catch {
        // fall through to revert
      } finally {
        setDropdownLoading(false);
      }
    }
    setInputFocused(false);
    setInputText(displayLabel);
  };

  const handleSingleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setInputText(val);
    if (dropdownTimer.current) { clearTimeout(dropdownTimer.current); dropdownTimer.current = null; }
    setDropdownOpen(false);
    if (!val && !isMulti) {
      onValueChange('');
      setDisplayLabel('');
    }
  };

  const handleIconMouseDown = (e: React.MouseEvent) => {
    e.preventDefault(); // keep input focused, avoid blur
    // Buka dengan query kosong kecuali user sudah mengetik sesuatu yang berbeda dari display
    const typedQuery = !isMulti && inputText !== displayLabel ? inputText : '';
    openModal(isMulti ? '' : typedQuery);
  };

  const handleSingleKeyDown = async (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'F12' || (e.key === 'F12' && (e.metaKey || e.ctrlKey))) {
      e.preventDefault();
      // Buka dengan query kosong kecuali user sudah mengetik sesuatu yang berbeda dari display
      openModal(inputText !== displayLabel ? inputText : '');
      return;
    }
    if (e.key !== 'Enter') return;
    e.preventDefault();
    if (!inputText) return;
    if (dropdownTimer.current) { clearTimeout(dropdownTimer.current); dropdownTimer.current = null; }
    ignoreNextBlurRef.current = true;
    setDropdownOpen(false);
    setDropdownLoading(true);
    try {
      const { data: results } = await loadOptions(inputText, 1, limit);
      if (results.length === 0) {
        // Tidak ditemukan → kosongkan lalu buka modal dengan teks pencarian
        if (!isMulti) onValueChange('');
        setDisplayLabel('');
        setInputText('');
        ignoreNextBlurRef.current = false;
        openModal(inputText);
        return;
      }
      const exact = pickExactCodeMatch(results, inputText);
      if (exact) {
        selectFromDropdown(exact);
        setTimeout(() => focusNextFrom(triggerRef.current), 0);
      } else if (results.length === 1) {
        selectFromDropdown(results[0]);
        setTimeout(() => focusNextFrom(triggerRef.current), 0);
      } else {
        openModal(inputText);
      }
    } finally {
      setDropdownLoading(false);
    }
  };

  const selectFromDropdown = (opt: SearchSelectOption) => {
    if (!isMulti) {
      onValueChange(opt.value);
      onPick?.({ ...opt, label: optLabel(opt), meta: String(opt.meta ?? '') });
      setDisplayLabel(optLabel(opt));
      setInputText(optLabel(opt));
    }
    setDropdownOpen(false);
    setInputFocused(false);
  };

  return {
    handleSingleFocus,
    handleSingleBlur,
    handleSingleChange,
    handleIconMouseDown,
    handleSingleKeyDown,
    selectFromDropdown,
  };
}