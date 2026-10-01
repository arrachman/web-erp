'use client';

import * as React from 'react';
import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';
import { Hov } from './rs-shared';

// Light ERP-token select with a custom caret (matches the ERP design system chrome).
const SELECT_LIGHT = 'height:30px;padding:0 26px 0 9px;border:1px solid var(--border,#e1e5ea);border-radius:var(--radius,5px);background:var(--panel2,#f5f6f9);color:var(--text,#1d2330);font-size:12px;font-weight:600;cursor:pointer;appearance:none;background-image:linear-gradient(45deg,transparent 50%,var(--muted,#6b7280) 50%),linear-gradient(135deg,var(--muted,#6b7280) 50%,transparent 50%);background-position:calc(100% - 14px) 13px,calc(100% - 9px) 13px;background-size:5px 5px,5px 5px;background-repeat:no-repeat';
const DIVIDER = 'width:1px;height:22px;background:var(--border,#e1e5ea)';
const GHOST_BTN = 'height:30px;display:flex;align-items:center;justify-content:center;border:1px solid var(--border,#e1e5ea);background:var(--panel,#fff);color:var(--text,#1d2330);border-radius:var(--radius,5px);cursor:pointer';

export function RsQuickbar({ v }: { v: RsVals }) {
  return (
    <header style={s('height:44px;flex:0 0 44px;display:flex;align-items:center;gap:8px;padding:0 12px;background:var(--panel,#fff);border-bottom:1px solid var(--border,#e1e5ea);z-index:40')}>
      {/* Load / switch template — labelled so the control reads as "open template". */}
      <span style={s('font-size:11px;font-weight:600;color:var(--muted,#6b7280)')}>Template</span>
      <select value={v.tplKey} onChange={v.onTemplate} title="Template" style={s(SELECT_LIGHT + ';min-width:170px;max-width:220px')}>
        {v.templateOptions.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}
      </select>

      <div style={s(DIVIDER)} />
      <div style={s('display:flex;align-items:center;gap:3px')}>
        <button onClick={v.onUndo} title="Undo (Ctrl+Z)" style={s(v.undoStyle)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M6 4L2.5 7 6 10M3 7h7a3 3 0 0 1 0 6H7" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" /></svg></button>
        <button onClick={v.onRedo} title="Redo (Ctrl+Y)" style={s(v.redoStyle)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M10 4l3.5 3L10 10M13 7H6a3 3 0 0 0 0 6h3" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" /></svg></button>
      </div>

      <div style={s(DIVIDER)} />
      {/* Document name — reads as the report title being edited. */}
      <svg width="15" height="15" viewBox="0 0 16 16" style={s('flex:0 0 auto;opacity:.55;color:var(--muted,#6b7280)')}><path d="M3 2h7l3 3v9H3z" fill="none" stroke="currentColor" strokeWidth="1.2" /><path d="M10 2v3h3" fill="none" stroke="currentColor" strokeWidth="1.2" /></svg>
      <Hov as="input" value={v.reportName} onChange={v.onName} placeholder={v.t.reportName}
        base="height:30px;width:240px;min-width:120px;padding:0 9px;border:1px solid transparent;border-radius:var(--radius,5px);background:transparent;color:var(--text,#1d2330);font-size:13px;font-weight:600"
        hover="background:var(--hover,#eef2f8)"
        focus="border:1px solid var(--accent,#2563eb);background:var(--panel,#fff)" />

      <div style={s('margin-left:auto;display:flex;align-items:center;gap:7px')}>
        <button onClick={v.toggleLang} title="Language" style={s(GHOST_BTN + ';min-width:36px;padding:0 8px;font-size:11px;font-weight:700')}>{v.langBtnLabel}</button>
        <button onClick={v.toggleTheme} title="Theme" style={s(GHOST_BTN + ';width:32px')}>{v.themeGlyph}</button>
        <div style={s(DIVIDER)} />
        {/* Primary daily action. */}
        <button onClick={v.onSave} title="Simpan (Ctrl+S)" style={s('height:30px;padding:0 15px;display:flex;align-items:center;gap:7px;border:none;background:var(--accent,#2563eb);color:#fff;font-size:12.5px;font-weight:600;border-radius:var(--radius,5px);cursor:pointer')}>
          <svg width="13" height="13" viewBox="0 0 16 16"><path d="M3 2h8l2 2v10H3zM5 2v4h6V2M5 11h6" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinejoin="round" /></svg>{v.t.export === 'Export' ? 'Save' : 'Simpan'}
        </button>
        <div style={s('position:relative')}>
          <button onClick={v.toggleExport} style={s(GHOST_BTN + ';padding:0 12px;gap:6px;font-size:12px;font-weight:600')}>
            <svg width="12" height="12" viewBox="0 0 16 16"><path d="M8 2v7M5 6.2l3 3 3-3M3 12.5h10" stroke="currentColor" strokeWidth="1.5" fill="none" strokeLinecap="round" strokeLinejoin="round" /></svg>{v.t.export}
          </button>
          {v.expOpen && (
            <div style={s('position:absolute;right:0;top:36px;width:182px;background:var(--panel,#fff);border:1px solid var(--border,#e1e5ea);border-radius:10px;box-shadow:0 14px 40px rgba(0,0,0,.22);padding:5px;z-index:60;animation:rsPop .12s ease')}>
              {v.exportItems.map((x) => (
                <Hov as="button" key={x.ext} onClick={x.onClick}
                  base="width:100%;display:flex;align-items:center;gap:9px;height:32px;padding:0 9px;border:none;background:transparent;color:var(--text,#1d2330);font-size:12.5px;font-weight:500;border-radius:7px;cursor:pointer;text-align:left"
                  hover="background:var(--hover,#eef2f8)">
                  <span style={s("width:32px;font-family:'IBM Plex Mono',monospace;font-size:9.5px;font-weight:600;color:var(--accent,#2563eb)")}>{x.ext}</span>{x.label}
                </Hov>
              ))}
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
