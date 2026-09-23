'use client';

import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';

export function RsPropGrid({ v }: { v: RsVals }) {
  return (
    <>
      {v.propGroups.map((g) => (
        <div key={g.key}>
          <div onClick={g.onToggle} style={s(g.headStyle)}>
            <span style={s('width:12px;font-size:9px;color:var(--muted,#6b7280)')}>{g.chev}</span>{g.title}
          </div>
          {g.open && g.rows.map((r, i) => (
            <div key={i} style={s('display:flex;align-items:center;min-height:28px;border-bottom:1px solid var(--border,#eef0f3)')}>
              <div style={s('flex:0 0 112px;padding:0 8px 0 22px;font-size:11.5px;color:var(--muted,#5a6473)')}>{r.label}</div>
              <div style={s('flex:1;padding:3px 8px 3px 0')}>
                {r.isText && <input value={r.value} onChange={r.onInput} style={s(v.gridInput)} />}
                {r.isNum && <input type="number" value={r.value} onChange={r.onInput} style={s(v.gridInput)} />}
                {r.isSelect && <select value={r.value} onChange={r.onChange} style={s(v.gridSelect)}>{r.options!.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select>}
                {r.isBool && <select value={r.value} onChange={r.onChange} style={s(v.gridSelect)}>{r.options!.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select>}
                {r.isColor && <div style={s('display:flex;gap:3px;flex-wrap:wrap')}>{r.swatches!.map((c, j) => <button key={j} onClick={c.onClick} style={s(c.style)} />)}</div>}
              </div>
            </div>
          ))}
        </div>
      ))}
    </>
  );
}
