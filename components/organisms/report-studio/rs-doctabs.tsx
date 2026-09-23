'use client';

import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';

export function RsDocTabs({ v }: { v: RsVals }) {
  return (
    <div style={s('display:flex;align-items:center;gap:2px;height:32px;flex:0 0 32px;padding:0 10px;background:var(--panel2,#f0f2f5);border-bottom:1px solid var(--border,#e1e5ea)')}>
      {v.docTabs.map((dt) => (
        <button key={dt.label} onClick={dt.onClick} style={s(dt.style)}>
          <span style={s('display:flex;align-items:center;gap:6px')}>{dt.label}</span>
        </button>
      ))}
    </div>
  );
}
