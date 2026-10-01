'use client';

import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';

export function RsRibbonOther({ v }: { v: RsVals }) {
  return (
    <>
      {/* PAGE */}
      {v.isPage && (
        <>
          <div style={s(v.rGroup)}>
            <div style={s('display:flex;flex-direction:column;gap:5px;flex:1;padding-top:8px')}>
              <div style={s('display:flex;align-items:center;gap:6px')}><span style={s('font-size:11px;color:var(--muted,#6b7280);width:56px')}>{v.t.pageSize}</span><select value={v.pageSizeVal} onChange={v.onPageSize} style={s(v.ribbonSelectWide)}>{v.pageSizeOpts.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select></div>
              <div style={s('display:flex;align-items:center;gap:6px')}><span style={s('font-size:11px;color:var(--muted,#6b7280);width:56px')}>{v.t.margins}</span><select value={v.marginVal} onChange={v.onMargin} style={s(v.ribbonSelectWide)}>{v.marginOpts.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select></div>
            </div>
            <div style={s(v.rLabel)}>{v.t.pageSetup}</div>
          </div>
          <div style={s(v.rDiv)} />
          <div style={s(v.rGroup)}>
            <div style={s('display:flex;gap:5px;flex:1;padding-top:6px')}>
              <button onClick={v.setOrientP} style={s(v.orientP + ';flex-direction:column;width:54px;height:58px;gap:3px')}><svg width="22" height="26" viewBox="0 0 22 26"><rect x="2" y="1" width="18" height="24" rx="1.5" fill="none" stroke="currentColor" strokeWidth="1.3" /></svg><span style={s('font-size:10px')}>{v.t.portrait}</span></button>
              <button onClick={v.setOrientL} style={s(v.orientL + ';flex-direction:column;width:54px;height:58px;gap:3px')}><svg width="26" height="22" viewBox="0 0 26 22"><rect x="1" y="2" width="24" height="18" rx="1.5" fill="none" stroke="currentColor" strokeWidth="1.3" /></svg><span style={s('font-size:10px')}>{v.t.landscape}</span></button>
            </div>
            <div style={s(v.rLabel)}>{v.t.orientation}</div>
          </div>
          <div style={s(v.rDiv)} />
          <div style={s(v.rGroup)}>
            <div style={s('display:flex;flex-direction:column;gap:4px;flex:1;padding-top:8px')}>
              <button onClick={v.toggleGuides} style={s(v.guidesBtn)}><span style={s('font-size:11.5px')}>{v.guidesLabel}</span></button>
              <button onClick={v.toggleRuler} style={s(v.rulerBtnWide)}><span style={s('font-size:11.5px')}>{v.rulerLabel}</span></button>
            </div>
            <div style={s(v.rLabel)}>{v.t.show}</div>
          </div>
        </>
      )}

      {/* LAYOUT */}
      {v.isLayout && (
        <>
          <div style={s(v.rGroup)}>
            <div style={s('display:flex;flex-direction:column;gap:4px;flex:1;padding-top:8px')}>
              <button onClick={v.toggleSnap} style={s(v.snapBtn)}><svg width="13" height="13" viewBox="0 0 16 16"><path d="M2 6h2M2 10h2M12 6h2M12 10h2M6 2v2M10 2v2M6 12v2M10 12v2" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" /><rect x="5" y="5" width="6" height="6" rx="1" fill="none" stroke="currentColor" strokeWidth="1.2" /></svg><span style={s('font-size:11px')}>{v.t.snapGrid}</span></button>
              <button onClick={v.toggleShowGrid} style={s(v.gridBtn)}><svg width="13" height="13" viewBox="0 0 16 16"><path d="M2 2h12v12H2zM2 6h12M2 10h12M6 2v12M10 2v12" fill="none" stroke="currentColor" strokeWidth="1" /></svg><span style={s('font-size:11px')}>{v.t.showGrid}</span></button>
            </div>
            <div style={s(v.rLabel)}>{v.t.grid}</div>
          </div>
          <div style={s(v.rDiv)} />
          <div style={s(v.rGroup)}>
            <div style={s('display:flex;align-items:center;gap:6px;flex:1;padding-top:8px')}><span style={s('font-size:11px;color:var(--muted,#6b7280)')}>{v.t.gridSize}</span><select value={v.gridSizeVal} onChange={v.onGridSize} style={s(v.ribbonSelectSm)}>{v.gridSizeOpts.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select></div>
            <div style={s(v.rLabel)}>{v.t.spacing}</div>
          </div>
          <div style={s(v.rDiv)} />
          <div style={s(v.rGroup)}>
            <div style={s('display:flex;gap:2px;flex:1;padding-top:8px;flex-wrap:wrap;max-width:150px')}>
              <button onClick={v.alignPageL} title="Align left on page" style={s(v.rBtn)}><svg width="13" height="13" viewBox="0 0 16 16"><path d="M2 2v12" stroke="currentColor" strokeWidth="1.4" /><rect x="4" y="5" width="8" height="6" fill="none" stroke="currentColor" strokeWidth="1.2" /></svg></button>
              <button onClick={v.alignPageC} title="Center on page" style={s(v.rBtn)}><svg width="13" height="13" viewBox="0 0 16 16"><path d="M8 2v12" stroke="currentColor" strokeWidth="1.4" /><rect x="4" y="5" width="8" height="6" fill="none" stroke="currentColor" strokeWidth="1.2" /></svg></button>
              <button onClick={v.alignPageR} title="Align right on page" style={s(v.rBtn)}><svg width="13" height="13" viewBox="0 0 16 16"><path d="M14 2v12" stroke="currentColor" strokeWidth="1.4" /><rect x="4" y="5" width="8" height="6" fill="none" stroke="currentColor" strokeWidth="1.2" /></svg></button>
              <button onClick={v.snapSel} title="Snap to grid" style={s(v.rBtn)}><svg width="13" height="13" viewBox="0 0 16 16"><rect x="4" y="4" width="8" height="8" fill="none" stroke="currentColor" strokeWidth="1.2" /><path d="M2 2h2M12 2h2M2 14h2M12 14h2" stroke="currentColor" strokeWidth="1.2" /></svg></button>
              <button onClick={v.zFront} title="Bring to front" style={s(v.rBtn)}><svg width="13" height="13" viewBox="0 0 16 16"><rect x="5" y="2" width="9" height="9" fill="var(--ribbon,#f7f8fa)" stroke="currentColor" strokeWidth="1.2" /><rect x="2" y="5" width="6" height="6" fill="none" stroke="currentColor" strokeWidth="1" /></svg></button>
              <button onClick={v.zBack} title="Send to back" style={s(v.rBtn)}><svg width="13" height="13" viewBox="0 0 16 16"><rect x="2" y="2" width="9" height="9" fill="none" stroke="currentColor" strokeWidth="1" /><rect x="8" y="8" width="6" height="6" fill="var(--ribbon,#f7f8fa)" stroke="currentColor" strokeWidth="1.2" /></svg></button>
            </div>
            <div style={s(v.rLabel)}>{v.t.arrange}</div>
          </div>
        </>
      )}

      {/* VIEW */}
      {v.isView && (
        <>
          <div style={s(v.rGroup)}>
            <div style={s('display:flex;align-items:center;gap:3px;flex:1;padding-top:8px')}>
              <button onClick={v.zoomOut} title="Zoom out" style={s(v.rBtn)}><span style={s('font-size:16px')}>−</span></button>
              <div style={s("width:48px;text-align:center;font-size:12px;font-weight:600;color:var(--text,#1d2330);font-family:'IBM Plex Mono',monospace")}>{v.zoomPct}</div>
              <button onClick={v.zoomIn} title="Zoom in" style={s(v.rBtn)}><span style={s('font-size:16px')}>+</span></button>
              <button onClick={v.zoom100} style={s(v.rBtnWide)}><span style={s('font-size:11px')}>100%</span></button>
              <button onClick={v.zoomFit} style={s(v.rBtnWide)}><span style={s('font-size:11px')}>{v.t.fit}</span></button>
            </div>
            <div style={s(v.rLabel)}>{v.t.zoom}</div>
          </div>
          <div style={s(v.rDiv)} />
          <div style={s(v.rGroup)}>
            <div style={s('display:flex;flex-direction:column;gap:4px;flex:1;padding-top:8px')}>
              <button onClick={v.toggleRuler} style={s(v.rulerBtnWide)}><span style={s('font-size:11.5px')}>{v.rulerLabel}</span></button>
              <button onClick={v.toggleShowGrid} style={s(v.gridBtn)}><svg width="13" height="13" viewBox="0 0 16 16"><path d="M2 2h12v12H2zM2 6h12M2 10h12M6 2v12M10 2v12" fill="none" stroke="currentColor" strokeWidth="1" /></svg><span style={s('font-size:11px')}>{v.t.showGrid}</span></button>
            </div>
            <div style={s(v.rLabel)}>{v.t.show}</div>
          </div>
          <div style={s(v.rDiv)} />
          <div style={s(v.rGroup)}>
            <div style={s('display:flex;flex-direction:column;gap:4px;flex:1;padding-top:8px')}>
              <button onClick={v.toggleLeftPanel} style={s(v.dataPanelBtn)}><span style={s('font-size:11.5px')}>{v.dataPanelLabel}</span></button>
            </div>
            <div style={s(v.rLabel)}>{v.t.panels}</div>
          </div>
        </>
      )}
    </>
  );
}
