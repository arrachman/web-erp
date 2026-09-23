'use client';

import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';
import type { RsPreviewBand } from '@/lib/report-studio/pagination';

function Band({ bi }: { bi: RsPreviewBand }) {
  return (
    <div style={s(bi.style)}>
      {bi.els.map((el, i) => <div key={i} style={s(el.boxStyle)}>{el.display}</div>)}
    </div>
  );
}

export function RsPreview({ v }: { v: RsVals }) {
  return (
    <div style={s('flex:1;overflow:auto;padding:30px;display:flex;flex-direction:column;align-items:center;gap:24px;min-height:0')}>
      {v.previewPages.map((pg, i) => (
        <div key={i} style={s(v.previewSheetStyle)}>
          <div>{pg.top.map((bi, j) => <Band key={j} bi={bi} />)}</div>
          <div style={s('flex:1')} />
          <div>{pg.bottom.map((bi, j) => <Band key={j} bi={bi} />)}</div>
        </div>
      ))}
    </div>
  );
}
