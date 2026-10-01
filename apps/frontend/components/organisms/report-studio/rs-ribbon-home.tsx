'use client';

import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';

type Swatch = { onClick: () => void; style: string };
function ColorPop({ open, swatches, popStyle }: { open: boolean; swatches: Swatch[]; popStyle: string }) {
  if (!open) return null;
  return <div style={s(popStyle)}>{swatches.map((c, i) => <button key={i} onClick={c.onClick} style={s(c.style)} />)}</div>;
}

export function RsRibbonHome({ v }: { v: RsVals }) {
  return (
    <>
      {/* Clipboard */}
      <div style={s(v.rGroup)}>
        <div style={s('display:flex;align-items:flex-start;gap:4px;flex:1;padding-top:4px')}>
          <button onClick={v.doPaste} title="Paste (Ctrl+V)" style={s(v.pasteBtnStyle)}>
            <svg width="20" height="20" viewBox="0 0 20 20"><rect x="4" y="3" width="12" height="14" rx="1.5" fill="none" stroke="currentColor" strokeWidth="1.3" /><rect x="7" y="1.5" width="6" height="3" rx="1" fill="currentColor" /></svg>
            <span style={s('font-size:10.5px;margin-top:2px')}>{v.t.paste}</span>
          </button>
          <div style={s('display:flex;flex-direction:column;gap:2px')}>
            <button onClick={v.doCut} title="Cut (Ctrl+X)" style={s(v.rBtnWide)}><svg width="13" height="13" viewBox="0 0 16 16"><circle cx="4" cy="11" r="2" fill="none" stroke="currentColor" strokeWidth="1.3" /><circle cx="12" cy="11" r="2" fill="none" stroke="currentColor" strokeWidth="1.3" /><path d="M5.5 9.5L13 2M10.5 9.5L3 2" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" /></svg><span style={s('font-size:10.5px')}>{v.t.cut}</span></button>
            <button onClick={v.doCopy} title="Copy (Ctrl+C)" style={s(v.rBtnWide)}><svg width="13" height="13" viewBox="0 0 16 16"><rect x="5" y="5" width="9" height="9" rx="1.3" fill="none" stroke="currentColor" strokeWidth="1.3" /><path d="M3 11V3a1 1 0 0 1 1-1h7" fill="none" stroke="currentColor" strokeWidth="1.3" /></svg><span style={s('font-size:10.5px')}>{v.t.copy}</span></button>
            <button onClick={v.delSel} title="Delete (Del)" style={s(v.rBtnWide)}><svg width="13" height="13" viewBox="0 0 16 16"><path d="M3 4h10M6 4V2.5h4V4M5 4l.6 9h4.8L11 4" fill="none" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" strokeLinejoin="round" /></svg><span style={s('font-size:10.5px')}>{v.t.delete}</span></button>
          </div>
        </div>
        <div style={s(v.rLabel)}>{v.t.clipboard}</div>
      </div>
      <div style={s(v.rDiv)} />

      {/* Font */}
      <div style={s(v.rGroup)}>
        <div style={s('display:flex;flex-direction:column;gap:4px;flex:1;padding-top:5px')}>
          <div style={s('display:flex;gap:3px')}>
            <select value={v.fontFamilyVal} onChange={v.onFontFamily} style={s(v.ribbonSelectWide)}>{v.fontFamilyOpts.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select>
            <select value={v.fontSizeVal} onChange={v.onFontSizeSel} style={s(v.ribbonSelectSm)}>{v.fontSizeOpts.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select>
          </div>
          <div style={s('display:flex;gap:2px;align-items:center')}>
            <button onClick={v.toggleBold} title="Bold" style={s(v.biBold)}>B</button>
            <button onClick={v.toggleItalic} title="Italic" style={s(v.biItalic + ';font-style:italic')}>i</button>
            <button onClick={v.toggleUnderline} title="Underline" style={s(v.biUnder + ';text-decoration:underline')}>U</button>
            <button onClick={v.toggleStrike} title="Strikethrough" style={s(v.biStrike + ';text-decoration:line-through')}>S</button>
            <div style={s('width:1px;height:18px;background:var(--border,#e1e5ea);margin:0 2px')} />
            <button onClick={v.fontGrow} title="Grow font" style={s(v.rBtn)}><span style={s('font-size:13px;font-weight:700')}>A</span><span style={s('font-size:9px')}>▲</span></button>
            <button onClick={v.fontShrink} title="Shrink font" style={s(v.rBtn)}><span style={s('font-size:10px;font-weight:700')}>A</span><span style={s('font-size:9px')}>▼</span></button>
            <div style={s('position:relative')}>
              <button onClick={v.openFontColor} title="Text color" style={s(v.rBtn + ';flex-direction:column;gap:0')}><span style={s('font-size:12px;font-weight:700;line-height:1')}>A</span><span style={s(v.fontColorBar)} /></button>
              <ColorPop open={v.menuFontColor} swatches={v.swColor} popStyle={v.colorPopStyle} />
            </div>
          </div>
        </div>
        <div style={s(v.rLabel)}>{v.t.font}</div>
      </div>
      <div style={s(v.rDiv)} />

      {/* Alignment */}
      <div style={s(v.rGroup)}>
        <div style={s('display:flex;flex-direction:column;gap:4px;flex:1;padding-top:6px')}>
          <div style={s('display:flex;gap:2px')}>
            <button onClick={v.setVTop} title="Top" style={s(v.vT)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M3 3h10M6 6h4v7H6z" fill="currentColor" stroke="currentColor" strokeWidth=".5" /></svg></button>
            <button onClick={v.setVMid} title="Middle" style={s(v.vM)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M3 8h10M6 4.5h4v7H6z" fill="currentColor" stroke="currentColor" strokeWidth=".5" /></svg></button>
            <button onClick={v.setVBot} title="Bottom" style={s(v.vB)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M3 13h10M6 3h4v7H6z" fill="currentColor" stroke="currentColor" strokeWidth=".5" /></svg></button>
            <div style={s('width:1px;height:18px;background:var(--border,#e1e5ea);margin:0 2px')} />
            <button onClick={v.toggleWrap} title="Word wrap" style={s(v.biWrap)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M2 4h12M2 8h9a2 2 0 1 1 0 4H8M9 10.5L7.5 12 9 13.5M2 12h3" fill="none" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" strokeLinejoin="round" /></svg></button>
          </div>
          <div style={s('display:flex;gap:2px')}>
            <button onClick={v.setAlignL} title="Align left" style={s(v.alL)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M2 4h12M2 7h8M2 10h11M2 13h7" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" /></svg></button>
            <button onClick={v.setAlignC} title="Center" style={s(v.alC)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M2 4h12M4 7h8M3 10h10M5 13h6" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" /></svg></button>
            <button onClick={v.setAlignR} title="Align right" style={s(v.alR)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M2 4h12M6 7h8M3 10h11M7 13h7" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" /></svg></button>
            <button onClick={v.setAlignJ} title="Justify" style={s(v.alJ)}><svg width="14" height="14" viewBox="0 0 16 16"><path d="M2 4h12M2 7h12M2 10h12M2 13h12" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" /></svg></button>
          </div>
        </div>
        <div style={s(v.rLabel)}>{v.t.alignment}</div>
      </div>
      <div style={s(v.rDiv)} />

      {/* Borders */}
      <div style={s(v.rGroup)}>
        <div style={s('display:flex;flex-direction:column;gap:4px;flex:1;padding-top:6px')}>
          <div style={s('display:flex;gap:2px')}>
            <button onClick={v.brdAll} title="All borders" style={s(v.rBtn)}><svg width="14" height="14" viewBox="0 0 16 16"><rect x="2.5" y="2.5" width="11" height="11" fill="none" stroke="currentColor" strokeWidth="1.2" /><path d="M8 2.5v11M2.5 8h11" stroke="currentColor" strokeWidth="1" /></svg></button>
            <button onClick={v.brdBox} title="Outline" style={s(v.rBtn)}><svg width="14" height="14" viewBox="0 0 16 16"><rect x="2.5" y="2.5" width="11" height="11" fill="none" stroke="currentColor" strokeWidth="1.3" /></svg></button>
            <button onClick={v.brdNone} title="No border" style={s(v.rBtn)}><svg width="14" height="14" viewBox="0 0 16 16"><rect x="2.5" y="2.5" width="11" height="11" fill="none" stroke="currentColor" strokeWidth="1" strokeDasharray="2 2" opacity=".5" /></svg></button>
            <div style={s('width:1px;height:18px;background:var(--border,#e1e5ea);margin:0 2px')} />
            <button onClick={v.brdTop} title="Top" style={s(v.bT)}><svg width="14" height="14" viewBox="0 0 16 16"><rect x="2.5" y="2.5" width="11" height="11" fill="none" stroke="currentColor" strokeWidth=".7" opacity=".4" /><path d="M2.5 2.5h11" stroke="currentColor" strokeWidth="1.6" /></svg></button>
            <button onClick={v.brdBottom} title="Bottom" style={s(v.bB)}><svg width="14" height="14" viewBox="0 0 16 16"><rect x="2.5" y="2.5" width="11" height="11" fill="none" stroke="currentColor" strokeWidth=".7" opacity=".4" /><path d="M2.5 13.5h11" stroke="currentColor" strokeWidth="1.6" /></svg></button>
            <button onClick={v.brdLeft} title="Left" style={s(v.bL)}><svg width="14" height="14" viewBox="0 0 16 16"><rect x="2.5" y="2.5" width="11" height="11" fill="none" stroke="currentColor" strokeWidth=".7" opacity=".4" /><path d="M2.5 2.5v11" stroke="currentColor" strokeWidth="1.6" /></svg></button>
            <button onClick={v.brdRight} title="Right" style={s(v.bR)}><svg width="14" height="14" viewBox="0 0 16 16"><rect x="2.5" y="2.5" width="11" height="11" fill="none" stroke="currentColor" strokeWidth=".7" opacity=".4" /><path d="M13.5 2.5v11" stroke="currentColor" strokeWidth="1.6" /></svg></button>
          </div>
          <div style={s('display:flex;gap:3px;align-items:center')}>
            <div style={s('position:relative')}>
              <button onClick={v.openFillColor} title="Fill color" style={s(v.rBtn + ';flex-direction:column;gap:0')}><svg width="13" height="13" viewBox="0 0 16 16"><path d="M3 8l5-5 5 5-5 5z" fill="none" stroke="currentColor" strokeWidth="1.2" /></svg><span style={s(v.fillColorBar)} /></button>
              <ColorPop open={v.menuFillColor} swatches={v.swFill} popStyle={v.colorPopStyle} />
            </div>
            <div style={s('position:relative')}>
              <button onClick={v.openLineColor} title="Border color" style={s(v.rBtn + ';flex-direction:column;gap:0')}><svg width="13" height="13" viewBox="0 0 16 16"><rect x="3" y="3" width="10" height="10" fill="none" stroke="currentColor" strokeWidth="1.4" /></svg><span style={s(v.lineColorBar)} /></button>
              <ColorPop open={v.menuLineColor} swatches={v.swLine} popStyle={v.colorPopStyle} />
            </div>
            <select value={v.lineWidthVal} onChange={v.onLineWidth} title="Border width" style={s(v.ribbonSelectSm)}>{v.lineWidthOpts.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select>
          </div>
        </div>
        <div style={s(v.rLabel)}>{v.t.borders}</div>
      </div>
      <div style={s(v.rDiv)} />

      {/* Text Format */}
      <div style={s(v.rGroup)}>
        <div style={s('display:flex;flex-direction:column;gap:5px;flex:1;padding-top:8px')}>
          <select value={v.fmtVal} onChange={v.onFormat} style={s(v.ribbonSelectWide)}>{v.fmtOpts.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select>
          <div style={s("font-size:10.5px;color:var(--muted,#6b7280);font-family:'IBM Plex Mono',monospace")}>{v.fmtSample}</div>
        </div>
        <div style={s(v.rLabel)}>{v.t.textFormat}</div>
      </div>
      <div style={s(v.rDiv)} />

      {/* Style */}
      <div style={s(v.rGroup)}>
        <div style={s('display:flex;flex-direction:column;gap:5px;flex:1;padding-top:8px')}>
          <select value={v.styleVal} onChange={v.onStyleApply} style={s(v.ribbonSelectWide)}>{v.styleOpts.map((o) => <option key={o.v} value={o.v}>{o.label}</option>)}</select>
          <div style={s('font-size:10.5px;color:var(--muted,#6b7280)')}>{v.t.styleHint}</div>
        </div>
        <div style={s(v.rLabel)}>{v.t.style}</div>
      </div>
    </>
  );
}
