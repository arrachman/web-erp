/** Shared inline-CSS style strings ported verbatim from ReportStudio renderVals(). */

export const qBtn = 'width:28px;height:28px;display:flex;align-items:center;justify-content:center;border:1px solid rgba(255,255,255,.14);background:rgba(255,255,255,.06);border-radius:6px;cursor:pointer;';

/** Light chrome toolbar button (ERP-token aligned) — used by the integrated quickbar. */
export const qIconLight = 'width:30px;height:30px;display:flex;align-items:center;justify-content:center;border:1px solid var(--border,#e1e5ea);background:var(--panel,#fff);border-radius:var(--radius,5px);cursor:pointer;';

export const rtBase = 'height:28px;padding:0 14px;border:none;background:transparent;font-size:12.5px;font-weight:600;cursor:pointer;border-radius:6px 6px 0 0;';
export const rGroup = 'display:flex;flex-direction:column;align-items:center;justify-content:space-between;padding:4px 8px 3px;flex:0 0 auto;';
export const rLabel = 'font-size:9px;color:var(--muted,#6b7280);letter-spacing:.02em;margin-top:2px;text-align:center;';
export const rDiv = 'width:1px;align-self:center;height:66px;background:var(--border,#e1e5ea);margin:0 1px;';
export const rBtn = 'min-width:26px;height:26px;display:flex;align-items:center;justify-content:center;gap:1px;border:1px solid transparent;background:transparent;color:var(--text,#1d2330);border-radius:5px;cursor:pointer;font-size:12px;padding:0 4px;';
export const rBtnWide = 'display:flex;align-items:center;gap:6px;height:22px;padding:0 7px;border:1px solid transparent;background:transparent;color:var(--text,#1d2330);border-radius:5px;cursor:pointer;font-size:11px;';
export const ribbonSelectWide = 'height:24px;min-width:118px;max-width:140px;border:1px solid var(--border,#e1e5ea);border-radius:5px;background:var(--panel2,#f5f6f9);color:var(--text,#1d2330);font-size:11.5px;padding:0 6px;cursor:pointer;';
export const ribbonSelectSm = 'height:24px;width:56px;border:1px solid var(--border,#e1e5ea);border-radius:5px;background:var(--panel2,#f5f6f9);color:var(--text,#1d2330);font-size:11.5px;padding:0 4px;cursor:pointer;';

export const tgl = (on: boolean) =>
  'min-width:26px;height:26px;display:flex;align-items:center;justify-content:center;border:1px solid ' + (on ? 'var(--accent,#2563eb)' : 'transparent') + ';background:' + (on ? 'var(--accent-weak,#e7efff)' : 'transparent') + ';color:' + (on ? 'var(--accent,#2563eb)' : 'var(--text,#1d2330)') + ';border-radius:5px;cursor:pointer;font-size:13px;font-weight:700;padding:0 4px;';
export const wideTgl = (on: boolean) =>
  'display:flex;align-items:center;gap:7px;height:24px;padding:0 9px;border:1px solid ' + (on ? 'var(--accent,#2563eb)' : 'var(--border,#e1e5ea)') + ';background:' + (on ? 'var(--accent-weak,#e7efff)' : 'var(--panel2,#f5f6f9)') + ';color:' + (on ? 'var(--accent,#2563eb)' : 'var(--text,#1d2330)') + ';border-radius:6px;cursor:pointer;font-weight:600;';

export const pasteBtnStyle = (clipHas: boolean) =>
  'display:flex;flex-direction:column;align-items:center;justify-content:center;gap:2px;width:54px;height:62px;border:1px solid ' + (clipHas ? 'var(--border,#e1e5ea)' : 'transparent') + ';background:' + (clipHas ? 'var(--panel2,#f5f6f9)' : 'transparent') + ';color:' + (clipHas ? 'var(--accent,#2563eb)' : 'var(--muted,#9aa3b2)') + ';border-radius:7px;cursor:pointer;';

export const colorPopStyle = 'position:absolute;top:30px;left:0;z-index:70;display:grid;grid-template-columns:repeat(6,1fr);gap:4px;padding:7px;background:var(--panel,#fff);border:1px solid var(--border,#e1e5ea);border-radius:9px;box-shadow:0 12px 32px rgba(0,0,0,.2);width:152px;animation:rsPop .1s ease;';

export const bar = (c?: string) => 'display:block;width:14px;height:3px;border-radius:1px;background:' + (c || '#14181f') + ';margin-top:1px;';

export const gridInput = 'width:100%;height:24px;padding:0 6px;border:1px solid var(--border,#e1e5ea);border-radius:5px;background:var(--panel,#fff);color:var(--text,#1d2330);font-size:11.5px;';
export const gridSelect = gridInput + 'cursor:pointer;';
export const headStyle = 'display:flex;align-items:center;gap:4px;padding:6px 10px;font-size:11.5px;font-weight:700;color:var(--text,#1d2330);background:var(--panel2,#f5f6f9);border-bottom:1px solid var(--border,#e1e5ea);cursor:pointer;position:sticky;top:0;';

export const dtBase = 'height:26px;padding:0 14px;border:none;border-radius:6px 6px 0 0;font-size:12px;font-weight:600;cursor:pointer;display:flex;align-items:center;';
export const rbBase = 'flex:1;height:30px;border:none;background:transparent;font-size:11px;font-weight:600;cursor:pointer;border-top:2px solid transparent;';

/** Swatch button style (selected ring when current color matches). */
export const swatchStyle = (c: string, selected: boolean, ring = 3.5) =>
  'width:18px;height:18px;border-radius:4px;cursor:pointer;background:' + c + ';border:1px solid ' + (c === '#ffffff' ? '#d0d4da' : 'rgba(0,0,0,.15)') + ';' + (selected ? 'box-shadow:0 0 0 2px #fff,0 0 0 ' + ring + 'px var(--accent,#2563eb);' : '');
export const pSwatchStyle = (c: string, selected: boolean) =>
  'width:17px;height:17px;border-radius:4px;cursor:pointer;background:' + c + ';border:1px solid ' + (c === '#ffffff' ? '#d0d4da' : 'rgba(0,0,0,.15)') + ';' + (selected ? 'box-shadow:0 0 0 1.5px #fff,0 0 0 3px var(--accent,#2563eb);' : '');
