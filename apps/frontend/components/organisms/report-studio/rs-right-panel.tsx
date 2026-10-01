'use client';

import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';
import { Hov } from './rs-shared';
import { RsPropGrid } from './rs-prop-grid';

const ROW_HOVER = 'background:var(--hover,#eef2f8)';
const FIELD_HOVER = 'background:var(--accent-weak,#e7efff)';

export function RsRightPanel({ v }: { v: RsVals }) {
  return (
    <aside style={s('width:300px;flex:0 0 300px;display:flex;flex-direction:column;background:var(--panel,#fff);border-left:1px solid var(--border,#e1e5ea);min-height:0')}>
      <div style={s('padding:8px 10px 7px;font-size:11px;font-weight:700;letter-spacing:.05em;text-transform:uppercase;color:var(--muted,#6b7280);border-bottom:1px solid var(--border,#e1e5ea)')}>{v.rightTitle}</div>

      {v.rightProps && (
        <>
          <div style={s('padding:8px 10px;border-bottom:1px solid var(--border,#e1e5ea)')}>
            <select value={v.compSel} onChange={v.onSelectComponent} style={s('width:100%;height:30px;padding:0 9px;border:1px solid var(--border,#e1e5ea);border-radius:7px;background:var(--panel2,#f7f8fa);color:var(--text,#1d2330);font-size:12px;font-weight:600')}>
              <option value="">{v.t.selectComp}</option>
              {v.componentOptions.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}
            </select>
          </div>
          <div style={s('flex:1;overflow:auto;min-height:0')}>
            {v.noSel && <div style={s('padding:24px 16px;font-size:12.5px;line-height:1.55;color:var(--muted,#6b7280);text-align:center')}>{v.t.noSel}</div>}
            <RsPropGrid v={v} />
          </div>
          <div style={s('border-top:1px solid var(--border,#e1e5ea);padding:9px 12px;background:var(--panel2,#f7f8fa)')}>
            <div style={s('font-size:12.5px;font-weight:700;color:var(--text,#1d2330)')}>{v.selName}</div>
            <div style={s('font-size:11px;color:var(--muted,#6b7280);margin-top:2px;line-height:1.4')}>{v.selDesc}</div>
          </div>
        </>
      )}

      {v.rightTree && (
        <div style={s('flex:1;overflow:auto;padding:6px 6px 10px;min-height:0')}>
          {v.structure.map((n, i) => <div key={i} onClick={n.onClick} style={s(n.style)}>{n.label}</div>)}
        </div>
      )}

      {v.rightDict && (
        <div style={s('flex:1;overflow:auto;padding:6px 0 12px;min-height:0')}>
          {v.dataTree.map((ds) => (
            <div key={ds.name}>
              <div style={s("padding:6px 12px 3px;font-size:10.5px;font-weight:700;letter-spacing:.03em;text-transform:uppercase;color:var(--accent,#2563eb);font-family:'IBM Plex Mono',monospace")}>{ds.name}</div>
              {ds.tables.map((tb) => (
                <div key={tb.name}>
                  <Hov onClick={tb.onToggle} base="display:flex;align-items:center;gap:7px;padding:5px 12px 5px 16px;cursor:pointer;font-size:12.5px;font-weight:600;color:var(--text,#1d2330)" hover={ROW_HOVER}>
                    <span style={s('color:var(--muted,#6b7280);font-size:9px;width:8px')}>{tb.caret}</span>{tb.name}
                  </Hov>
                  {tb.open && tb.fields.map((f) => (
                    <Hov key={f.path} draggable data-path={f.path} onDragStart={v.onFieldDragStart} base="display:flex;align-items:center;gap:8px;padding:4px 12px 4px 38px;cursor:grab;font-size:12px;color:var(--text,#1d2330)" hover={FIELD_HOVER}>
                      <span style={s("width:15px;height:15px;flex:0 0 15px;border-radius:4px;background:var(--panel2,#f0f2f5);display:flex;align-items:center;justify-content:center;font-family:'IBM Plex Mono',monospace;font-size:9px;font-weight:600;color:var(--muted,#6b7280)")}>{f.badge}</span>{f.name}
                    </Hov>
                  ))}
                </div>
              ))}
            </div>
          ))}
        </div>
      )}

      <div style={s('display:flex;border-top:1px solid var(--border,#e1e5ea);background:var(--panel2,#f7f8fa)')}>
        {v.rightBottomTabs.map((bt) => <button key={bt.label} onClick={bt.onClick} style={s(bt.style)}>{bt.label}</button>)}
      </div>
    </aside>
  );
}
