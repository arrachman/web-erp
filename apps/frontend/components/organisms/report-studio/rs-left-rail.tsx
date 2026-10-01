'use client';

import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';
import { Hov } from './rs-shared';

const FIELD_HOVER = 'background:var(--accent-weak,#e7efff)';
const ROW_HOVER = 'background:var(--hover,#eef2f8)';

function DataTree({ v }: { v: RsVals }) {
  return (
    <>
      {v.dataTree.map((ds) => (
        <div key={ds.name}>
          <div style={s("padding:6px 12px 3px;font-size:10.5px;font-weight:700;letter-spacing:.03em;text-transform:uppercase;color:var(--accent,#2563eb);font-family:'IBM Plex Mono',monospace")}>{ds.name}</div>
          {ds.tables.map((tb) => (
            <div key={tb.name}>
              <Hov onClick={tb.onToggle} base="display:flex;align-items:center;gap:7px;padding:5px 12px 5px 16px;cursor:pointer;font-size:12.5px;font-weight:600;color:var(--text,#1d2330)" hover={ROW_HOVER}>
                <span style={s('color:var(--muted,#6b7280);font-size:9px;width:8px')}>{tb.caret}</span>
                <svg width="13" height="13" viewBox="0 0 16 16" style={s('opacity:.55')}><rect x="2" y="3" width="12" height="10" rx="1.5" stroke="currentColor" strokeWidth="1.3" fill="none" /><path d="M2 6h12" stroke="currentColor" strokeWidth="1.3" /></svg>
                {tb.name}
              </Hov>
              {tb.open && tb.fields.map((f) => (
                <Hov key={f.path} draggable data-path={f.path} onDragStart={v.onFieldDragStart} base="display:flex;align-items:center;gap:8px;padding:4px 12px 4px 38px;cursor:grab;font-size:12px;color:var(--text,#1d2330)" hover={FIELD_HOVER}>
                  <span style={s("width:15px;height:15px;flex:0 0 15px;border-radius:4px;background:var(--panel2,#f0f2f5);display:flex;align-items:center;justify-content:center;font-family:'IBM Plex Mono',monospace;font-size:9px;font-weight:600;color:var(--muted,#6b7280)")}>{f.badge}</span>
                  {f.name}
                </Hov>
              ))}
            </div>
          ))}
        </div>
      ))}
      <div style={s('height:14px')} />
    </>
  );
}

export function RsLeftRail({ v }: { v: RsVals }) {
  if (v.leftClosed) {
    return (
      <Hov onClick={v.toggleLeftPanel} title="Expand data panel" base="width:28px;flex:0 0 28px;background:var(--panel,#fff);border-right:1px solid var(--border,#e1e5ea);display:flex;flex-direction:column;align-items:center;padding-top:10px;gap:10px;cursor:pointer" hover={ROW_HOVER}>
        <span style={s('color:var(--accent,#2563eb);font-size:14px')}>›</span>
        <span style={s('writing-mode:vertical-rl;font-size:11px;font-weight:600;color:var(--muted,#6b7280);letter-spacing:.05em')}>{v.t.data}</span>
      </Hov>
    );
  }
  return (
    <aside style={s('width:248px;flex:0 0 248px;display:flex;flex-direction:column;background:var(--panel,#fff);border-right:1px solid var(--border,#e1e5ea);min-height:0')}>
      <div style={s('display:flex;align-items:center;padding:6px 6px 0;gap:2px;border-bottom:1px solid var(--border,#e1e5ea)')}>
        {v.leftTabs.map((tab) => <button key={tab.label} onClick={tab.onClick} style={s(tab.style)}>{tab.label}</button>)}
        <Hov as="button" onClick={v.toggleLeftPanel} title="Collapse" base="margin-left:auto;width:24px;height:26px;border:none;background:transparent;color:var(--muted,#6b7280);cursor:pointer;border-radius:5px" hover={ROW_HOVER}>‹</Hov>
      </div>
      <div style={s('flex:1;overflow:auto;min-height:0')}>
        {v.tabData && (
          <>
            <div style={s('padding:10px 12px;border-bottom:1px solid var(--border,#e1e5ea)')}>
              <div style={s('font-size:10px;font-weight:700;letter-spacing:.04em;text-transform:uppercase;color:var(--muted,#6b7280);margin-bottom:6px;display:flex;align-items:center;gap:6px')}>SQL{v.sqlLoading ? <span style={s('font-weight:500;text-transform:none;color:var(--accent,#2563eb)')}>· memuat…</span> : null}</div>
              <textarea value={v.sql} onChange={(e) => v.onSql(e.target.value)} placeholder="SELECT … (kosong = data contoh)" spellCheck={false} style={s("width:100%;height:62px;resize:vertical;padding:6px 8px;border:1px solid var(--border,#e1e5ea);border-radius:7px;background:var(--panel2,#f7f8fa);color:var(--text,#1d2330);font-size:11px;font-family:'IBM Plex Mono',monospace;line-height:1.4")} />
              {v.sqlErr ? <div style={s('margin-top:5px;font-size:10.5px;color:#dc2626;line-height:1.3')}>{v.sqlErr}</div> : null}
            </div>
            <div style={s('padding:10px 12px;border-bottom:1px solid var(--border,#e1e5ea)')}>
              <div style={s('font-size:10px;font-weight:700;letter-spacing:.04em;text-transform:uppercase;color:var(--muted,#6b7280);margin-bottom:6px')}>{v.t.groupBy}</div>
              <select value={v.groupBy} onChange={(e) => v.onGroupBy(e.target.value)} style={s("width:100%;height:30px;padding:0 9px;border:1px solid var(--border,#e1e5ea);border-radius:7px;background:var(--panel2,#f7f8fa);color:var(--text,#1d2330);font-size:12px;font-family:'IBM Plex Mono',monospace")}>
                {v.groupOptions.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}
              </select>
            </div>
            <div style={s('padding:8px 12px 4px;font-size:10px;font-weight:600;letter-spacing:.04em;text-transform:uppercase;color:var(--muted,#6b7280)')}>{v.t.dragHint}</div>
            <DataTree v={v} />
          </>
        )}

        {v.tabRel && (
          <>
            <div style={s('padding:11px 12px;font-size:11.5px;line-height:1.5;color:var(--muted,#6b7280)')}>{v.t.relHint}</div>
            {v.relations.map((r) => (
              <div key={r.left} style={s('margin:0 10px 8px;padding:10px;border:1px solid var(--border,#e1e5ea);border-radius:9px;background:var(--panel2,#f7f8fa)')}>
                <div style={s("font-family:'IBM Plex Mono',monospace;font-size:11px;color:var(--text,#1d2330);line-height:1.5")}><span style={s('color:var(--accent,#2563eb)')}>{r.left}</span></div>
                <div style={s("font-family:'IBM Plex Mono',monospace;font-size:11px;color:var(--muted,#6b7280);margin:1px 0 8px")}>↳ {r.right}</div>
                <button onClick={r.onToggle} style={s(r.btnStyle)}>{r.mode}</button>
              </div>
            ))}
          </>
        )}

        {v.tabParam && (
          <>
            <div style={s('padding:8px 12px 4px;font-size:10px;font-weight:600;letter-spacing:.04em;text-transform:uppercase;color:var(--muted,#6b7280)')}>{v.t.dragHint}</div>
            {v.params.map((p) => (
              <Hov key={p.name} draggable data-path={p.path} onDragStart={v.onFieldDragStart} base="display:flex;align-items:center;gap:9px;margin:0 10px 6px;padding:8px 10px;border:1px solid var(--border,#e1e5ea);border-radius:8px;background:var(--panel2,#f7f8fa);cursor:grab" hover="border-color:var(--accent,#2563eb)">
                <span style={s("font-family:'IBM Plex Mono',monospace;font-size:12px;font-weight:600;color:var(--accent,#2563eb)")}>{p.name}</span>
                <span style={s('margin-left:auto;font-size:11px;color:var(--muted,#6b7280)')}>{p.val}</span>
              </Hov>
            ))}
          </>
        )}

        {v.tabFunc && (
          <>
            <div style={s('padding:8px 12px 4px;font-size:10px;font-weight:600;letter-spacing:.04em;text-transform:uppercase;color:var(--muted,#6b7280)')}>{v.t.dragHint}</div>
            {v.funcs.map((fn) => (
              <Hov key={fn.sig} draggable data-path={fn.path} onDragStart={v.onFieldDragStart} base="margin:0 10px 6px;padding:8px 10px;border:1px solid var(--border,#e1e5ea);border-radius:8px;background:var(--panel2,#f7f8fa);cursor:grab" hover="border-color:var(--accent,#2563eb)">
                <div style={s("font-family:'IBM Plex Mono',monospace;font-size:12px;font-weight:600;color:var(--text,#1d2330)")}>{fn.sig}</div>
                <div style={s('font-size:11px;color:var(--muted,#6b7280);margin-top:2px')}>{fn.desc}</div>
              </Hov>
            ))}
          </>
        )}
      </div>
    </aside>
  );
}
